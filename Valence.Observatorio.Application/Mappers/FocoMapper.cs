using System.Globalization;
using Microsoft.AspNetCore.WebUtilities;
using Valence.Observatorio.Application.Dtos;
using Valence.Observatorio.Domain.Entities;

namespace Valence.Observatorio.Application.Mappers
{
    public static class FocoMapper
    {
        public const string RotaFocos = "/api/publico/focos";
        public const string RotaEstatisticas = "/api/publico/estatisticas";

        public static GetFocoDto ToDto(this FocoAnonimizadoEntity foco) => new()
        {
            Id = foco.Id,
            Cultura = foco.Cultura,
            PragaProvavel = foco.PragaProvavel,
            Severidade = foco.Severidade,
            Municipio = foco.Municipio,
            Uf = foco.Uf,
            Status = foco.Status,
            DataOcorrencia = foco.DataOcorrencia,
            AtualizadoEm = foco.AtualizadoEm,
            Links =
            [
                new LinkDto { Href = $"{RotaFocos}/{foco.Id}", Rel = "self", Method = "GET" },
                new LinkDto { Href = RotaFocos, Rel = "focos", Method = "GET" }
            ]
        };

        public static GetContagemDto ToDto(this ContagemModel contagem) => new()
        {
            Chave = contagem.Chave,
            Total = contagem.Total
        };

        public static GetEstatisticaDto ToDto(this EstatisticaFocoModel modelo, string? uf, int periodoDias, DateTime desde, DateTime ate) => new()
        {
            Uf = uf,
            PeriodoDias = periodoDias,
            Desde = desde,
            Ate = ate,
            TotalFocos = modelo.TotalFocos,
            PorStatus = modelo.PorStatus.Select(c => c.ToDto()).ToList(),
            PorCultura = modelo.PorCultura.Select(c => c.ToDto()).ToList(),
            PorPraga = modelo.PorPraga.Select(c => c.ToDto()).ToList(),
            PorMunicipio = modelo.PorMunicipio.Select(c => c.ToDto()).ToList(),
            Links =
            [
                new LinkDto
                {
                    Href = QueryHelpers.AddQueryString(RotaEstatisticas, Parametros(("uf", uf), ("periodo", periodoDias.ToString(CultureInfo.InvariantCulture)))),
                    Rel = "self",
                    Method = "GET"
                },
                new LinkDto
                {
                    Href = QueryHelpers.AddQueryString(RotaFocos, Parametros(("uf", uf), ("desde", FormatarData(desde)))),
                    Rel = "focos",
                    Method = "GET"
                }
            ]
        };

        public static List<LinkDto> LinksDaPagina(FiltroFocoDto filtro, DateTime? desde, int deslocamento, int registroRetornado, int totalRegistros)
        {
            var links = new List<LinkDto>
            {
                new() { Href = UrlDaPagina(filtro, desde, deslocamento, registroRetornado), Rel = "self", Method = "GET" }
            };

            if (deslocamento > 0)
                links.Add(new() { Href = UrlDaPagina(filtro, desde, Math.Max(0, deslocamento - registroRetornado), registroRetornado), Rel = "prev", Method = "GET" });

            if (deslocamento + registroRetornado < totalRegistros)
                links.Add(new() { Href = UrlDaPagina(filtro, desde, deslocamento + registroRetornado, registroRetornado), Rel = "next", Method = "GET" });

            return links;
        }

        private static string UrlDaPagina(FiltroFocoDto filtro, DateTime? desde, int deslocamento, int registroRetornado)
            => QueryHelpers.AddQueryString(RotaFocos, Parametros(
                ("cultura", filtro.Cultura?.Trim().ToUpperInvariant()),
                ("uf", filtro.Uf?.Trim().ToUpperInvariant()),
                ("municipio", filtro.Municipio?.Trim()),
                ("desde", desde.HasValue ? FormatarData(desde.Value) : null),
                ("deslocamento", deslocamento.ToString(CultureInfo.InvariantCulture)),
                ("registroRetornado", registroRetornado.ToString(CultureInfo.InvariantCulture))));

        private static IEnumerable<KeyValuePair<string, string?>> Parametros(params (string chave, string? valor)[] itens)
            => itens
                .Where(i => !string.IsNullOrWhiteSpace(i.valor))
                .Select(i => new KeyValuePair<string, string?>(i.chave, i.valor));

        private static string FormatarData(DateTime data)
            => data.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);
    }
}
