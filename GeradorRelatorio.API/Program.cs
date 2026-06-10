using System.Text.Json.Serialization;
using Microsoft.OpenApi;
using Serilog;
using GeradorRelatorio.API.ErrorHandling;
using GeradorRelatorio.API.Tenancy;
using GeradorRelatorio.Application;
using GeradorRelatorio.Application.Interfaces;
using GeradorRelatorio.Infrastructure;
using GeradorRelatorio.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

// CORS para permitir o frontend local acessar a API no servidor.
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendLocal", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:3000",
                "http://127.0.0.1:3000",
                "http://192.168.1.50:3000",
                "http://192.168.1.80:3000"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// MVC + serialização de enums como string (ex.: "Pdf", "Currency").
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// ProblemDetails + handler de exceções de negócio.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ReportExceptionHandler>();

// Contexto multiempresa (lido do header X-Empresa-Id).
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantContext, TenantContext>();

// Camadas de aplicação e infraestrutura.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Swagger / OpenAPI.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "GeradorRelatorio API",
        Version = "v1",
        Description = "API de geração dinâmica de relatórios do ERP DNA Plus."
    });

    options.AddSecurityDefinition(TenantContext.HeaderName, new OpenApiSecurityScheme
    {
        Name = TenantContext.HeaderName,
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Description = "Identificador da empresa (tenant)."
    });
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseSerilogRequestLogging();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "GeradorRelatorio API v1");
});

// IMPORTANTE: UseCors antes de Authorization e antes de MapControllers.
app.UseCors("FrontendLocal");

app.UseAuthorization();

app.MapControllers();

await DatabaseInitializer.InitializeAsync(app.Services);

app.Run();