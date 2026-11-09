using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Valence.Observatorio.Application.Dtos;
using Valence.Observatorio.Application.Interfaces;
using Valence.Observatorio.Application.Mappers;
using Valence.Observatorio.Domain.Entities;
using Valence.Observatorio.Domain.Errors;
using Valence.Observatorio.Domain.Interfaces;
using Valence.Observatorio.Domain.Rules;

namespace Valence.Observatorio.Application.UseCases
{
    public class BoletimUseCase : IBoletimUseCase
    {
        public const int StatusCriado = 201;
        public const string MensagemNaoEncontrado = "Boletim não encontrado.";
        public const string MensagemChaveReutilizada = "Esta Idempotency-Key já foi usada com outro conteúdo. Gere uma nova chave para uma nova publicação.";

        private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

        private readonly IBoletimRepository _boletins;
        private readonly IRequisicaoIdempotenteRepository _requisicoes;
        private readonly IMunicipioService _municipios;
        private readonly TimeProvider _tempo;
        private readonly ILogger<BoletimUseCase> _logger;

        public BoletimUseCase(IBoletimRepository boletins, IRequisicaoIdempotenteRepository requisicoes, IMunicipioService municipios, TimeProvider tempo, ILogger<BoletimUseCase> logger)
        {
            _boletins = boletins;
            _requisicoes = requisicoes;
            _municipios = municipios;
            _tempo = tempo;
            _logger = logger;
        }

        public async Task<OperationResult<PageResponseDto<GetBoletimDto>>> ObterTodosAsync(FiltroBoletimDto filtro)
        {
            var deslocamento = filtro.Deslocamento ?? FocoUseCase.DeslocamentoPadrao;
            var registroRetornado = filtro.RegistroRetornado ?? FocoUseCase.RegistroRetornadoPadrao;

            var (data, rowsCount) = await _boletins.ObterTodosAsync(filtro.Uf, filtro.Municipio, deslocamento, registroRetornado);
            var itens = data.Select(b => b.ToDto()).ToList();

            var pagina = new PageResponseDto<GetBoletimDto>
            {
                Items = itens,
                Deslocamento = deslocamento,
                RegistroRetornado = registroRetornado,
                TotalRegistros = rowsCount,
                Links = BoletimMapper.LinksDaPagina(filtro, deslocamento, registroRetornado, rowsCount)
            };

            return itens.Count == 0
                ? OperationResult<PageResponseDto<GetBoletimDto>>.Success(pagina, 204)
                : OperationResult<PageResponseDto<GetBoletimDto>>.Success(pagina);
        }

        public async Task<OperationResult<GetBoletimDto>> ObterUmAsync(int id)
        {
            var boletim = await _boletins.ObterUmAsync(id);

            return boletim is null
                ? OperationResult<GetBoletimDto>.Failure(MensagemNaoEncontrado, 404)
                : OperationResult<GetBoletimDto>.Success(boletim.ToDto());
        }

        public async Task<OperationResult<RespostaIdempotenteDto>> AdicionarAsync(PostBoletimDto dto, int gestorId, Guid idempotencyKey)
        {
            var hash = CalcularHash(dto, gestorId);

            try
            {
                var anterior = await _requisicoes.ObterAsync(idempotencyKey);

                if (anterior is not null)
                    return Repetir(anterior, hash);

                var municipio = await ObterMunicipioAsync(dto.Municipio, dto.Uf);
                var agora = _tempo.GetUtcNow().UtcDateTime;

                var boletim = BoletimEntity.Criar(municipio.Nome, municipio.Uf, dto.Titulo, dto.Texto, gestorId, idempotencyKey, agora);
                var requisicao = new RequisicaoIdempotenteEntity
                {
                    Chave = idempotencyKey,
                    HashCorpo = hash,
                    StatusCode = StatusCriado,
                    CriadaEm = agora
                };

                var salvo = await _boletins.AdicionarComIdempotenciaAsync(boletim, requisicao, b => JsonSerializer.Serialize(b.ToDto(), Json));

                _logger.LogInformation("Boletim {BoletimId} publicado pelo gestor {GestorId}", salvo.Id, gestorId);

                return OperationResult<RespostaIdempotenteDto>.Success(new RespostaIdempotenteDto
                {
                    StatusCode = StatusCriado,
                    Corpo = requisicao.CorpoResposta,
                    Local = BoletimMapper.Rota(salvo.Id),
                    Repetida = false
                }, StatusCriado);
            }
            catch (DbUpdateException)
            {
                var concorrente = await _requisicoes.ObterAsync(idempotencyKey);

                if (concorrente is null)
                    throw;

                _logger.LogInformation("Publicação concorrente com a chave {IdempotencyKey}; resposta original repetida", idempotencyKey);
                return Repetir(concorrente, hash);
            }
            catch (Exception ex) when (ex.EhDeNegocio())
            {
                return ex.ParaFalha<RespostaIdempotenteDto>();
            }
        }

