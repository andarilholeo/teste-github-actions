using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GeradorRelatorio.Application.Dtos;
using GeradorRelatorio.Application.Interfaces;

namespace GeradorRelatorio.Application.UseCases;

/// <summary>Lista os modelos de relatório ativos.</summary>
public sealed class ListReportModelsUseCase
{
    private readonly IReportModelRepository _repository;

    public ListReportModelsUseCase(IReportModelRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<ReportModelDto>> ExecuteAsync(CancellationToken cancellationToken)
        => _repository.ListAsync(cancellationToken);
}
