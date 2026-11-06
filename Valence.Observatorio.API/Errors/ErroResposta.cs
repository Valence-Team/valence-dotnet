using Valence.Observatorio.Application.Dtos;

namespace Valence.Observatorio.API.Errors
{
    public static class ErroResposta
    {
        public static ErroDto Criar(HttpContext http, int status, string? mensagem = null, IEnumerable<CampoErroDto>? campos = null)
        {
            var agora = DateTime.UtcNow;

            return new ErroDto
            {
                Timestamp = new DateTime(agora.Ticks - agora.Ticks % TimeSpan.TicksPerSecond, DateTimeKind.Utc),
                Status = status,
                Erro = Titulo(status),
                Mensagem = string.IsNullOrWhiteSpace(mensagem) ? MensagemPadrao(status) : mensagem,
                Caminho = http.Request.Path.Value ?? string.Empty,
                Campos = campos ?? []
            };
        }

        public static string Titulo(int status) => status switch
        {
            400 => "Requisição inválida",
            401 => "Não autenticado",
            403 => "Acesso negado",
            404 => "Não encontrado",
            405 => "Método não permitido",
            409 => "Conflito",
            415 => "Tipo de conteúdo não suportado",
            422 => "Regra de negócio violada",
            429 => "Muitas requisições",
            503 => "Serviço indisponível",
            >= 500 => "Erro interno",
            _ => "Erro"
        };

        public static string MensagemPadrao(int status) => status switch
        {
            400 => "A requisição contém dados inválidos.",
            401 => "Token ausente, inválido ou expirado.",
            403 => "Você não tem permissão para acessar este recurso.",
            404 => "Recurso não encontrado.",
            405 => "Método HTTP não permitido para este recurso.",
            409 => "A requisição conflita com o estado atual do recurso.",
            415 => "Envie o corpo em JSON (application/json).",
            422 => "A requisição viola uma regra de negócio.",
            429 => "Limite de requisições excedido. Tente novamente em instantes.",
            503 => "Um serviço necessário está indisponível. Tente novamente mais tarde.",
            >= 500 => "Ocorreu um erro inesperado. Tente novamente mais tarde.",
            _ => "Não foi possível processar a requisição."
        };
    }
}
