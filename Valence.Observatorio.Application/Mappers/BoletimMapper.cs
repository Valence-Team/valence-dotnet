using Valence.Observatorio.Application.Dtos;
using Valence.Observatorio.Domain.Entities;

namespace Valence.Observatorio.Application.Mappers
{
    public static class BoletimMapper
    {
        public const string RotaBoletins = "/api/boletins";

        public static string Rota(int id) => $"{RotaBoletins}/{id}";

        public static GetBoletimDto ToDto(this BoletimEntity boletim) => new()
        {
            Id = boletim.Id,
            Municipio = boletim.Municipio,
            Uf = boletim.Uf,
            Titulo = boletim.Titulo,
            Texto = boletim.Texto,
            GestorId = boletim.GestorId,
            CriadoEm = boletim.CriadoEm,
            AtualizadoEm = boletim.AtualizadoEm,
            Links =
            [
                new LinkDto { Href = Rota(boletim.Id), Rel = "self", Method = "GET" },
                new LinkDto { Href = Rota(boletim.Id), Rel = "atualizar", Method = "PUT" },
                new LinkDto { Href = Rota(boletim.Id), Rel = "remover", Method = "DELETE" },
                new LinkDto { Href = RotaBoletins, Rel = "boletins", Method = "GET" }
            ]
        };

        public static List<LinkDto> LinksDaPagina(FiltroBoletimDto filtro, int deslocamento, int registroRetornado, int totalRegistros)
        {
            var filtros = new List<KeyValuePair<string, string?>>
            {
                new("uf", filtro.Uf?.Trim().ToUpperInvariant()),
                new("municipio", filtro.Municipio?.Trim())
            };

            return PaginacaoMapper.Links(RotaBoletins, filtros, deslocamento, registroRetornado, totalRegistros);
        }
    }
}
