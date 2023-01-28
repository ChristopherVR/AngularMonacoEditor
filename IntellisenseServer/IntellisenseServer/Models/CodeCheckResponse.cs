using IntellisenseServer.Interfaces;
using Microsoft.CodeAnalysis;

namespace IntellisenseServer.Models;


public sealed class CodeCheckResponse : IResponse
{
    public string Id { get; private init; }

    public string Keyword { get; private init; }

    public string Message { get; private init; }

    public int OffsetFrom { get; private init; }

    public int OffsetTo { get; private init; }

    public DiagnosticSeverity Severity { get; private init; }

    public CodeCheckResponse(string id, string keyword, string message, int offsetFrom, int offsetTo, DiagnosticSeverity severity)
    {
        Id = id;
        Keyword = keyword;
        Message = message;
        OffsetFrom = offsetFrom;
        OffsetTo = offsetTo;
        Severity = severity;
    }
}
