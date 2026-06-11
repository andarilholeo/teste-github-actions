using GeradorRelatorio.Application.Common;
using GeradorRelatorio.Application.Dtos;

namespace GeradorRelatorio.Application.Interfaces;

public interface IDataSourceCatalog
{
    Task<Result<IReadOnlyList<DataSourceDto>>> ListAsync(CancellationToken cancellationToken);
    Task<Result<IReadOnlyList<DataSourceColumnDto>>> GetColumnsAsync(string fonteDados, CancellationToken cancellationToken);
}
