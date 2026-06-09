using Microsoft.AspNetCore.Mvc;
using GeradorRelatorio.Application.Dtos;
using GeradorRelatorio.Application.UseCases;

namespace GeradorRelatorio.API.Controllers;

[ApiController]
[Route("api/v1/report-models")]
[Produces("application/json")]
public sealed class ReportModelsController : ControllerBase
{
    private readonly ListReportModelsUseCase _listUseCase;
    private readonly GetReportModelMetadataUseCase _metadataUseCase;

    public ReportModelsController(
        ListReportModelsUseCase listUseCase,
        GetReportModelMetadataUseCase metadataUseCase)
    {
        _listUseCase = listUseCase;
        _metadataUseCase = metadataUseCase;
    }

    /// <summary>Lista os modelos de relatório disponíveis.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ReportModelDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ReportModelDto>>> List(CancellationToken cancellationToken)
    {
        var models = await _listUseCase.ExecuteAsync(cancellationToken);
        return Ok(models);
    }

    /// <summary>Retorna os metadados (parâmetros, colunas, formatos) de um modelo.</summary>
    [HttpGet("{id:guid}/metadata")]
    [ProducesResponseType(typeof(ReportModelMetadataDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReportModelMetadataDto>> GetMetadata(
        Guid id,
        CancellationToken cancellationToken)
    {
        var metadata = await _metadataUseCase.ExecuteAsync(id, cancellationToken);
        return Ok(metadata);
    }
}
