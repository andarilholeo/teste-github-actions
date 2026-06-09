using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GeradorRelatorio.Application.Dtos;
using GeradorRelatorio.Application.Interfaces;
using GeradorRelatorio.Application.Reporting;
using GeradorRelatorio.Domain.Enums;

namespace GeradorRelatorio.Infrastructure.Reporting;

/// <summary>
/// Executor mockado usado enquanto as views/materialized views reais não existem.
/// Gera linhas de exemplo coerentes com as colunas do template, mantendo o mesmo
/// contrato do executor real (<see cref="NpgsqlReportQueryExecutor"/>).
/// </summary>
public sealed class MockReportQueryExecutor : IReportQueryExecutor
{
    private const int MockRowCount = 3;

    public Task<ReportResultDto> ExecutePreviewAsync(ReportQuery query, CancellationToken cancellationToken)
    {
        var result = new ReportResultDto
        {
            ModelId = query.ModelId,
            Title = query.Title,
            Columns = new List<ReportColumnDto>(query.Columns),
            GeneratedAt = DateTime.UtcNow
        };

        var rowsToGenerate = Math.Min(MockRowCount, query.MaxRows <= 0 ? MockRowCount : query.MaxRows);
        for (var i = 0; i < rowsToGenerate; i++)
        {
            var row = new Dictionary<string, object?>(query.Columns.Count);
            foreach (var column in query.Columns)
            {
                row[column.Field] = SampleValue(column.Type, i);
            }

            result.Rows.Add(row);
        }

        return Task.FromResult(result);
    }

    private static object SampleValue(ReportColumnType type, int index) => type switch
    {
        ReportColumnType.Currency => 1000m + index * 250.5m,
        ReportColumnType.Number => 10 + index,
        ReportColumnType.Date => new DateTime(2026, 1, 1).AddDays(index),
        ReportColumnType.Boolean => index % 2 == 0,
        _ => $"Exemplo {index + 1}"
    };
}
