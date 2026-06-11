using GeradorRelatorio.Application.Common;
using GeradorRelatorio.Application.Dtos;
using GeradorRelatorio.Application.Interfaces;

namespace GeradorRelatorio.Application.UseCases;

public sealed class ListReportTemplatesUseCase(IReportTemplateRepository repository)
{
    private readonly IReportTemplateRepository _repository = repository;

    public Task<Result<IReadOnlyList<ReportTemplateDto>>> ExecuteAsync(CancellationToken cancellationToken)
        => _repository.ListAsync(cancellationToken);
}
