using IntellisenseServer.Models;
using IntellisenseServer.Providers.Providers;
using IntellisenseServer.Providers.Providers.Interfaces;
using IntellisenseServer.Services.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using System.Reflection;

namespace IntellisenseServer.Services;

public sealed class IntellisenseCodeService : IIntellisenseCodeService
{
    public async Task<TabCompletionResponse[]> HandleAsync(TabCompletionRequest request, CancellationToken cancellationToken = default)
    {
        var (Document, _) = CreateDocumentWithReferences(request.Code, request.Assemblies);

        var completionService = CompletionService.GetService(Document)!;
        var results = await completionService.GetCompletionsAsync(Document, request.Position, cancellationToken: cancellationToken);

        var tabCompletionDTOs = new TabCompletionResponse[results.ItemsList.Count];

        if (results is null)
        {
            return Array.Empty<TabCompletionResponse>();
        }

        var suggestions = new string[results.ItemsList.Count];

        for (int i = 0; i < results.ItemsList.Count; i++)
        {
            var itemDescription = await completionService.GetDescriptionAsync(Document, results.ItemsList[i], cancellationToken);

            var dto = new TabCompletionResponse(results.ItemsList[i].DisplayText, itemDescription?.Text);
            tabCompletionDTOs[i] = dto;
            suggestions[i] = results.ItemsList[i].DisplayText;
        }

        return tabCompletionDTOs;
    }

    /// <summary>
    /// Returns information about the signature for the given method or constructor.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<SignatureResponse?> HandleAsync(SignatureRequest request, CancellationToken cancellationToken = default)
    {
        var (Document, References) = CreateDocumentWithReferences(request.Code, request.Assemblies);

        var compilation = await GetCSharpCompilationAsync(Document, References);

        var semanticModel = GetSemanticModel(compilation.SyntaxTree, compilation.Compilation);

        IProvider provider = new SignatureHelpProvider();

        var result = await provider.ProvideAsync(Document, request.Position, semanticModel, cancellationToken: cancellationToken);

        if (result is not SignatureResponse signature)
        {
            return null;
        }

        return signature;
    }

    /// <summary>
    /// Returns information regarding the current position in the code the user is located in.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<HoverInfoResponse?> HandleAsync(HoverInfoRequest request, CancellationToken cancellationToken = default)
    {
        var (Document, References) = CreateDocumentWithReferences(request.Code, request.Assemblies);

        var compilation = await GetCSharpCompilationAsync(Document, References);

        var semanticModel = GetSemanticModel(compilation.SyntaxTree, compilation.Compilation);

        IProvider provider = new HoverInformationProvider();

        var result = await provider.ProvideAsync(Document, request.Position, semanticModel, cancellationToken: cancellationToken);

        if (result is not HoverInfoResponse hover)
        {
            return null;
        }

        return hover;
    }

    /// <summary>
    /// Returns a list of code analysis response. This indicates whether there are any severities that needs to be addressed.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<CodeCheckResponse[]> HandleAsync(CodeCheckRequest request, CancellationToken cancellationToken = default)
    {
        var (Document, References) = CreateDocumentWithReferences(request.Code, request.Assemblies);

        var compilation = await GetCSharpCompilationAsync(Document, References);

        var emitResult = GetEmitResult(compilation.Compilation);

        var result = new List<CodeCheckResponse>();
        foreach (var r in emitResult.Diagnostics)
        {
            var keyword = (await Document.GetTextAsync(cancellationToken)).GetSubText(r.Location.SourceSpan).ToString();

            var msg = new CodeCheckResponse(
                r.Id,
                keyword,
                r.GetMessage(),
                r.Location.SourceSpan.Start,
                r.Location.SourceSpan.End,
                r.Severity);

            result.Add(msg);
        }

        return result.ToArray();
    }

    /// <summary>
    /// Returns a <see cref="Document"/> with the list of <see cref="MetadataReference"/> used in it. The document is the source code for the input the user provided in the editor.
    /// </summary>
    /// <param name="code"></param>
    /// <param name="assemblies"></param>
    /// <returns></returns>
    private static (Document Document, List<MetadataReference> References) CreateDocumentWithReferences(string code, params string[] assemblies)
    {
        Assembly[] lst = new[]
        {
            Assembly.Load("Microsoft.CodeAnalysis.Workspaces"),
            Assembly.Load("Microsoft.CodeAnalysis.CSharp.Workspaces"),
            Assembly.Load("Microsoft.CodeAnalysis.Features"),
            Assembly.Load("Microsoft.CodeAnalysis.CSharp.Features"),
        };

        var host = MefHostServices.Create(lst);
        var workspace = new AdhocWorkspace(host);

        List<MetadataReference> references = AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .Cast<MetadataReference>()
            .ToList();

        if (assemblies is not null && assemblies.Length > 0)
        {
            for (int i = 0; i < assemblies.Length; i++)
            {
                references.Add(MetadataReference.CreateFromFile(assemblies[i]));
            }
        }

        var projectInfo = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), "TempProject", "TempProject", LanguageNames.CSharp)
            .WithMetadataReferences(references);

        var project = workspace.AddProject(projectInfo);

        var document = workspace.AddDocument(project.Id, "CodeIntellisenseTemp.cs", SourceText.From(code));

        return (document, references);
    }

    private static async Task<(CSharpCompilation Compilation, SyntaxTree SyntaxTree)> GetCSharpCompilationAsync(Document document, List<MetadataReference> references)
    {
        var st = await document.GetSyntaxTreeAsync()!;

        var compilation =
        CSharpCompilation
            .Create("TempResult",
                new[] { st! },
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                references: references
            );

        return (compilation, st!);
    }

    private static EmitResult GetEmitResult(CSharpCompilation compilation)
        => compilation.Emit("TempResult");

    private static SemanticModel GetSemanticModel(SyntaxTree st, CSharpCompilation compilation)
        => compilation.GetSemanticModel(st, true);
}
