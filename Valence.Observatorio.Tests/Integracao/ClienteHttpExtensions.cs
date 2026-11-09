using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Valence.Observatorio.API.Extensions;
using Valence.Observatorio.Application.Dtos;

namespace Valence.Observatorio.Tests.Integracao
{
    public static class ClienteHttpExtensions
    {
        public static readonly JsonSerializerOptions Json = CriarOpcoes();

        private static JsonSerializerOptions CriarOpcoes()
        {
            var opcoes = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            JsonExtensions.Configurar(opcoes);
            return opcoes;
        }

        public static async Task<string> ObterTokenAsync(this HttpClient cliente, string email)
        {
            var resposta = await cliente.PostAsJsonAsync("/api/auth/login", new PostLoginDto { Email = email, Senha = UsuariosDeTeste.Senha });
            resposta.EnsureSuccessStatusCode();

            var login = await resposta.Content.ReadFromJsonAsync<GetLoginDto>(Json);
            return login!.Token;
        }

        public static void Autenticar(this HttpRequestMessage requisicao, string token)
            => requisicao.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        public static async Task<T> LerAsync<T>(this HttpResponseMessage resposta)
            => (await resposta.Content.ReadFromJsonAsync<T>(Json))!;
    }
}
