using GeradorRelatorio.Application.Dtos;

namespace GeradorRelatorio.Application.Interfaces;

public interface IReportModelRepository
{
    Task<IReadOnlyList<ReportModelDto>> ListAsync(CancellationToken cancellationToken);

    Task<ReportModelMetadataDto?> GetMetadataAsync(Guid id, CancellationToken cancellationToken);

    Task<ModeloRelatorioDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
