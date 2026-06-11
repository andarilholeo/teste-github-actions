using Microsoft.AspNetCore.Mvc;
using GeradorRelatorio.API.Extensions;
using GeradorRelatorio.Application.Dtos;
using GeradorRelatorio.Application.UseCases;

namespace GeradorRelatorio.API.Controllers;

[ApiController]
[Route("api/v1/data-sources")]
[Produces("application/json")]
public sealed class DataSourcesController : ControllerBase
{
    private readonly ListDataSourcesUseCase _listUseCase;
    private readonly ListDataSourceColumnsUseCase _columnsUseCase;

    public DataSourcesController(
        ListDataSourcesUseCase listUseCase,
        ListDataSourceColumnsUseCase columnsUseCase)
    {
        _listUseCase = listUseCase;
        _columnsUseCase = columnsUseCase;
    }

    [HttpGet(Name = "ListarTabelas")]
    [ProducesResponseType(typeof(IReadOnlyList<DataSourceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<DataSourceDto>>> List(CancellationToken cancellationToken)
    {
        var result = await _listUseCase.ExecuteAsync(cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpGet("{table}/columns", Name = "ObterColunasTabela")]
    [ProducesResponseType(typeof(IReadOnlyList<DataSourceColumnDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<IReadOnlyList<DataSourceColumnDto>>> GetColumns(
        string table,
        CancellationToken cancellationToken)
    {
        var result = await _columnsUseCase.ExecuteAsync(table, cancellationToken);
        return result.ToActionResult(this);
    }
}
