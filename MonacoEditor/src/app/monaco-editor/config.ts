import { InjectionToken } from '@angular/core';
import { MonacoEditorOptions } from '../base-editor/types';

export interface MonacoEditorConfig {
  baseUrl?: string;
  defaultOptions?: MonacoEditorOptions;
  onMonacoLoad?: () => void;
}

export const MONACO_EDITOR_CONFIG = new InjectionToken('MONACO_EDITOR_CONFIG');
