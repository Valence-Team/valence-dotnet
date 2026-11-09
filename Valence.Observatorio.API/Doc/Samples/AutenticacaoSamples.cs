using Swashbuckle.AspNetCore.Filters;
using Valence.Observatorio.Application.Dtos;
using Valence.Observatorio.Domain.Enums;

namespace Valence.Observatorio.API.Doc.Samples
{
    public class PostLoginSample : IExamplesProvider<PostLoginDto>
    {
        public PostLoginDto GetExamples() => new()
        {
            Email = "gestor1@valence.local",
            Senha = "senha-do-usuario-de-teste"
        };
    }

    public class GetLoginSample : IExamplesProvider<GetLoginDto>
    {
        public GetLoginDto GetExamples() => new()
        {
            Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.exemplo.assinatura",
            Tipo = "Bearer",
            ExpiraEm = new DateTime(2026, 11, 10, 16, 32, 0, DateTimeKind.Utc),
            Papel = Papel.GESTOR
        };
    }

    public class GetUsuarioLogadoSample : IExamplesProvider<GetUsuarioLogadoDto>
    {
        public GetUsuarioLogadoDto GetExamples() => new()
        {
            Id = 1,
            Papel = nameof(Papel.GESTOR),
            ExpiraEm = new DateTime(2026, 11, 10, 16, 32, 0, DateTimeKind.Utc)
        };
    }
}
