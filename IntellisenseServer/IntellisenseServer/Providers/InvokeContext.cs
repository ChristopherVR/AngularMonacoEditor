using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace IntellisenseServer.Providers;

public sealed class InvokeContext
{
    public SemanticModel SemanticModel { get; private init; }
    public int Position { get; private init; }
    public SyntaxNode Receiver { get; private init; }
    public IEnumerable<TypeInfo>? ArgumentTypes { get; private init; }
    public IEnumerable<SyntaxToken>? Separators { get; private init; }
    public bool IsInStaticContext { get; }

    public InvokeContext(SemanticModel semModel, int position, SyntaxNode receiver, ArgumentListSyntax? argList)
    {
        SemanticModel = semModel;
        Position = position;
        Receiver = receiver;
        ArgumentTypes = argList?.Arguments.Select(argument => semModel.GetTypeInfo(argument.Expression));
        Separators = argList?.Arguments.GetSeparators();
    }

    public InvokeContext(SemanticModel semModel, int position, SyntaxNode receiver, AttributeArgumentListSyntax? argList)
    {
        SemanticModel = semModel;
        Position = position;
        Receiver = receiver;
        ArgumentTypes = argList?.Arguments.Select(argument => semModel.GetTypeInfo(argument.Expression));
        Separators = argList?.Arguments.GetSeparators();
    }

    public static async Task<InvokeContext?> GetInvocation(Document document, int position, CancellationToken cancellationToken = default)
    {
        var tree = await document.GetSyntaxTreeAsync(cancellationToken);
        var root = await tree!.GetRootAsync(cancellationToken);
        var node = root.FindToken(position).Parent;

        while (node is not null)
        {
            if (node is InvocationExpressionSyntax invocation && invocation.ArgumentList.Span.Contains(position))
            {
                var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
                if (semanticModel is null)
                {
                    return null;
                }
                return new(semanticModel, position, invocation.Expression, invocation.ArgumentList);
            }

            if (node is BaseObjectCreationExpressionSyntax objectCreation && (objectCreation.ArgumentList?.Span.Contains(position) ?? false))
            {
                var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
                if (semanticModel is null)
                {
                    return null;
                }
                return new(semanticModel, position, objectCreation, objectCreation.ArgumentList);
            }

            if (node is AttributeSyntax attributeSyntax && (attributeSyntax.ArgumentList?.Span.Contains(position) ?? false))
            {
                var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
                if (semanticModel is null)
                {
                    return null;
                }
                return new(semanticModel, position, attributeSyntax, attributeSyntax.ArgumentList);
            }

            node = node.Parent;
        }

        return null;
    }
}
