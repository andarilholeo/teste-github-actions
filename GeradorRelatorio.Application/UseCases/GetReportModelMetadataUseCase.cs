using System.Threading;
using System.Threading.Tasks;
using GeradorRelatorio.Application.Dtos;
using GeradorRelatorio.Application.Exceptions;
using GeradorRelatorio.Application.Interfaces;

namespace GeradorRelatorio.Application.UseCases;

/// <summary>Retorna o metadata (parâmetros, colunas, formatos) de um modelo.</summary>
public sealed class GetReportModelMetadataUseCase
{
    private readonly IReportModelRepository _repository;

    public GetReportModelMetadataUseCase(IReportModelRepository repository)
    {
        _repository = repository;
    }

    public async Task<ReportModelMetadataDto> ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        var metadata = await _repository.GetMetadataAsync(id, cancellationToken);
        if (metadata is null)
        {
            throw new ReportModelNotFoundException(id);
        }

        return metadata;
    }
}
