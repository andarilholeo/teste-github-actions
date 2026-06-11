using GeradorRelatorio.Application.Common;
using GeradorRelatorio.Application.Dtos;
using GeradorRelatorio.Application.Reporting;

namespace GeradorRelatorio.Application.UseCases;

public sealed class GenerateReportSqlUseCase
{
    public Result<ReportSqlDto> Execute(ReportQueryRequest request)
    {
        var built = ReportSqlBuilder.Build(request);
        if (!built.IsSuccess)
        {
            return built.Error!;
        }

        return new ReportSqlDto
        {
            Sql = built.Value!.Sql,
            Parametros = built.Value.Parameters
        };
    }
}
