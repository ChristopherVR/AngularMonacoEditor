using IntellisenseServer.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using DynamicSystem.V1;
using Grpc.Core;
using IntellisenseServer.Models;
using SignatureResponse = DynamicSystem.V1.SignatureResponse;

namespace IntellisenseServer.API.gRPC;

[Authorize]
public class DynamicServiceV1 : Dynamic.DynamicBase
{
    private readonly IIntellisenseCodeService _intellisenseCodeService;

    public DynamicServiceV1(ILogger<DynamicServiceV1> logger, IIntellisenseCodeService intellisenseCodeService)
        => _intellisenseCodeService = intellisenseCodeService ?? throw new ArgumentNullException(nameof(intellisenseCodeService));

    public override async Task<ListTabCompletionsResponse> ListTabCompletions(ListTabCompletionsRequest request, ServerCallContext context)
    {
        var result = await _intellisenseCodeService.HandleAsync(new TabCompletionRequest(request.Code, request.Position, request.Assemblies.ToArray()), context.CancellationToken);

        return new()
        {
            Tabs =
            {
                result.Select(z => new ListTabCompletionsResponse.Types.TabCompletion()
                {
                    Description = z.Description,
                    Suggestion = z.Suggestion,
                }),
            },
        };
    }

    public override async Task<ListCodeCompletionsResponse> ListCodeCompletions(ListCodeCompletionsRequest request, ServerCallContext context)
    {
        var result = await _intellisenseCodeService.HandleAsync(new CodeCheckRequest(request.Code, Position: default, request.Assemblies.ToArray()), context.CancellationToken);

        return new()
        {
            Checks =
            {
                result.Select(z => new ListCodeCompletionsResponse.Types.CodeCheckCompletion
                {
                    Id = z.Id,
                    Keyword = z.Keyword,
                    Message = z.Message,
                    OffsetFrom = z.OffsetFrom,
                    OffsetTo = z.OffsetTo,
                    Severity = (DiagnosticSeverity) (int)z.Severity,
                }),
            },
        };
    }


    public override async Task<SignatureResponse> ValidateSignature(ValidateSignatureRequest request, ServerCallContext context)
    {
        var result = await _intellisenseCodeService.HandleAsync(new SignatureRequest(request.Code, request.Position, request.Assemblies.ToArray()), context.CancellationToken);

        if (result is null)
        {
            return new();
        }

        return new()
        {
            ActiveParameter = result.ActiveParameter,
            ActiveSignature = result.ActiveSignature,
            Signatures =
            {
                result.Signatures.Select(z => new DynamicSystem.V1.Signature()
                {
                    Documentation = z.Documentation,
                    Label = z.Label,
                    Parameters =
                    {
                        z.Parameters.Select(y => new DynamicSystem.V1.Signature.Types.Parameter()
                        {
                            Label = y.Label,
                            Documentation = y.Documentation,
                        }),
                    },
                }),
            },
        };
    }


    public override async Task<HoverInformation> ValidateHoverInformation(ValidateHoverInformationRequest request, ServerCallContext context)
    {
        var result = await _intellisenseCodeService.HandleAsync(new HoverInfoRequest(request.Code, request.Position, request.Assemblies.ToArray()), context.CancellationToken);

        if (result is null)
        {
            return new();
        }

        return new()
        {
            Information = result.Information,
            OffsetFrom = result.OffsetFrom,
            OffsetTo = result.OffsetTo,
        };
    }
}
