/* eslint-disable no-unsafe-optional-chaining */
/* eslint-disable @typescript-eslint/no-non-null-assertion */
import { AfterViewInit, Component, ElementRef, forwardRef, Inject, Input, NgZone, OnDestroy, ViewChild } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { fromEvent, Subscription } from 'rxjs';
import { IDisposable, Uri } from 'monaco-editor';
import { lastValueFrom } from 'rxjs';
import { MONACO_EDITOR_CONFIG, MonacoEditorConfig } from '../monaco-editor/config';
import { IntellisenseService } from '../services/intellisense.service';
import { getBaseOptions, registerAmdLoader } from './helper';
import { CodeMarker, ITextModel, MonacoEditor, MonacoEditorOptions } from './types';
import { HoverInfo, SignatureResponse } from './interfaces';
import { getMarkerSeverity } from './enums';

let loadedMonaco = false;
let loadPromise: Promise<void>;
const defaultEditorLanguage = 'csharp';
@Component({
  selector: 'app-base-editor',
  template: '<div class="w-100 h-100" #editorContainer></div>',
  styles: [
    `
      :host {
        display: block;
        height: 12.5rem;
      }
    `,
  ],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => BaseEditorComponent),
      multi: true,
    },
  ],
})
export class BaseEditorComponent implements ControlValueAccessor, AfterViewInit, OnDestroy {
  @ViewChild('editorContainer', { static: true }) underlyingEditorContainer!: ElementRef;
  private underlyingEditor!: MonacoEditor;
  private optionsObject!: MonacoEditorOptions;
  private windowResizeSubscription!: Subscription;
  private customValue = '';
  private id!: string;

  private modelId!: string;
  private modelUri!: Uri;

  private tabCompletionProvider!: IDisposable;
  private signatureProvider!: IDisposable;
  private onModelCreate!: IDisposable;
  private hoverProvider!: IDisposable;
  private onDidMarkersChange!: IDisposable;
  private onDidCreateModel!: IDisposable;
  private modelChangeContentProvider!: IDisposable;

  constructor(
    private zone: NgZone,
    @Inject(MONACO_EDITOR_CONFIG) private config: MonacoEditorConfig,
    private intellisenseService: IntellisenseService,
  ) {}

  get options(): MonacoEditorOptions {
    return this.optionsObject;
  }

  @Input()
  set options(options: MonacoEditorOptions) {
    this.optionsObject = {
      ...this.config.defaultOptions,
      ...options,
    };
    if (this.underlyingEditor) {
      if (this.underlyingEditor.dispose) {
        this.underlyingEditor.dispose();
      }
      this.initMonaco(options);
    }
  }

  @Input()
  set model(model: MonacoEditor | ITextModel) {
    this.options.model = model;
    if (this.underlyingEditor) {
      if (this.underlyingEditor.dispose) {
        this.underlyingEditor.dispose();
      }
      this.initMonaco(this.options);
    }
  }

  @Input()
  set key(key: string) {
    this.id = key;
  }

  propagateChange: (val?: string) => void = () => false;
  onTouched: () => void = () => false;

  ngAfterViewInit(): void {
    if (loadedMonaco) {
      // Wait until monaco editor is available
      loadPromise.then(() => {
        this.registerMonacoEventListeners();
        this.initMonaco(this.optionsObject);
      });
    } else {
      loadedMonaco = true;
      loadPromise = new Promise<void>((resolve: () => void) => {
        const baseUrl = (this.config.baseUrl || './assets') + '/monaco-editor/min/vs';
        if (typeof window.monaco === 'object') {
          resolve();
          return;
        }

        registerAmdLoader(
          baseUrl,
          this.config,
          this.optionsObject,
          () => this.registerMonacoEventListeners(),
          (db: MonacoEditorOptions) => this.initMonaco(db),
          resolve,
        );
      });
    }
  }

  writeValue(value?: string): void {
    this.customValue = value || '';
    // Fix for value change while dispose in process.
    setTimeout(() => {
      if (this.underlyingEditor && !this.options.model) {
        if (this.underlyingEditor.setValue) {
          this.underlyingEditor.setValue(this.customValue);
        }
      }
    });
  }

