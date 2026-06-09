namespace GeradorRelatorio.Application.Interfaces;

public interface IPdfGenerator
{
    Task<byte[]> GenerateFromHtmlAsync(string html, CancellationToken cancellationToken);
}
