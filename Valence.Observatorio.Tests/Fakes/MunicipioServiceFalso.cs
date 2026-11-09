using Valence.Observatorio.Domain.Entities;
using Valence.Observatorio.Domain.Errors;
using Valence.Observatorio.Domain.Interfaces;
using Valence.Observatorio.Infrastructure.Services;

namespace Valence.Observatorio.Tests.Fakes
{
    public class MunicipioServiceFalso : IMunicipioService
    {
        public const string MunicipioForaDoAr = "Cidade Fora do Ar";

        private static readonly MunicipioModel[] Municipios =
        [
            new() { Id = 3543402, Nome = "Ribeirão Preto", Uf = "SP" },
            new() { Id = 3509502, Nome = "Campinas", Uf = "SP" },
            new() { Id = 4109401, Nome = "Guarapuava", Uf = "PR" }
        ];

        public Task<MunicipioModel?> ObterAsync(string municipio, string uf)
        {
            if (municipio == MunicipioForaDoAr)
                throw new ExternalServiceException("O serviço de localidades do IBGE está indisponível. Tente novamente em instantes.");

            var nome = NomeNormalizador.Normalizar(municipio);
            var sigla = uf.Trim().ToUpperInvariant();

            var encontrado = Municipios.FirstOrDefault(m => m.Uf == sigla && NomeNormalizador.Normalizar(m.Nome) == nome);

            return Task.FromResult(encontrado);
        }
    }
}
