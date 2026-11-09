using System.Globalization;
using Microsoft.AspNetCore.WebUtilities;
using Valence.Observatorio.Application.Dtos;

namespace Valence.Observatorio.Application.Mappers
{
    public static class PaginacaoMapper
    {
        public static List<LinkDto> Links(string rota, IReadOnlyList<KeyValuePair<string, string?>> filtros, int deslocamento, int registroRetornado, int totalRegistros)
        {
            var links = new List<LinkDto> { Link(rota, filtros, deslocamento, registroRetornado, "self") };

            if (deslocamento > 0)
                links.Add(Link(rota, filtros, Math.Max(0, deslocamento - registroRetornado), registroRetornado, "prev"));

            if (deslocamento + registroRetornado < totalRegistros)
                links.Add(Link(rota, filtros, deslocamento + registroRetornado, registroRetornado, "next"));

            return links;
        }

        private static LinkDto Link(string rota, IEnumerable<KeyValuePair<string, string?>> filtros, int deslocamento, int registroRetornado, string rel)
        {
            var parametros = filtros
                .Where(f => !string.IsNullOrWhiteSpace(f.Value))
                .Append(new KeyValuePair<string, string?>("deslocamento", deslocamento.ToString(CultureInfo.InvariantCulture)))
                .Append(new KeyValuePair<string, string?>("registroRetornado", registroRetornado.ToString(CultureInfo.InvariantCulture)));

            return new LinkDto { Href = QueryHelpers.AddQueryString(rota, parametros), Rel = rel, Method = "GET" };
        }
    }
}
