using System.IO.Compression;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.AspNetCore.ResponseCompression;
using Serilog;
using Valence.Observatorio.API.Endpoints;
using Valence.Observatorio.API.Errors;
using Valence.Observatorio.API.Extensions;
using Valence.Observatorio.API.Messaging;
using Valence.Observatorio.IoC;

var builder = WebApplication.CreateBuilder(args);

// Logs
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));

// Telemetria
var applicationInsights = builder.Configuration["ApplicationInsights:ConnectionString"] ?? string.Empty;

if (!string.IsNullOrWhiteSpace(applicationInsights))
    builder.Services.AddOpenTelemetry().UseAzureMonitor(options => options.ConnectionString = applicationInsights);

// Dependências
builder.Services.AddIoC(builder.Configuration);

// Mensageria
builder.Services.AddSingleton<RabbitMqConexao>();
builder.Services.AddHostedService<ObservatorioConsumer>();

// JSON
builder.Services.AddJsonObservatorio();

// Tratamento de erros
builder.Services.AddExceptionHandler<TratadorDeExcecoes>();
builder.Services.AddProblemDetails();

// Compressão
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
builder.Services.Configure<BrotliCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);
builder.Services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);

// Segurança
builder.Services.AddAutenticacaoJwt();
builder.Services.AddLimitesDeRequisicao();
builder.Services.AddCorsObservatorio();

// Saúde
builder.Services.AddVerificacoesDeSaude(builder.Configuration);

// Swagger
builder.Services.AddDocumentacao();

var app = builder.Build();

// Pipeline
app.UseSerilogRequestLogging();
app.UseExceptionHandler();
app.UseStatusCodePages(async contexto =>
{
    var http = contexto.HttpContext;
    await http.Response.WriteAsJsonAsync(ErroResposta.Criar(http, http.Response.StatusCode));
});
app.UseResponseCompression();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.UseSwagger();
app.UseSwaggerUI(options => options.DocumentTitle = "Valence · Observatório Fitossanitário");

// Endpoints
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
app.MapAuthEndpoints();
app.MapPublicoEndpoints();
app.MapBoletimEndpoints();
app.MapVerificacoesDeSaude();

app.Run();

// Acesso dos testes de integração
public partial class Program { }