        public async Task<OperationResult<GetBoletimDto>> EditarAsync(int id, PostBoletimDto dto, int gestorId)
        {
            try
            {
                var boletim = await _boletins.ObterUmAsync(id) ?? throw new NoContentException(MensagemNaoEncontrado);

                boletim.GarantirAutor(gestorId);

                var municipio = await ObterMunicipioAsync(dto.Municipio, dto.Uf);
                boletim.Editar(municipio.Nome, municipio.Uf, dto.Titulo, dto.Texto, gestorId, _tempo.GetUtcNow().UtcDateTime);

                var atualizado = await _boletins.EditarAsync(boletim);

                _logger.LogInformation("Boletim {BoletimId} atualizado pelo gestor {GestorId}", atualizado.Id, gestorId);

                return OperationResult<GetBoletimDto>.Success(atualizado.ToDto());
            }
            catch (Exception ex) when (ex.EhDeNegocio())
            {
                return ex.ParaFalha<GetBoletimDto>();
            }
        }

        public async Task<OperationResult<bool>> DeletarAsync(int id, int gestorId)
        {
            try
            {
                var boletim = await _boletins.ObterUmAsync(id) ?? throw new NoContentException(MensagemNaoEncontrado);

                boletim.GarantirAutor(gestorId);
                await _boletins.DeletarAsync(boletim);

                _logger.LogInformation("Boletim {BoletimId} removido pelo gestor {GestorId}", id, gestorId);

                return OperationResult<bool>.Success(true, 204);
            }
            catch (Exception ex) when (ex.EhDeNegocio())
            {
                return ex.ParaFalha<bool>();
            }
        }

        // Regras de apoio
        private async Task<MunicipioModel> ObterMunicipioAsync(string municipio, string uf)
            => await _municipios.ObterAsync(municipio, uf)
               ?? throw new BusinessRuleException($"Município não encontrado em {UfRegras.Normalizar(uf)} na base de localidades do IBGE.");

        private static OperationResult<RespostaIdempotenteDto> Repetir(RequisicaoIdempotenteEntity anterior, string hash)
        {
            if (!anterior.CorrespondeAo(hash))
                return OperationResult<RespostaIdempotenteDto>.Failure(MensagemChaveReutilizada, 409);

            var original = JsonSerializer.Deserialize<GetBoletimDto>(anterior.CorpoResposta, Json);

            return OperationResult<RespostaIdempotenteDto>.Success(new RespostaIdempotenteDto
            {
                StatusCode = anterior.StatusCode,
                Corpo = anterior.CorpoResposta,
                Local = original is null ? BoletimMapper.RotaBoletins : BoletimMapper.Rota(original.Id),
                Repetida = true
            }, anterior.StatusCode);
        }

        private static string CalcularHash(PostBoletimDto dto, int gestorId)
        {
            var conteudo = JsonSerializer.Serialize(new
            {
                gestorId,
                municipio = dto.Municipio.Trim(),
                uf = dto.Uf.Trim().ToUpperInvariant(),
                titulo = dto.Titulo.Trim(),
                texto = dto.Texto.Trim()
            });

            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(conteudo)));
        }
    }
}
