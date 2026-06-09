using GeradorRelatorio.Application.Dtos;

namespace GeradorRelatorio.Application.Interfaces;

public interface IDataSourceCatalog
{
    Task<IReadOnlyList<DataSourceDto>> ListAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<DataSourceColumnDto>> GetColumnsAsync(string fonteDados, CancellationToken cancellationToken);
}
