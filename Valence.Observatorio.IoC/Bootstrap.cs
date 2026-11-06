using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Oracle.EntityFrameworkCore.Infrastructure;
using Valence.Observatorio.Application.Configs;
using Valence.Observatorio.Application.Interfaces;
using Valence.Observatorio.Application.Services;
using Valence.Observatorio.Application.UseCases;
using Valence.Observatorio.Domain.Interfaces;
using Valence.Observatorio.Infrastructure.Data;
using Valence.Observatorio.Infrastructure.Data.Repositories;

namespace Valence.Observatorio.IoC
{
    public static class Bootstrap
    {
        public static void AddIoC(this IServiceCollection services, IConfiguration configuration)
        {
            // Configurações
            services.AddOptions<JwtSettings>()
                .Bind(configuration.GetSection(JwtSettings.Secao))
                .Validate(s => Encoding.UTF8.GetByteCount(s.Secret ?? string.Empty) >= JwtSettings.TamanhoMinimoSegredo,
                    $"Configure Jwt:Secret (variável Jwt__Secret) com pelo menos {JwtSettings.TamanhoMinimoSegredo} caracteres.")
                .Validate(s => s.ExpiracaoMinutos > 0, "Jwt:ExpiracaoMinutos deve ser maior que zero.")
                .ValidateOnStart();

            services.AddSingleton(TimeProvider.System);

            // Banco de dados
            services.AddDbContext<ApplicationContext>(options =>
                options.UseOracle(configuration.GetConnectionString("Oracle"), oracle =>
                {
                    oracle.MigrationsHistoryTable("TB_OBS_MIGRATIONS");
                    oracle.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion19);
                }));

            // Repositórios
            services.AddScoped<IFocoRepository, FocoRepository>();
            services.AddScoped<IEventoProcessadoRepository, EventoProcessadoRepository>();
            services.AddScoped<IBoletimRepository, BoletimRepository>();
            services.AddScoped<IRequisicaoIdempotenteRepository, RequisicaoIdempotenteRepository>();
            services.AddScoped<IUsuarioGestorRepository, UsuarioGestorRepository>();

            // Serviços
            services.AddScoped<ITokenService, TokenService>();

            // Casos de uso
            services.AddScoped<IAuthUseCase, AuthUseCase>();
            services.AddScoped<IFocoUseCase, FocoUseCase>();
        }
    }
}
