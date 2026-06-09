namespace GeradorRelatorio.Application.Interfaces;

public interface ITemplateProvider
{
    Task<string> GetDefaultTemplateAsync(CancellationToken cancellationToken);
}
