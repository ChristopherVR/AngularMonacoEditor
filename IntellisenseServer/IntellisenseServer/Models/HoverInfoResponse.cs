using IntellisenseServer.Interfaces;

namespace IntellisenseServer.Models;

public sealed class HoverInfoResponse : IResponse
{
    public string? Information { get; private init; }

    public int OffsetFrom { get; private init; }

    public int OffsetTo { get; private init; }

    public HoverInfoResponse(string? information, int offsetFrom, int offsetTo)
    {
        Information = information;
        OffsetFrom = offsetFrom;
        OffsetTo = offsetTo;
    }
}
