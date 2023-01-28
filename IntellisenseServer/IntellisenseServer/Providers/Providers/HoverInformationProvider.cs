using IntellisenseServer.Interfaces;
using IntellisenseServer.Models;
using IntellisenseServer.Providers.Providers.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace IntellisenseServer.Providers.Providers;

internal class HoverInformationProvider : IProvider
{
    /// <summary>
    /// Provides information about the <see cref="SemanticModel"/> when the system attempts to hover over code.
    /// </summary>
    /// <param name="document"></param>
    /// <param name="position"></param>
    /// <param name="semanticModel"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IResponse?> ProvideAsync(Document document, int position, SemanticModel semanticModel, CancellationToken cancellationToken = default)
    {
        TypeInfo typeInfo;
        var syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken: cancellationToken);

        var expressionNode = syntaxRoot?.FindToken(position).Parent;
        if (expressionNode is VariableDeclaratorSyntax)
        {
            SyntaxNode? childNode = expressionNode.ChildNodes()?.FirstOrDefault()?.ChildNodes()?.FirstOrDefault();
            if (childNode is null)
            {
                return null;
            }
            typeInfo = semanticModel.GetTypeInfo(childNode, cancellationToken: cancellationToken);
            var location = expressionNode.GetLocation();
            return new HoverInfoResponse(typeInfo.Type?.ToString(), location.SourceSpan.Start, location.SourceSpan.End);
        }

        if (expressionNode is PropertyDeclarationSyntax prop)
        {
            var location = expressionNode.GetLocation();
            return new HoverInfoResponse(prop.Type.ToString(), location.SourceSpan.Start, location.SourceSpan.End);
        }

        if (expressionNode is ParameterSyntax p)
        {
            var location = expressionNode.GetLocation();
            return new HoverInfoResponse(p.Type?.ToString(), location.SourceSpan.Start, location.SourceSpan.End);
        }

        if (expressionNode is IdentifierNameSyntax i)
        {
            var location = expressionNode.GetLocation();
            typeInfo = semanticModel.GetTypeInfo(i, cancellationToken: cancellationToken);
            if (typeInfo.Type is not null)
                return new HoverInfoResponse(typeInfo.Type.ToString(), location.SourceSpan.Start, location.SourceSpan.End);
        }

        if (expressionNode is null)
        {
            return null;
        }

        var symbolInfo = semanticModel.GetSymbolInfo(expressionNode, cancellationToken: cancellationToken);

        if (symbolInfo.Symbol is not null)
        {
            var location = expressionNode.GetLocation();
            return new HoverInfoResponse(Build(symbolInfo), location.SourceSpan.Start, location.SourceSpan.End); ;
        }

        return null;
    }

    /// <summary>
    /// Creates a <see cref="string"/> to represent the body information for the given <see cref="SymbolInfo"/>.
    /// </summary>
    /// <param name="symbolInfo"></param>
    /// <returns></returns>
    private static string Build(SymbolInfo symbolInfo)
    {
        if (symbolInfo.Symbol is IMethodSymbol methodsymbol)
        {
            var sb = new StringBuilder().Append("(method) ").Append(methodsymbol.DeclaredAccessibility.ToString().ToLower()).Append(' ');
            if (methodsymbol.IsStatic)
                sb.Append("static").Append(' ');
            sb.Append(methodsymbol.Name).Append('(');
            for (var i = 0; i < methodsymbol.Parameters.Length; i++)
            {
                sb.Append(methodsymbol.Parameters[i].Type).Append(' ').Append(methodsymbol.Parameters[i].Name);
                if (i < methodsymbol.Parameters.Length - 1) sb.Append(", ");
            }
            sb.Append(") : ");
            sb.Append(methodsymbol.ReturnType).ToString();
            return sb.ToString();
        }
        if (symbolInfo.Symbol is ILocalSymbol localsymbol)
        {
            var sb = new StringBuilder().Append(localsymbol.Name).Append(" : ");
            if (localsymbol.IsConst)
                sb.Append("const").Append(' ');
            sb.Append(localsymbol.Type);
            return sb.ToString();
        }
        if (symbolInfo.Symbol is IFieldSymbol fieldSymbol)
        {
            var sb = new StringBuilder().Append(fieldSymbol.Name).Append(" : ").Append(fieldSymbol.DeclaredAccessibility.ToString().ToLower()).Append(' ');
            if (fieldSymbol.IsStatic)
                sb.Append("static").Append(' ');
            if (fieldSymbol.IsReadOnly)
                sb.Append("readonly").Append(' ');
            if (fieldSymbol.IsConst)
                sb.Append("const").Append(' ');
            sb.Append(fieldSymbol.Type).ToString();
            return sb.ToString();
        }

        return string.Empty;
    }
}
