using Swashbuckle.AspNetCore.Filters;
using Valence.Observatorio.Application.Dtos;

namespace Valence.Observatorio.API.Doc.Samples
{
    public class ErroSamples : IMultipleExamplesProvider<ErroDto>
    {
        private static readonly DateTime Momento = new(2026, 11, 10, 14, 32, 0, DateTimeKind.Utc);

        public IEnumerable<SwaggerExample<ErroDto>> GetExamples()
        {
            yield return SwaggerExample.Create("400 campos", "Campos inválidos", Erro(400, "Requisição inválida", "Um ou mais campos são inválidos.", "/api/boletins",
                new CampoErroDto { Campo = "titulo", Mensagem = "O título é obrigatório." }));
            yield return SwaggerExample.Create("400 idempotência", "Idempotency-Key ausente", Erro(400, "Requisição inválida", "A requisição não tem uma chave de idempotência válida.", "/api/boletins",
                new CampoErroDto { Campo = "Idempotency-Key", Mensagem = "O cabeçalho Idempotency-Key é obrigatório." }));
            yield return SwaggerExample.Create("401", "Sem token ou credencial errada", Erro(401, "Não autenticado", "Token ausente, inválido ou expirado.", "/api/boletins"));
            yield return SwaggerExample.Create("403", "Papel ou autoria sem permissão", Erro(403, "Acesso negado", "Somente o gestor que publicou o boletim pode alterá-lo ou removê-lo.", "/api/boletins/3"));
            yield return SwaggerExample.Create("404", "Recurso inexistente", Erro(404, "Não encontrado", "Boletim não encontrado.", "/api/boletins/999"));
            yield return SwaggerExample.Create("409", "Chave reutilizada", Erro(409, "Conflito", "Esta Idempotency-Key já foi usada com outro conteúdo. Gere uma nova chave para uma nova publicação.", "/api/boletins"));
            yield return SwaggerExample.Create("422 município", "Município fora do IBGE", Erro(422, "Regra de negócio violada", "Município não encontrado em SP na base de localidades do IBGE.", "/api/boletins"));
            yield return SwaggerExample.Create("422 dados pessoais", "Texto com dado pessoal", Erro(422, "Regra de negócio violada", "O boletim não pode conter dados pessoais (e-mail, telefone ou CPF).", "/api/boletins"));
            yield return SwaggerExample.Create("429", "Limite de requisições", Erro(429, "Muitas requisições", "Limite de requisições excedido. Tente novamente em instantes.", "/api/publico/focos"));
            yield return SwaggerExample.Create("503", "IBGE fora do ar", Erro(503, "Serviço indisponível", "O serviço de localidades do IBGE está indisponível. Tente novamente em instantes.", "/api/boletins"));
        }

        private static ErroDto Erro(int status, string erro, string mensagem, string caminho, params CampoErroDto[] campos) => new()
        {
            Timestamp = Momento,
            Status = status,
            Erro = erro,
            Mensagem = mensagem,
            Caminho = caminho,
            Campos = campos
        };
    }
}
