using GeradorRelatorio.Application.Common;
using GeradorRelatorio.Application.Dtos;
using GeradorRelatorio.Application.Interfaces;

namespace GeradorRelatorio.Application.UseCases;

public sealed class ListDataSourceColumnsUseCase(IDataSourceCatalog catalog)
{
    private readonly IDataSourceCatalog _catalog = catalog;

    public Task<Result<IReadOnlyList<DataSourceColumnDto>>> ExecuteAsync(string table, CancellationToken cancellationToken)
        => _catalog.GetColumnsAsync(table, cancellationToken);
}
