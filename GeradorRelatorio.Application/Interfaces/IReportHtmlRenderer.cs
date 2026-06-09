using GeradorRelatorio.Application.Dtos;

namespace GeradorRelatorio.Application.Interfaces;

public interface IReportHtmlRenderer
{
    Task<string> RenderAsync(ReportResultDto result, CancellationToken cancellationToken);
}
