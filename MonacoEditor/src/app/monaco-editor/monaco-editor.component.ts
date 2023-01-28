import { Component, ElementRef, Input, ViewChild } from '@angular/core';
import { MonacoEditorOptions, NgxEditorModel } from '../base-editor/types';

@Component({
  selector: 'app-monaco-editor',
  templateUrl: './monaco-editor.component.html',
  styleUrls: ['./monaco-editor.component.scss'],
})
export class MonacoEditorComponent {
  @Input() label: string | undefined;
  @ViewChild('editorContainer', { static: true }) editorContainer: ElementRef | undefined;
  editorOptions: MonacoEditorOptions = {
    theme: 'vs',
    language: 'csharp',
    // disables the dropdown menu when right-clicking
    contextmenu: false,
    model: {
      language: 'csharp',
    },
  };
  code = `using System;
  public class DefaultClass 
  {
      public void DefaultMethod() 
      {
          Console.WriteLine("Hello world!");
      }
  }`;

  model: NgxEditorModel = {
    value: 'chsarp',
    language: 'csharp',
  };
}
