export interface DiffEditorModel {
  code: string;
  language: string;
}
export interface NgxEditorModel {
  value: string;
  language?: string;
  uri?: string;
  setValue?: (c?: string) => void;
}

/* eslint-disable @typescript-eslint/naming-convention */
import { editor, Uri } from 'monaco-editor';

export type Monaco = typeof import('monaco-editor');

export type MonacoEditor = Partial<editor.IStandaloneCodeEditor> & {
  value?: string;
  language?: string;
  uri?: Uri;
};

export type ITextModel = editor.ITextModel;

export type MonacoEditorOptions = Omit<editor.IStandaloneEditorConstructionOptions, 'model'> & {
  model?: MonacoEditor | ITextModel;
  easter?: boolean;
  theme?: 'vs' | 'vs-dark' | 'nord' | 'github' | 'github-light' | 'github-dark' | 'twilight';
};

export type CodeMarker = editor.IMarkerData;

// export type Position = editor.Position
declare global {
  interface Window {
    monaco?: Monaco;
  }
}
