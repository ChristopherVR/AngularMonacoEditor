using IntellisenseServer.Interfaces;
using Microsoft.CodeAnalysis;

namespace IntellisenseServer.Providers.Providers.Interfaces;

public interface IProvider
{
    Task<IResponse?> ProvideAsync(Document document, int position, SemanticModel semanticModel, CancellationToken cancellationToken = default);
}
