using System.Text.Json;
using System.Text.Json.Serialization;
using Valence.Observatorio.API.Serializacao;

namespace Valence.Observatorio.API.Extensions
{
    public static class JsonExtensions
    {
        public static IServiceCollection AddJsonObservatorio(this IServiceCollection services)
        {
            services.ConfigureHttpJsonOptions(options => Configurar(options.SerializerOptions));
            services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options => Configurar(options.JsonSerializerOptions));

            return services;
        }

        public static void Configurar(JsonSerializerOptions options)
        {
            options.Converters.Add(new JsonStringEnumConverter());
            options.Converters.Add(new DataUtcJsonConverter());
        }
    }
}
