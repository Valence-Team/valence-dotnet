using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Valence.Observatorio.API.Filters
{
    public class DescricaoSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema.Properties is null || schema.Properties.Count == 0)
                return;

            foreach (var propriedade in context.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var descricao = propriedade.GetCustomAttribute<DescriptionAttribute>()?.Description;

                if (string.IsNullOrWhiteSpace(descricao))
                    continue;

                var nome = JsonNamingPolicy.CamelCase.ConvertName(propriedade.Name);

                if (schema.Properties.TryGetValue(nome, out var esquema) && esquema.Reference is null)
                    esquema.Description = descricao;
            }
        }
    }
}
