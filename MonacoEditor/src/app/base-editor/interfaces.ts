import { DiagnosticServerity } from './enums';

export interface Parameter {
  label: string;
  documentation: string;
}

export interface Signature {
  label: string;
  documentation?: string;
  parameters: Parameter[];
}

export interface SignatureResponse {
  signatures: Signature[];
  activeParameterIndex: number;
  activeSignatureIndex: number;
}

export interface CodeCheck {
  id: string;
  message: string;
  startLine: number;
  endLine: number;
  sourceSpanStart: number;
  sourceSpanEnd: number;
  sourceSpanLength: number;
  severity: DiagnosticServerity;
}

export interface HoverInfo {
  information: string;
  spanStart: number;
  spanEnd: number;
}

export interface TabCompletion {
  suggestion: string;
  description: string;
}

export interface MonacoRequest {
  code: string;
  position?: number;
}
