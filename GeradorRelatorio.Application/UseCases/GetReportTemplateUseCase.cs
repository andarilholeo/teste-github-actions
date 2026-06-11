using GeradorRelatorio.Application.Common;
using GeradorRelatorio.Application.Dtos;
using GeradorRelatorio.Application.Interfaces;

namespace GeradorRelatorio.Application.UseCases;

public sealed class GetReportTemplateUseCase(IReportTemplateRepository repository)
{
    private readonly IReportTemplateRepository _repository = repository;

    public Task<Result<ReportTemplateDto>> ExecuteAsync(Guid id, CancellationToken cancellationToken)
        => _repository.GetAsync(id, cancellationToken);
}
