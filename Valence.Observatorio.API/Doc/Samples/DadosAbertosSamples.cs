using Swashbuckle.AspNetCore.Filters;
using Valence.Observatorio.Application.Dtos;
using Valence.Observatorio.Domain.Enums;

namespace Valence.Observatorio.API.Doc.Samples
{
    public class GetFocoSample : IExamplesProvider<GetFocoDto>
    {
        public GetFocoDto GetExamples() => Focos.Ribeirao;
    }

    public class PageFocoSample : IExamplesProvider<PageResponseDto<GetFocoDto>>
    {
        public PageResponseDto<GetFocoDto> GetExamples() => new()
        {
            Items = [Focos.Ribeirao, Focos.Campinas],
            Deslocamento = 0,
            RegistroRetornado = 2,
            TotalRegistros = 5,
            Links =
            [
                new LinkDto { Href = "/api/publico/focos?uf=SP&deslocamento=0&registroRetornado=2", Rel = "self", Method = "GET" },
                new LinkDto { Href = "/api/publico/focos?uf=SP&deslocamento=2&registroRetornado=2", Rel = "next", Method = "GET" }
            ]
        };
    }

    public class GetEstatisticaSample : IExamplesProvider<GetEstatisticaDto>
    {
        public GetEstatisticaDto GetExamples() => new()
        {
            Uf = "SP",
            PeriodoDias = 30,
            Desde = new DateTime(2026, 10, 11, 12, 0, 0, DateTimeKind.Utc),
            Ate = new DateTime(2026, 11, 10, 12, 0, 0, DateTimeKind.Utc),
            TotalFocos = 4,
            PorStatus =
            [
                new GetContagemDto { Chave = nameof(StatusFoco.SUSPEITA), Total = 3 },
                new GetContagemDto { Chave = nameof(StatusFoco.CONFIRMADO), Total = 1 }
            ],
            PorCultura =
            [
                new GetContagemDto { Chave = nameof(Cultura.MILHO), Total = 3 },
                new GetContagemDto { Chave = nameof(Cultura.TOMATE), Total = 1 }
            ],
            PorPraga =
            [
                new GetContagemDto { Chave = "Cigarrinha-do-milho (enfezamento)", Total = 3 },
                new GetContagemDto { Chave = "Requeima", Total = 1 }
            ],
            PorMunicipio =
            [
                new GetContagemDto { Chave = "Ribeirão Preto/SP", Total = 3 },
                new GetContagemDto { Chave = "Campinas/SP", Total = 1 }
            ],
            Links =
            [
                new LinkDto { Href = "/api/publico/estatisticas?uf=SP&periodo=30", Rel = "self", Method = "GET" },
                new LinkDto { Href = "/api/publico/focos?uf=SP&desde=2026-10-11T12:00:00Z", Rel = "focos", Method = "GET" }
            ]
        };
    }

    internal static class Focos
    {
        public static GetFocoDto Ribeirao => new()
        {
            Id = 12,
            Cultura = Cultura.MILHO,
            PragaProvavel = "Cigarrinha-do-milho (enfezamento)",
            Severidade = 4,
            Municipio = "Ribeirão Preto",
            Uf = "SP",
            Status = StatusFoco.CONFIRMADO,
            DataOcorrencia = new DateTime(2026, 11, 8, 14, 20, 0, DateTimeKind.Utc),
            AtualizadoEm = new DateTime(2026, 11, 9, 9, 10, 0, DateTimeKind.Utc),
            Links =
            [
                new LinkDto { Href = "/api/publico/focos/12", Rel = "self", Method = "GET" },
                new LinkDto { Href = "/api/publico/focos", Rel = "focos", Method = "GET" }
            ]
        };

        public static GetFocoDto Campinas => new()
        {
            Id = 15,
            Cultura = Cultura.TOMATE,
            PragaProvavel = "Requeima",
            Severidade = 3,
            Municipio = "Campinas",
            Uf = "SP",
            Status = StatusFoco.SUSPEITA,
            DataOcorrencia = new DateTime(2026, 11, 7, 10, 5, 0, DateTimeKind.Utc),
            AtualizadoEm = new DateTime(2026, 11, 7, 10, 6, 0, DateTimeKind.Utc),
            Links =
            [
                new LinkDto { Href = "/api/publico/focos/15", Rel = "self", Method = "GET" },
                new LinkDto { Href = "/api/publico/focos", Rel = "focos", Method = "GET" }
            ]
        };
    }
}
