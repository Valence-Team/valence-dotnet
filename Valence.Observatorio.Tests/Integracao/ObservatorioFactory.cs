using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Valence.Observatorio.API.Messaging;
using Valence.Observatorio.Domain.Entities;
using Valence.Observatorio.Domain.Enums;
using Valence.Observatorio.Domain.Interfaces;
using Valence.Observatorio.Infrastructure.Data;
using Valence.Observatorio.Tests.Fakes;

namespace Valence.Observatorio.Tests.Integracao
{
    public class ObservatorioFactory : WebApplicationFactory<Program>
    {
        public const string SegredoJwt = "segredo-exclusivo-dos-testes-de-integracao-do-observatorio";

        private readonly string _banco = $"observatorio-integracao-{Guid.NewGuid()}";

        protected virtual IDictionary<string, string?> Configuracoes() => new Dictionary<string, string?>
        {
            ["Jwt:Secret"] = SegredoJwt,
            ["ConnectionStrings:Oracle"] = string.Empty,
            ["RabbitMq:Uri"] = string.Empty,
            ["RateLimiting:Publica:Limite"] = "1000",
            ["RateLimiting:Login:Limite"] = "1000",
            ["RateLimiting:Escrita:Limite"] = "1000",
            ["Serilog:MinimumLevel:Default"] = "Warning",
            ["Serilog:WriteTo:1:Args:shared"] = "true"
        };

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testes");
            builder.UseSetting("ConnectionStrings:Oracle", string.Empty);
            builder.UseSetting("RabbitMq:Uri", string.Empty);
            builder.ConfigureAppConfiguration((_, configuracao) => configuracao.AddInMemoryCollection(Configuracoes()));

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<ApplicationContext>>();
                services.AddDbContext<ApplicationContext>(opcoes => opcoes
                    .UseInMemoryDatabase(_banco)
                    .ConfigureWarnings(avisos => avisos.Ignore(InMemoryEventId.TransactionIgnoredWarning)));

                services.RemoveAll<IMunicipioService>();
                services.AddSingleton<IMunicipioService, MunicipioServiceFalso>();

                var consumidor = services.SingleOrDefault(d => !d.IsKeyedService && d.ImplementationType == typeof(ObservatorioConsumer));
                if (consumidor is not null)
                    services.Remove(consumidor);
            });
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            var host = base.CreateHost(builder);

            using var escopo = host.Services.CreateScope();
            var contexto = escopo.ServiceProvider.GetRequiredService<ApplicationContext>();
            contexto.Database.EnsureCreated();
            Semear(contexto);

            return host;
        }

        private static void Semear(ApplicationContext contexto)
        {
            if (contexto.UsuarioGestor.Any(u => u.Id == UsuariosDeTeste.Gestor.Id))
                return;

            var hash = BCrypt.Net.BCrypt.HashPassword(UsuariosDeTeste.Senha, 4);

            contexto.UsuarioGestor.AddRange(
                new UsuarioGestorEntity { Id = UsuariosDeTeste.Gestor.Id, Email = UsuariosDeTeste.Gestor.Email, SenhaHash = hash, Papel = Papel.GESTOR },
                new UsuarioGestorEntity { Id = UsuariosDeTeste.OutroGestor.Id, Email = UsuariosDeTeste.OutroGestor.Email, SenhaHash = hash, Papel = Papel.GESTOR },
                new UsuarioGestorEntity { Id = UsuariosDeTeste.Agronomo.Id, Email = UsuariosDeTeste.Agronomo.Email, SenhaHash = hash, Papel = Papel.AGRONOMO });

            var agora = DateTime.UtcNow;

            contexto.Foco.AddRange(
                FocoAnonimizadoEntity.CriarPorTriagem(900001, Cultura.MILHO, "Cigarrinha-do-milho (enfezamento)", 4, "Ribeirão Preto", "SP", agora.AddDays(-2), agora),
                FocoAnonimizadoEntity.CriarPorTriagem(900002, Cultura.TOMATE, "Requeima", 3, "Campinas", "SP", agora.AddDays(-1), agora),
                FocoAnonimizadoEntity.CriarPorLaudo(900003, Cultura.BATATA, DecisaoLaudo.CONFIRMADA, "Requeima", "Guarapuava", "PR", agora.AddDays(-10), agora),
                FocoAnonimizadoEntity.CriarPorTriagem(900004, Cultura.MILHO, "Lagarta-do-cartucho", 2, "Campinas", "SP", agora.AddDays(-45), agora));

            contexto.SaveChanges();
        }
    }

    public class ObservatorioFactoryLimitesBaixos : ObservatorioFactory
    {
        public const int LimitePublico = 3;
        public const int LimiteLogin = 2;

        protected override IDictionary<string, string?> Configuracoes()
        {
            var configuracoes = base.Configuracoes();
            configuracoes["RateLimiting:Publica:Limite"] = LimitePublico.ToString();
            configuracoes["RateLimiting:Login:Limite"] = LimiteLogin.ToString();
            return configuracoes;
        }
    }
}
