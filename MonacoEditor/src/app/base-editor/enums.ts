/* eslint-disable @typescript-eslint/naming-convention */
export enum MarkerSeverity {
  Hint = 1,
  Info = 2,
  Warning = 4,
  Error = 8,
}

export enum DiagnosticServerity {
  /// <summary>
  /// Something that is an issue, as determined by some authority,
  /// but is not surfaced through normal means.
  /// There may be different mechanisms that act on these issues.
  /// </summary>
  Hidden = 0,

  /// <summary>
  /// Information that does not indicate a problem (i.e. not prescriptive).
  /// </summary>
  Info = 1,

  /// <summary>
  /// Something suspicious but allowed.
  /// </summary>
  Warning = 2,

  /// <summary>
  /// Something not allowed by the rules of the language or other authority.
  /// </summary>
  Error = 3,
}

export const getMarkerSeverity = (severity: DiagnosticServerity) => {
  switch (severity) {
    case DiagnosticServerity.Hidden: {
      return MarkerSeverity.Error;
    }
    case DiagnosticServerity.Info: {
      return MarkerSeverity.Info;
    }
    case DiagnosticServerity.Warning: {
      return MarkerSeverity.Warning;
    }
    case DiagnosticServerity.Error: {
      return MarkerSeverity.Error;
    }
    default: {
      return MarkerSeverity.Hint;
    }
  }
};
