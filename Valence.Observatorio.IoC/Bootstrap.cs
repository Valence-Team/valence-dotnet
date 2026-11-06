using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Oracle.EntityFrameworkCore.Infrastructure;
using Valence.Observatorio.Domain.Interfaces;
using Valence.Observatorio.Infrastructure.Data;
using Valence.Observatorio.Infrastructure.Data.Repositories;

namespace Valence.Observatorio.IoC
{
    public static class Bootstrap
    {
        public static void AddIoC(this IServiceCollection services, IConfiguration configuration)
        {
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
        }
    }
}