  registerOnChange(fn: (val?: string) => void): void {
    this.propagateChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  ngOnDestroy() {
    if (this.windowResizeSubscription) {
      this.windowResizeSubscription.unsubscribe();
    }
    if (this.underlyingEditor) {
      if (this.underlyingEditor.dispose) {
        this.underlyingEditor.dispose();
      }
      this.underlyingEditor = undefined!;
    }

    this.tabCompletionProvider?.dispose();
    this.signatureProvider?.dispose();
    this.hoverProvider?.dispose();
    this.onDidCreateModel?.dispose();
    this.onDidMarkersChange?.dispose();
    this.onModelCreate?.dispose();
    this.modelChangeContentProvider?.dispose();
  }

  registerMonacoEventListeners = () => {
    this.tabCompletionProvider = window.monaco!.languages.registerCompletionItemProvider(defaultEditorLanguage, {
      triggerCharacters: ['.', ' '],
      provideCompletionItems: async (model: ITextModel, position) => {
        if (model.id !== this.modelId) {
          return {
            suggestions: [],
          };
        }
        const suggestions = [];

        const request = {
          position: model.getOffsetAt(position),
          code: model.getValue(),
        };

        const completions = await lastValueFrom(this.intellisenseService.valdiateTabCodeCompletion(request, this.id));

        for (const elem of completions) {
          suggestions.push({
            label: {
              label: elem.suggestion,
              description: elem.description,
            },
            kind: window.monaco!.languages.CompletionItemKind.Method,
            insertText: elem.suggestion,
            range: undefined!,
          });
        }

        return { suggestions };
      },
    });

    this.signatureProvider = window.monaco!.languages.registerSignatureHelpProvider(defaultEditorLanguage, {
      signatureHelpTriggerCharacters: ['('],
      signatureHelpRetriggerCharacters: [','],

      provideSignatureHelp: async (model: ITextModel, position) => {
        if (model.id !== this.modelId) {
          return;
        }
        const request = {
          code: model.getValue(),
          position: model.getOffsetAt(position),
        };
        const result: SignatureResponse = await lastValueFrom(this.intellisenseService.validateSignatureInformation(request, this.id));

        if (!result) {
          return {
            value: {
              signatures: [],
              activeParameter: -1,
              activeSignature: -1,
            },
            dispose: () => this.signatureProvider.dispose(),
          };
        }

        const signatures = result.signatures.map((s) => ({
          label: s.label,
          documentation: s.documentation ?? '',
          parameters: s.parameters.map((p) => ({
            label: p.label,
            documentation: p.documentation ?? '',
          })),
        }));

        const signatureHelp = {
          signatures,
          activeSignature: result.activeSignatureIndex,
          activeParameter: result.activeParameterIndex,
        };

        return {
          value: signatureHelp,
          dispose: () => this.signatureProvider.dispose(),
        };
      },
    });

    this.hoverProvider = window.monaco!.languages.registerHoverProvider(defaultEditorLanguage, {
      provideHover: async (model: ITextModel, position) => {
        if (model.id !== this.modelId) {
          return;
        }
        const request = {
          code: model.getValue(),
          position: model.getOffsetAt(position),
        };
        const result: HoverInfo = await lastValueFrom(this.intellisenseService.validateHoverInformation(request, this.id));

        if (!result) {
          return null;
        }
        const posStart = model.getPositionAt(result.spanStart);
        const posEnd = model.getPositionAt(result.spanEnd);

        return {
          range: new window.monaco!.Range(posStart.lineNumber, posStart.column, posEnd.lineNumber, posEnd.column),
          contents: [{ value: result.information }],
        };
      },
    });

    // this.onDidMarkersChange = window.monaco!.editor.onDidChangeMarkers((e) => {
    //   const uri = e.find((y) => y === this.modelUri);
    //   if (uri) {
    //     const markers = window.monaco!.editor.getModelMarkers({}).filter((y) => y.resource === this.modelUri);
    //   }
    // });

    this.onDidCreateModel = window.monaco!.editor.onDidCreateModel(async (model) => {
      await this.validateModel(model);
    });
  };

  private async validateModel(model: ITextModel) {
    const validate = async () => {
      if (model.id !== this.modelId) {
        return;
      }
      const request = {
        position: 0,
        code: model.getValue(),
      };
      if (!request.code) {
        return true;
      }

      const items = await lastValueFrom(this.intellisenseService.analyseCode(request, this.id));

      const markers: CodeMarker[] = items.map((elem) => ({
        severity: getMarkerSeverity(elem.severity),
        startLineNumber: model.getPositionAt(elem.sourceSpanStart).lineNumber,
        startColumn: model.getPositionAt(elem.sourceSpanStart).column,
        endLineNumber: model.getPositionAt(elem.sourceSpanEnd).lineNumber,
        endColumn: model.getPositionAt(elem.sourceSpanEnd).column,
        message: elem.message,
        code: elem.id,
      }));

      window.monaco!.editor.setModelMarkers(model, this.id, markers);

      return true;
    };

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    let handle: any;
    this.modelChangeContentProvider = model.onDidChangeContent(() => {
      if (model.id !== this.modelId) {
        return;
      }
      window.monaco!.editor.setModelMarkers(model, this.id, []);
      if (handle) {
        clearTimeout(handle);
      }
      handle = setTimeout(() => validate(), 500);
    });
    validate();
  }

  private async initMonaco(options: MonacoEditorOptions): Promise<void> {
    const hasModel = !!this.modelId;
    if (hasModel || !!this.options.model) {
      const model = window.monaco!.editor.getModels().find((y) => y.id === this.modelId);
      if (model) {
        options.model = model;
        options.model.setValue(this.customValue);
      } else {
        const language: string | undefined = options.model && 'language' in options.model ? options.model.language : 'csharp';
        const value: string = options.model && 'value' in options.model ? options.model.value ?? '' : '';

        options.model = window.monaco!.editor.createModel(value, language);

        this.modelId = options.model.id;
        this.modelUri = options.model.uri;
        await this.validateModel(options.model);
      }
    }

    this.underlyingEditor = window.monaco!.editor.create(this.underlyingEditorContainer.nativeElement, {
      ...getBaseOptions(options),
      suggest: {
        showKeywords: false,
        showDeprecated: false,
      },
    });
    if (!hasModel) {
      if (this.underlyingEditor.setValue) {
        this.underlyingEditor.setValue(this.customValue);
      }
    }

    if (this.underlyingEditor.onDidChangeModelContent) {
      this.underlyingEditor.onDidChangeModelContent(() => {
        const value = this.underlyingEditor.getValue ? this.underlyingEditor.getValue() : '';

        // value is not propagated to parent when executing outside zone.
        this.zone.run(() => {
          this.propagateChange(value);
          this.customValue = value;
        });
      });
    }

    if (this.underlyingEditor.onDidBlurEditorWidget) {
      this.underlyingEditor.onDidBlurEditorWidget(() => {
        this.onTouched();
      });
    }

    // refresh layout on resize event.
    if (this.windowResizeSubscription) {
      this.windowResizeSubscription.unsubscribe();
    }
    this.windowResizeSubscription = fromEvent(window, 'resize').subscribe(() => {
      if (this.underlyingEditor.layout) {
        this.underlyingEditor.layout();
      }
    });
  }
}
