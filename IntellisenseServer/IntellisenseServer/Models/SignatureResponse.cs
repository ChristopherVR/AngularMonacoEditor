using IntellisenseServer.Interfaces;

namespace IntellisenseServer.Models;

public sealed class SignatureResponse : IResponse
{
    public int ActiveParameter { get; private init; }
    public int ActiveSignature { get; private init; }
    public Signature[] Signatures { get; private init; }
    public SignatureResponse(int activeParameter, int activeSignature, Signature[] signatures)
    {
        ActiveParameter = activeParameter;
        ActiveSignature = activeSignature;
        Signatures = signatures;
    }
}
