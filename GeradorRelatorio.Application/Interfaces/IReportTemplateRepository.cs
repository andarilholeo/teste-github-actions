using GeradorRelatorio.Application.Common;
using GeradorRelatorio.Application.Dtos;

namespace GeradorRelatorio.Application.Interfaces;

public interface IReportTemplateRepository
{
    Task<Result<Guid>> SaveAsync(SaveReportTemplateRequest request, Guid? id, CancellationToken cancellationToken);
    Task<Result<ReportTemplateDto>> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<Result<IReadOnlyList<ReportTemplateDto>>> ListAsync(CancellationToken cancellationToken);
}
