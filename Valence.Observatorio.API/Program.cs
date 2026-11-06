using Valence.Observatorio.IoC;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIoC(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();

// Acesso dos testes de integração
public partial class Program { }
