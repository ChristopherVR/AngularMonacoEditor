using IntellisenseServer.Models;

namespace IntellisenseServer.Services.Interfaces;
public interface IIntellisenseCodeService
{
    Task<TabCompletionResponse[]> HandleAsync(TabCompletionRequest request, CancellationToken cancellationToken = default);
    Task<SignatureResponse?> HandleAsync(SignatureRequest request, CancellationToken cancellationToken = default);
    Task<HoverInfoResponse?> HandleAsync(HoverInfoRequest request, CancellationToken cancellationToken = default);
    Task<CodeCheckResponse[]> HandleAsync(CodeCheckRequest request, CancellationToken cancellationToken = default);
}
