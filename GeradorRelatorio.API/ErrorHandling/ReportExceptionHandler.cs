using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GeradorRelatorio.Application.Exceptions;

namespace GeradorRelatorio.API.ErrorHandling;

public sealed class ReportExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<ReportExceptionHandler> _logger;

    public ReportExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<ReportExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title, detail) = Map(exception);
        if (status is null)
        {
            _logger.LogError(exception, "Erro não tratado ao processar a requisição.");
            return false;
        }

        if (status >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "{Title}", title);
        }
        else
        {
            _logger.LogWarning("{Title}: {Detail}", title, detail);
        }

        httpContext.Response.StatusCode = status.Value;

        var problemDetails = new ProblemDetails
        {
            Status = status.Value,
            Title = title,
            Detail = detail
        };

        if (exception is ReportValidationException validation)
        {
            problemDetails.Extensions["errors"] = validation.Errors;
        }

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails
        });
    }

    private static (int? Status, string Title, string? Detail) Map(Exception exception) => exception switch
    {
        ReportModelNotFoundException ex =>
            (StatusCodes.Status404NotFound, "Modelo de relatório não encontrado", ex.Message),
        ReportValidationException ex =>
            (StatusCodes.Status400BadRequest, "Parâmetros do relatório inválidos", ex.Message),
        InvalidExportFormatException ex =>
            (StatusCodes.Status400BadRequest, "Formato de exportação inválido", ex.Message),
        InvalidTemplateException ex =>
            (StatusCodes.Status422UnprocessableEntity, "Template do relatório inválido", ex.Message),
        InvalidDataSourceException ex =>
            (StatusCodes.Status422UnprocessableEntity, "Fonte de dados não permitida", ex.Message),
        ReportQueryExecutionException ex =>
            (StatusCodes.Status502BadGateway, "Erro ao consultar a fonte de dados", ex.Message),
        ReportExportException ex =>
            (StatusCodes.Status500InternalServerError, "Erro ao exportar o relatório", ex.Message),
        _ => (null, string.Empty, null)
    };
}
