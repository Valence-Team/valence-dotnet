using Swashbuckle.AspNetCore.Filters;
using Valence.Observatorio.Application.Dtos;

namespace Valence.Observatorio.API.Doc.Samples
{
    public class PostBoletimSample : IExamplesProvider<PostBoletimDto>
    {
        public PostBoletimDto GetExamples() => new()
        {
            Municipio = "Ribeirão Preto",
            Uf = "SP",
            Titulo = "Alerta de cigarrinha-do-milho na região",
            Texto = "Foram confirmados focos de enfezamento do milho em propriedades do município. Vistorie a lavoura, elimine plantas tigueras e procure a Casa da Agricultura para orientação técnica."
        };
    }

    public class GetBoletimSample : IExamplesProvider<GetBoletimDto>
    {
        public GetBoletimDto GetExamples() => Boletins.Exemplo;
    }

    public class PageBoletimSample : IExamplesProvider<PageResponseDto<GetBoletimDto>>
    {
        public PageResponseDto<GetBoletimDto> GetExamples() => new()
        {
            Items = [Boletins.Exemplo],
            Deslocamento = 0,
            RegistroRetornado = 10,
            TotalRegistros = 1,
            Links =
            [
                new LinkDto { Href = "/api/boletins?uf=SP&deslocamento=0&registroRetornado=10", Rel = "self", Method = "GET" }
            ]
        };
    }

    internal static class Boletins
    {
        public static GetBoletimDto Exemplo => new()
        {
            Id = 3,
            Municipio = "Ribeirão Preto",
            Uf = "SP",
            Titulo = "Alerta de cigarrinha-do-milho na região",
            Texto = "Foram confirmados focos de enfezamento do milho em propriedades do município. Vistorie a lavoura, elimine plantas tigueras e procure a Casa da Agricultura para orientação técnica.",
            GestorId = 1,
            CriadoEm = new DateTime(2026, 11, 10, 13, 0, 0, DateTimeKind.Utc),
            AtualizadoEm = null,
            Links =
            [
                new LinkDto { Href = "/api/boletins/3", Rel = "self", Method = "GET" },
                new LinkDto { Href = "/api/boletins/3", Rel = "atualizar", Method = "PUT" },
                new LinkDto { Href = "/api/boletins/3", Rel = "remover", Method = "DELETE" },
                new LinkDto { Href = "/api/boletins", Rel = "boletins", Method = "GET" }
            ]
        };
    }
}
