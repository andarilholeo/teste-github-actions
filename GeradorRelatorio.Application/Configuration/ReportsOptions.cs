namespace GeradorRelatorio.Application.Configuration;

public sealed class ReportsOptions
{
    public const string SectionName = "Reports";

    public int MaxPreviewRows { get; set; } = 100;

    public int DefaultTimeoutSeconds { get; set; } = 60;


    public bool UseMockData { get; set; } = true;
}
