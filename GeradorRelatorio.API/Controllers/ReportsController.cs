using Microsoft.AspNetCore.Mvc;
using GeradorRelatorio.API.Extensions;
using GeradorRelatorio.Application.Dtos;
using GeradorRelatorio.Application.UseCases;

namespace GeradorRelatorio.API.Controllers;

[ApiController]
[Route("api/v1/reports")]
[Produces("application/json")]
public sealed class ReportsController : ControllerBase
{
    private readonly GenerateReportSqlUseCase _sqlUseCase;
    private readonly GenerateReportPreviewUseCase _previewUseCase;

    public ReportsController(
        GenerateReportSqlUseCase sqlUseCase,
        GenerateReportPreviewUseCase previewUseCase)
    {
        _sqlUseCase = sqlUseCase;
        _previewUseCase = previewUseCase;
    }

    [HttpPost("sql", Name = "GerarSql")]
    [ProducesResponseType(typeof(ReportSqlDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public ActionResult<ReportSqlDto> GenerateSql([FromBody] ReportQueryRequest request)
    {
        var result = _sqlUseCase.Execute(request);
        return result.ToActionResult(this);
    }

    [HttpPost("preview", Name = "PreviewRelatorio")]
    [ProducesResponseType(typeof(ReportPreviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<ReportPreviewDto>> Preview(
        [FromBody] ReportQueryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _previewUseCase.ExecuteAsync(request, cancellationToken);
        return result.ToActionResult(this);
    }
}
