using IntellisenseServer.Interfaces;
using IntellisenseServer.Models;
using IntellisenseServer.Providers.Providers.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace IntellisenseServer.Providers.Providers;

/// <summary>
/// The <see cref="SignatureHelpProvider"/> is responsible for determing the best activate parameter for the given <see cref="SemanticModel"/> model.
/// </summary>
internal class SignatureHelpProvider : IProvider
{
    public async Task<IResponse?> ProvideAsync(Document document, int position, SemanticModel semanticModel, CancellationToken cancellationToken = default)
    {
        var invocation = await InvokeContext.GetInvocation(document, position, cancellationToken);
        if (invocation is null)
        {
            return null;
        }

        int activeParameter = 0;
        foreach (var comma in invocation.Separators!)
        {
            if (comma.Span.Start > invocation.Position)
                break;

            activeParameter += 1;
        }

        var signaturesSet = new HashSet<Signature>();
        var bestScore = int.MinValue;
        Signature? bestScoredItem = null;

        var types = invocation.ArgumentTypes;
        ISymbol? throughSymbol = null;
        ISymbol? throughType = null;
        var methodGroup = invocation.SemanticModel.GetMemberGroup(invocation.Receiver, cancellationToken).OfType<IMethodSymbol>();
        if (invocation.Receiver is MemberAccessExpressionSyntax syntax)
        {
            var throughExpression = syntax.Expression;
            var typeInfo = semanticModel.GetTypeInfo(throughExpression, cancellationToken);
            throughSymbol = invocation.SemanticModel.GetSpeculativeSymbolInfo(invocation.Position, throughExpression, SpeculativeBindingOption.BindAsExpression).Symbol;
            throughType = invocation.SemanticModel.GetSpeculativeTypeInfo(invocation.Position, throughExpression, SpeculativeBindingOption.BindAsTypeOrNamespace).Type;
            var includeInstance = throughSymbol is not null && throughSymbol is not ITypeSymbol ||
                throughExpression is LiteralExpressionSyntax or TypeOfExpressionSyntax;
            var includeStatic = throughSymbol is INamedTypeSymbol || throughType is not null;
            if (throughType is null)
            {
                throughType = typeInfo.Type;
                includeInstance = true;
            }
            methodGroup = methodGroup.Where(m => m.IsStatic && includeStatic || !m.IsStatic && includeInstance);
        }
        else if (invocation.Receiver is SimpleNameSyntax && invocation.IsInStaticContext)
        {
            methodGroup = methodGroup.Where(m => m.IsStatic || m.MethodKind is MethodKind.LocalFunction);
        }

        // Attempt to get the best scored item (Signature) that will be provided for the user.
        foreach (var methodOverload in methodGroup)
        {
            var signature = BuildSignature(methodOverload);
            signaturesSet.Add(signature);

            var score = InvocationScore(methodOverload, types);
            if (score > bestScore)
            {
                bestScore = score;
                bestScoredItem = signature;
            }
        }

        return new SignatureResponse(activeParameter, Array.IndexOf(signaturesSet.ToArray(), bestScoredItem), signaturesSet.ToArray());
    }

    private static Signature BuildSignature(IMethodSymbol symbol)
    {
        var signature = new Signature(
            symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
            symbol.GetDocumentationCommentXml(),
            symbol.Parameters.Select(paramater => new Parameter(paramater.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat))).ToArray());

        return signature;
    }

    /// <summary>
    /// Calculate the maximum score for this symbol.
    /// </summary>
    /// <param name="symbol"></param>
    /// <param name="types"></param>
    /// <returns></returns>
    private static int InvocationScore(IMethodSymbol symbol, IEnumerable<TypeInfo>? types)
    {
        var parameters = symbol.Parameters;
        if (parameters.Length < types?.Count())
            return int.MinValue;

        var score = 0;
        var invocationEnum = types!.GetEnumerator();
        var definitionEnum = parameters.GetEnumerator();
        while (invocationEnum.MoveNext() && definitionEnum.MoveNext())
        {
            if (invocationEnum.Current.ConvertedType is null)
            {
                score += 1;
            }


            else if (SymbolEqualityComparer.Default.Equals(invocationEnum.Current.ConvertedType, definitionEnum.Current.Type))
            {
                score += 2;
            }

        }
        return score;
    }
}
