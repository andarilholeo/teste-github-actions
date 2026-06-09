namespace GeradorRelatorio.Application.Exceptions;

public abstract class ReportException : Exception
{
    protected ReportException(string message) : base(message)
    {
    }
}

public sealed class ReportModelNotFoundException : ReportException
{
    public ReportModelNotFoundException(Guid id)
        : base($"Nenhum modelo de relatório foi encontrado para o id informado ({id}).")
    {
        Id = id;
    }

    public Guid Id { get; }
}

public sealed class ReportValidationException : ReportException
{
    public ReportValidationException(IReadOnlyList<string> errors)
        : base("Um ou mais parâmetros do relatório são inválidos.")
    {
        Errors = errors;
    }

    public ReportValidationException(string error)
        : this([error])
    {
    }

    public IReadOnlyList<string> Errors { get; }
}

public sealed class InvalidTemplateException(string message) : ReportException(message)
{
}

public sealed class InvalidDataSourceException : ReportException
{
    public InvalidDataSourceException(string fonteDados)
        : base($"A fonte de dados '{fonteDados}' não é permitida ou não existe. Apenas tabelas/views do schema 'public' são aceitas.")
    {
        FonteDados = fonteDados;
    }

    public string FonteDados { get; }
}

public sealed class InvalidExportFormatException(string message) : ReportException(message)
{
}

public sealed class ReportQueryExecutionException(string message, Exception innerException) : ReportException(message)
{
    public Exception Inner { get; } = innerException;
}

public sealed class ReportExportException(string message, Exception innerException) : ReportException(message)
{
    public Exception Inner { get; } = innerException;
}
