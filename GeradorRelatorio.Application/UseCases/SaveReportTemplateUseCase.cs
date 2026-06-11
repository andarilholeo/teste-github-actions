using GeradorRelatorio.Application.Common;
using GeradorRelatorio.Application.Dtos;
using GeradorRelatorio.Application.Interfaces;

namespace GeradorRelatorio.Application.UseCases;

public sealed class SaveReportTemplateUseCase(IReportTemplateRepository repository)
{
    private readonly IReportTemplateRepository _repository = repository;

    public Task<Result<Guid>> ExecuteAsync(SaveReportTemplateRequest request, Guid? id, CancellationToken cancellationToken)
        => _repository.SaveAsync(request, id, cancellationToken);
}
