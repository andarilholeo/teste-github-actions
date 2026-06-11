using GeradorRelatorio.Application.Common;
using GeradorRelatorio.Application.Dtos;
using GeradorRelatorio.Application.Interfaces;

namespace GeradorRelatorio.Application.UseCases;

public sealed class ListDataSourcesUseCase(IDataSourceCatalog catalog)
{
    private readonly IDataSourceCatalog _catalog = catalog;

    public Task<Result<IReadOnlyList<DataSourceDto>>> ExecuteAsync(CancellationToken cancellationToken)
        => _catalog.ListAsync(cancellationToken);
}
