using GeradorRelatorio.Application.Interfaces;

namespace GeradorRelatorio.API.Tenancy;

public sealed class TenantContext : ITenantContext
{
    public const string HeaderName = "X-Empresa-Id";

    public TenantContext(IHttpContextAccessor httpContextAccessor)
    {
        var headers = httpContextAccessor.HttpContext?.Request.Headers;
        if (headers is not null
            && headers.TryGetValue(HeaderName, out var value)
            && Guid.TryParse(value.ToString(), out var empresaId))
        {
            EmpresaId = empresaId;
        }
    }

    public Guid? EmpresaId { get; }
}
