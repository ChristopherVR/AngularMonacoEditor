namespace IntellisenseServer.Models;

public sealed class CompilationResult
{
    public CompilationResult(string[] errors, bool success)
    {
        Errors = errors;
        Success = success;
    }

    public string[] Errors { get; private init; }
    public bool Success { get; private init; }

}
