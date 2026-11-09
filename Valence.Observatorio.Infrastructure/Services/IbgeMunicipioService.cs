using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestSharp;
using Valence.Observatorio.Domain.Entities;
using Valence.Observatorio.Domain.Errors;
using Valence.Observatorio.Domain.Interfaces;
using Valence.Observatorio.Domain.Rules;
using Valence.Observatorio.Infrastructure.Configs;

namespace Valence.Observatorio.Infrastructure.Services
{
    public sealed class IbgeMunicipioService : IMunicipioService, IDisposable
    {
        private const string MensagemIndisponivel = "O serviço de localidades do IBGE está indisponível. Tente novamente em instantes.";
        private static readonly TimeSpan TempoDeCache = TimeSpan.FromHours(24);

        private readonly RestClient _client;
        private readonly IMemoryCache _cache;
        private readonly ILogger<IbgeMunicipioService> _logger;

        public IbgeMunicipioService(IOptions<IbgeSettings> options, IMemoryCache cache, ILogger<IbgeMunicipioService> logger)
        {
            var settings = options.Value;

            _client = new RestClient(new RestClientOptions(settings.ObterBaseUrl())
            {
                Timeout = TimeSpan.FromSeconds(settings.TimeoutSegundos)
            });
            _cache = cache;
            _logger = logger;
        }

        public async Task<MunicipioModel?> ObterAsync(string municipio, string uf)
        {
            if (string.IsNullOrWhiteSpace(municipio) || !UfRegras.EhValida(uf))
                return null;

            var sigla = UfRegras.Normalizar(uf);
            var procurado = NomeNormalizador.Normalizar(municipio);
            var municipios = await ObterMunicipiosDaUfAsync(sigla);

            var encontrado = municipios.FirstOrDefault(m => NomeNormalizador.Normalizar(m.Nome) == procurado);

            return encontrado is null
                ? null
                : new MunicipioModel { Id = encontrado.Id, Nome = encontrado.Nome, Uf = sigla };
        }

        private async Task<IReadOnlyList<IbgeMunicipioResposta>> ObterMunicipiosDaUfAsync(string uf)
        {
            var chave = $"ibge-municipios-{uf}";

            if (_cache.TryGetValue(chave, out IReadOnlyList<IbgeMunicipioResposta>? emCache) && emCache is not null)
                return emCache;

            var requisicao = new RestRequest("localidades/estados/{uf}/municipios").AddUrlSegment("uf", uf);
            var resposta = await _client.ExecuteGetAsync<List<IbgeMunicipioResposta>>(requisicao);

            if (!resposta.IsSuccessful || resposta.Data is null || resposta.Data.Count == 0)
            {
                _logger.LogWarning(resposta.ErrorException, "Consulta de municípios da UF {Uf} ao IBGE falhou com status {Status}", uf, (int)resposta.StatusCode);
                throw new ExternalServiceException(MensagemIndisponivel, resposta.ErrorException);
            }

            IReadOnlyList<IbgeMunicipioResposta> municipios = resposta.Data;
            _cache.Set(chave, municipios, TempoDeCache);

            return municipios;
        }

        public void Dispose() => _client.Dispose();
    }
}
