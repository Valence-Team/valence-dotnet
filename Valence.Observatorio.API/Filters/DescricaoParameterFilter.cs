using System.ComponentModel;
using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Valence.Observatorio.API.Filters
{
    public class DescricaoParameterFilter : IParameterFilter
    {
        public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
        {
            if (!string.IsNullOrWhiteSpace(parameter.Description))
                return;

            var descricao = context.PropertyInfo?.GetCustomAttribute<DescriptionAttribute>()?.Description
                ?? DescricaoDoParametro(context.ParameterInfo);

            if (!string.IsNullOrWhiteSpace(descricao))
                parameter.Description = descricao;
        }

        private static string? DescricaoDoParametro(ParameterInfo? parametro)
        {
            if (parametro is null)
                return null;

            return parametro.GetCustomAttributes(typeof(DescriptionAttribute), true)
                .OfType<DescriptionAttribute>()
                .FirstOrDefault()?.Description;
        }
    }
}
