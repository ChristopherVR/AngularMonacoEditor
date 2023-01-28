using IntellisenseServer.Interfaces;

namespace IntellisenseServer.Models;

public sealed class TabCompletionResponse : IResponse
{
    public string Suggestion { get; private init; }

    public string? Description { get; private init; }

    public TabCompletionResponse(string information, string? description)
    {
        Suggestion = information;
        Description = description;
    }
}
