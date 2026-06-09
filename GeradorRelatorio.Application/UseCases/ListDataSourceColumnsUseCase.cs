using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GeradorRelatorio.Application.Dtos;
using GeradorRelatorio.Application.Interfaces;

namespace GeradorRelatorio.Application.UseCases;

/// <summary>Lista as colunas de uma tabela/view do <c>public</c> escolhida.</summary>
public sealed class ListDataSourceColumnsUseCase
{
    private readonly IDataSourceCatalog _catalog;

    public ListDataSourceColumnsUseCase(IDataSourceCatalog catalog)
    {
        _catalog = catalog;
    }

    public Task<IReadOnlyList<DataSourceColumnDto>> ExecuteAsync(string table, CancellationToken cancellationToken)
        => _catalog.GetColumnsAsync(table, cancellationToken);
}
