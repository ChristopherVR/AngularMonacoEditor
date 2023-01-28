using IntellisenseServer.Models;
using IntellisenseServer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IntellisenseServer.API.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class DynamicModuleController : ControllerBase
{
    private readonly IIntellisenseCodeService _intellisenseCodeService;
    public DynamicModuleController(IIntellisenseCodeService intellisenseCodeService)
    {
        _intellisenseCodeService = intellisenseCodeService ?? throw new ArgumentNullException(nameof(intellisenseCodeService));
    }

    [HttpPost("tab")]
    public async Task<TabCompletionResponse[]> CheckTabCompletionAsync(TabCompletionRequest request)
    {
        var result = await _intellisenseCodeService.HandleAsync(request);

        return result;
    }

    [HttpPost("hover")]
    public async Task<HoverInfoResponse?> CheckHoverInformationAsync(HoverInfoRequest request)
    {
        var result = await _intellisenseCodeService.HandleAsync(request);

        return result;
    }

    [HttpPost("signature")]
    public async Task<SignatureResponse?> CheckSignatureInformationAsync(SignatureRequest request)
    {
        var result = await _intellisenseCodeService.HandleAsync(request);

        return result;
    }

    [HttpPost("check")]
    public async Task<CodeCheckResponse[]> CheckCodeCompletionAsync(CodeCheckRequest request)
    {
        var result = await _intellisenseCodeService.HandleAsync(request);

        return result;
    }
}
