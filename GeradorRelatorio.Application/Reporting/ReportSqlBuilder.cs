using System.Globalization;
using System.Text;
using System.Text.Json;
using GeradorRelatorio.Application.Common;
using GeradorRelatorio.Application.Dtos;

namespace GeradorRelatorio.Application.Reporting;

public static class ReportSqlBuilder
{
    private static readonly HashSet<string> JoinTypes =
        new(StringComparer.OrdinalIgnoreCase) { "INNER", "LEFT", "RIGHT", "FULL" };

    private static readonly HashSet<string> Aggregations =
        new(StringComparer.OrdinalIgnoreCase) { "SUM", "COUNT", "AVG", "MIN", "MAX" };

    private static readonly HashSet<string> Operators =
        new(StringComparer.OrdinalIgnoreCase) { "=", "<>", ">", "<", ">=", "<=", "LIKE", "ILIKE" };

    private static readonly HashSet<string> Directions =
        new(StringComparer.OrdinalIgnoreCase) { "ASC", "DESC" };

    public static Result<GeneratedQuery> Build(ReportQueryRequest request)
    {
        if (!DataSourceValidator.IsValidColumn(request.TabelaPrincipal))
        {
            return Error.Validation($"Tabela principal inválida: '{request.TabelaPrincipal}'.");
        }

        if (request.Campos.Count == 0)
        {
            return Error.Validation("Informe ao menos um campo no SELECT.");
        }

        var tables = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { request.TabelaPrincipal };
        var adjacency = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var join in request.Joins)
        {
            if (!JoinTypes.Contains(join.Tipo))
            {
                return Error.Validation($"Tipo de join inválido: '{join.Tipo}'.");
            }

            if (!IsValidRef(join.Origem) || !IsValidRef(join.Destino))
            {
                return Error.Validation("Join com tabela ou campo inválido.");
            }

            tables.Add(join.Origem.Tabela);
            tables.Add(join.Destino.Tabela);
            AddEdge(adjacency, join.Origem.Tabela, join.Destino.Tabela);
        }

        var connectivityError = EnsureConnected(request.TabelaPrincipal, tables, adjacency);
        if (connectivityError is not null)
        {
            return connectivityError;
        }

        var hasAggregation = request.Campos.Exists(c => !string.IsNullOrWhiteSpace(c.Agregacao));
        var groupKeys = new HashSet<string>(request.Agrupamentos.Select(g => Key(g.Tabela, g.Campo)));
        var aggregated = new Dictionary<string, string>();

        var selectParts = new List<string>();
        foreach (var campo in request.Campos)
        {
            if (!IsValidField(campo, tables))
            {
                return Error.Validation($"Campo inválido no SELECT: '{campo.Tabela}.{campo.Campo}'.");
            }

            string expression;
            if (!string.IsNullOrWhiteSpace(campo.Agregacao))
            {
                if (!Aggregations.Contains(campo.Agregacao))
                {
                    return Error.Validation($"Agregação inválida: '{campo.Agregacao}'.");
                }

                expression = $"{campo.Agregacao.ToUpperInvariant()}({Col(campo.Tabela, campo.Campo)})";
                aggregated[Key(campo.Tabela, campo.Campo)] = campo.Agregacao.ToUpperInvariant();
            }
            else
            {
                if ((hasAggregation || groupKeys.Count > 0) && !groupKeys.Contains(Key(campo.Tabela, campo.Campo)))
                {
                    return Error.Validation(
                        $"O campo '{campo.Tabela}.{campo.Campo}' precisa estar nos agrupamentos ou usar uma agregação quando há agrupamento.");
                }

                expression = Col(campo.Tabela, campo.Campo);
            }

            if (!string.IsNullOrWhiteSpace(campo.Alias))
            {
                if (!DataSourceValidator.IsValidColumn(campo.Alias))
                {
                    return Error.Validation($"Alias inválido: '{campo.Alias}'.");
                }

                expression += $" AS {Quote(campo.Alias)}";
            }

            selectParts.Add(expression);
        }

        var parameters = new List<object?>();
        var where = new List<string>();

        foreach (var filtro in request.Filtros)
        {
            if (!IsValidField(filtro.Tabela, filtro.Campo, tables))
            {
                return Error.Validation($"Filtro com campo inválido: '{filtro.Tabela}.{filtro.Campo}'.");
            }

            if (!Operators.Contains(filtro.Operador))
            {
                return Error.Validation($"Operador inválido: '{filtro.Operador}'.");
            }

            var placeholder = $"@p{parameters.Count}";
            parameters.Add(ToClrValue(filtro.Valor));
            where.Add($"{Col(filtro.Tabela, filtro.Campo)} {filtro.Operador.ToUpperInvariant()} {placeholder}");
        }

        foreach (var periodo in request.Periodos)
        {
            if (!IsValidField(periodo.Tabela, periodo.Campo, tables))
            {
                return Error.Validation($"Período com campo inválido: '{periodo.Tabela}.{periodo.Campo}'.");
            }

            var coluna = Col(periodo.Tabela, periodo.Campo);
            if (periodo.Inicio is not null && periodo.Fim is not null)
            {
                var p1 = $"@p{parameters.Count}";
                parameters.Add(ToDateValue(periodo.Inicio));
                var p2 = $"@p{parameters.Count}";
                parameters.Add(ToDateValue(periodo.Fim));
                where.Add($"{coluna} BETWEEN {p1} AND {p2}");
            }
            else if (periodo.Inicio is not null)
            {
                var p1 = $"@p{parameters.Count}";
                parameters.Add(ToDateValue(periodo.Inicio));
                where.Add($"{coluna} >= {p1}");
            }
            else if (periodo.Fim is not null)
            {
                var p1 = $"@p{parameters.Count}";
                parameters.Add(ToDateValue(periodo.Fim));
                where.Add($"{coluna} <= {p1}");
            }
        }

