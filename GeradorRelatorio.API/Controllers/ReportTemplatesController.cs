using Microsoft.AspNetCore.Mvc;
using GeradorRelatorio.API.Extensions;
using GeradorRelatorio.Application.Dtos;
using GeradorRelatorio.Application.UseCases;

namespace GeradorRelatorio.API.Controllers;

[ApiController]
[Route("api/v1/report-templates")]
[Produces("application/json")]
public sealed class ReportTemplatesController : ControllerBase
{
    private readonly SaveReportTemplateUseCase _saveUseCase;
    private readonly GetReportTemplateUseCase _getUseCase;
    private readonly ListReportTemplatesUseCase _listUseCase;

    public ReportTemplatesController(
        SaveReportTemplateUseCase saveUseCase,
        GetReportTemplateUseCase getUseCase,
        ListReportTemplatesUseCase listUseCase)
    {
        _saveUseCase = saveUseCase;
        _getUseCase = getUseCase;
        _listUseCase = listUseCase;
    }

    [HttpGet(Name = "ListarTemplates")]
    [ProducesResponseType(typeof(IReadOnlyList<ReportTemplateDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ReportTemplateDto>>> List(CancellationToken cancellationToken)
    {
        var result = await _listUseCase.ExecuteAsync(cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpGet("{id:guid}", Name = "ObterTemplate")]
    [ProducesResponseType(typeof(ReportTemplateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReportTemplateDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getUseCase.ExecuteAsync(id, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost(Name = "CriarTemplate")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<Guid>> Create(
        [FromBody] SaveReportTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _saveUseCase.ExecuteAsync(request, null, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPut("{id:guid}", Name = "AtualizarTemplate")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Guid>> Update(
        Guid id,
        [FromBody] SaveReportTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _saveUseCase.ExecuteAsync(request, id, cancellationToken);
        return result.ToActionResult(this);
    }
}
