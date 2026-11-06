using System.IO.Compression;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.OpenApi.Models;
using Serilog;
using Valence.Observatorio.API.Endpoints;
using Valence.Observatorio.API.Errors;
using Valence.Observatorio.API.Extensions;
using Valence.Observatorio.API.Filters;
using Valence.Observatorio.IoC;

var builder = WebApplication.CreateBuilder(args);

// Logs
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));

// Dependências
builder.Services.AddIoC(builder.Configuration);

// JSON
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

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

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(AutorizacaoOperationFilter.Esquema, new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Token obtido em POST /api/auth/login. Cole apenas o token, sem a palavra Bearer."
    });
    options.OperationFilter<AutorizacaoOperationFilter>();
});

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
app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

// Endpoints
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
app.MapAuthEndpoints();

app.Run();

// Acesso dos testes de integração
public partial class Program { }