        foreach (var grupo in request.Agrupamentos)
        {
            if (!IsValidField(grupo.Tabela, grupo.Campo, tables))
            {
                return Error.Validation($"Agrupamento com campo inválido: '{grupo.Tabela}.{grupo.Campo}'.");
            }
        }

        var orderParts = new List<string>();
        foreach (var ordenacao in request.Ordenacoes)
        {
            if (!IsValidField(ordenacao.Tabela, ordenacao.Campo, tables))
            {
                return Error.Validation($"Ordenação com campo inválido: '{ordenacao.Tabela}.{ordenacao.Campo}'.");
            }

            if (!Directions.Contains(ordenacao.Direcao))
            {
                return Error.Validation($"Direção de ordenação inválida: '{ordenacao.Direcao}'.");
            }

            var direcao = ordenacao.Direcao.ToUpperInvariant();
            var key = Key(ordenacao.Tabela, ordenacao.Campo);

            if (hasAggregation || groupKeys.Count > 0)
            {
                if (groupKeys.Contains(key))
                {
                    orderParts.Add($"{Col(ordenacao.Tabela, ordenacao.Campo)} {direcao}");
                }
                else if (aggregated.TryGetValue(key, out var agg))
                {
                    orderParts.Add($"{agg}({Col(ordenacao.Tabela, ordenacao.Campo)}) {direcao}");
                }
                else
                {
                    return Error.Validation(
                        $"A ordenação por '{ordenacao.Tabela}.{ordenacao.Campo}' precisa estar nos agrupamentos ou ser uma agregação.");
                }
            }
            else
            {
                orderParts.Add($"{Col(ordenacao.Tabela, ordenacao.Campo)} {direcao}");
            }
        }

        var sql = new StringBuilder();
        sql.Append("SELECT ").Append(string.Join(", ", selectParts));
        sql.Append(" FROM ").Append(Quote(request.TabelaPrincipal));

        foreach (var join in request.Joins)
        {
            sql.Append(' ').Append(join.Tipo.ToUpperInvariant()).Append(" JOIN ")
               .Append(Quote(join.Destino.Tabela))
               .Append(" ON ").Append(Col(join.Origem.Tabela, join.Origem.Campo))
               .Append(" = ").Append(Col(join.Destino.Tabela, join.Destino.Campo));
        }

        if (where.Count > 0)
        {
            sql.Append(" WHERE ").Append(string.Join(" AND ", where));
        }

        if (request.Agrupamentos.Count > 0)
        {
            var groupCols = request.Agrupamentos.Select(g => Col(g.Tabela, g.Campo));
            sql.Append(" GROUP BY ").Append(string.Join(", ", groupCols));
        }

        if (orderParts.Count > 0)
        {
            sql.Append(" ORDER BY ").Append(string.Join(", ", orderParts));
        }

        return new GeneratedQuery { Sql = sql.ToString(), Parameters = parameters };
    }

    private static Error? EnsureConnected(
        string root,
        HashSet<string> tables,
        Dictionary<string, HashSet<string>> adjacency)
    {
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var queue = new Queue<string>();
        queue.Enqueue(root);
        visited.Add(root);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!adjacency.TryGetValue(current, out var neighbours))
            {
                continue;
            }

            foreach (var next in neighbours)
            {
                if (visited.Add(next))
                {
                    queue.Enqueue(next);
                }
            }
        }

        var unreachable = tables.Where(t => !visited.Contains(t)).ToList();
        return unreachable.Count == 0
            ? null
            : Error.Validation(
                $"As tabelas {string.Join(", ", unreachable)} não estão conectadas à tabela principal '{root}' por joins.");
    }

    private static void AddEdge(Dictionary<string, HashSet<string>> adjacency, string a, string b)
    {
        if (!adjacency.TryGetValue(a, out var setA))
        {
            setA = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            adjacency[a] = setA;
        }

        if (!adjacency.TryGetValue(b, out var setB))
        {
            setB = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            adjacency[b] = setB;
        }

        setA.Add(b);
        setB.Add(a);
    }

    private static bool IsValidRef(ReportColumnRefDto reference)
        => DataSourceValidator.IsValidColumn(reference.Tabela) && DataSourceValidator.IsValidColumn(reference.Campo);

    private static bool IsValidField(ReportFieldDto field, HashSet<string> tables)
        => IsValidField(field.Tabela, field.Campo, tables);

    private static bool IsValidField(string tabela, string campo, HashSet<string> tables)
        => DataSourceValidator.IsValidColumn(tabela)
           && DataSourceValidator.IsValidColumn(campo)
           && tables.Contains(tabela);

    private static string Quote(string identifier) => $"\"{identifier}\"";

    private static string Col(string tabela, string campo) => $"{Quote(tabela)}.{Quote(campo)}";

    private static string Key(string tabela, string campo)
        => $"{tabela.ToLowerInvariant()}.{campo.ToLowerInvariant()}";

    private static object? ToClrValue(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.String => value.GetString(),
        JsonValueKind.Number => value.TryGetInt64(out var l) ? l : value.GetDouble(),
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.Null or JsonValueKind.Undefined => null,
        _ => value.GetRawText()
    };

    private static object ToDateValue(string value)
        => DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : value;
}
