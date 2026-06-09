using Microsoft.AspNetCore.Mvc;
using GeradorRelatorio.Application.Dtos;
using GeradorRelatorio.Application.UseCases;

namespace GeradorRelatorio.API.Controllers;

[ApiController]
[Route("api/v1/reports")]
public sealed class ReportsController : ControllerBase
{
    private readonly GenerateReportPreviewUseCase _previewUseCase;
    private readonly ExportReportUseCase _exportUseCase;

    public ReportsController(
        GenerateReportPreviewUseCase previewUseCase,
        ExportReportUseCase exportUseCase)
    {
        _previewUseCase = previewUseCase;
        _exportUseCase = exportUseCase;
    }

    /// <summary>Gera o preview tabular de um relatório.</summary>
    [HttpPost("preview")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ReportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReportResultDto>> Preview(
        [FromBody] GenerateReportRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _previewUseCase.ExecuteAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Exporta o relatório no formato solicitado (PDF, Excel ou CSV).</summary>
    [HttpPost("export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Export(
        [FromBody] ExportReportRequest request,
        CancellationToken cancellationToken)
    {
        var file = await _exportUseCase.ExecuteAsync(request, cancellationToken);
        return File(file.Content, file.ContentType, file.FileName);
    }
}
