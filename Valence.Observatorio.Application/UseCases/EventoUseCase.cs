using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Valence.Observatorio.Application.Configs;
using Valence.Observatorio.Application.Dtos.Eventos;
using Valence.Observatorio.Application.Interfaces;
using Valence.Observatorio.Domain.Entities;
using Valence.Observatorio.Domain.Enums;
using Valence.Observatorio.Domain.Errors;
using Valence.Observatorio.Domain.Interfaces;

namespace Valence.Observatorio.Application.UseCases
{
    public class EventoUseCase : IEventoUseCase
    {
        private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

        private readonly IFocoRepository _focos;
        private readonly IEventoProcessadoRepository _eventos;
        private readonly TimeProvider _tempo;
        private readonly ILogger<EventoUseCase> _logger;

        public EventoUseCase(IFocoRepository focos, IEventoProcessadoRepository eventos, TimeProvider tempo, ILogger<EventoUseCase> logger)
        {
            _focos = focos;
            _eventos = eventos;
            _tempo = tempo;
            _logger = logger;
        }

        public async Task<ResultadoEvento> ProcessarAsync(ReadOnlyMemory<byte> corpo)
        {
            EventoCabecalhoDto? cabecalho = null;

            try
            {
                cabecalho = Desserializar<EventoCabecalhoDto>(corpo);
                ValidarCabecalho(cabecalho);

                if (await _eventos.ExisteAsync(cabecalho.EventoId))
                {
                    _logger.LogInformation("Evento {EventoId} do tipo {Tipo} já processado; descartado", cabecalho.EventoId, cabecalho.Tipo);
                    return ResultadoEvento.Duplicado;
                }

                var agora = _tempo.GetUtcNow().UtcDateTime;

                var foco = cabecalho.Tipo == RabbitMqSettings.RotaOcorrenciaTriada
                    ? await AplicarTriagemAsync(Desserializar<OcorrenciaTriadaEventoDto>(corpo), agora)
                    : await AplicarLaudoAsync(Desserializar<LaudoEmitidoEventoDto>(corpo), agora);

                var evento = new EventoProcessadoEntity
                {
                    EventoId = cabecalho.EventoId,
                    Tipo = cabecalho.Tipo,
                    ProcessadoEm = agora
                };

                await _focos.SalvarComEventoAsync(foco, evento);

                _logger.LogInformation("Evento {EventoId} do tipo {Tipo} aplicado à ocorrência {OcorrenciaId}; foco {Status}",
                    cabecalho.EventoId, cabecalho.Tipo, foco.OcorrenciaId, foco.Status);

                return ResultadoEvento.Processado;
            }
            catch (JsonException)
            {
                _logger.LogWarning("Mensagem recusada: JSON inválido ou campo com tipo errado");
                return ResultadoEvento.Invalido;
            }
            catch (BusinessRuleException ex)
            {
                _logger.LogWarning("Evento {EventoId} recusado: {Motivo}", cabecalho?.EventoId, ex.Message);
                return ResultadoEvento.Invalido;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogWarning(ex, "Falha ao gravar o evento {EventoId}; a mensagem volta para a fila", cabecalho?.EventoId);
                return ResultadoEvento.FalhaTemporaria;
            }
        }

        // Regras de aplicação
        private async Task<FocoAnonimizadoEntity> AplicarTriagemAsync(OcorrenciaTriadaEventoDto dto, DateTime agora)
        {
            var cultura = ConverterCultura(dto.Cultura);
            var foco = await _focos.ObterPorOcorrenciaAsync(dto.OcorrenciaId);

            if (foco is null)
                return FocoAnonimizadoEntity.CriarPorTriagem(dto.OcorrenciaId, cultura, dto.PragaProvavel ?? string.Empty, dto.Severidade,
                    dto.Municipio ?? string.Empty, dto.Uf ?? string.Empty, ParaUtc(dto.DataOcorrencia), agora);

            foco.AplicarTriagem(dto.PragaProvavel ?? string.Empty, dto.Severidade, dto.Municipio ?? string.Empty, dto.Uf ?? string.Empty,
                ParaUtc(dto.DataOcorrencia), agora);

            return foco;
        }

        private async Task<FocoAnonimizadoEntity> AplicarLaudoAsync(LaudoEmitidoEventoDto dto, DateTime agora)
        {
            var cultura = ConverterCultura(dto.Cultura);
            var decisao = ConverterDecisao(dto.Decisao);
            var foco = await _focos.ObterPorOcorrenciaAsync(dto.OcorrenciaId);

            if (foco is null)
                return FocoAnonimizadoEntity.CriarPorLaudo(dto.OcorrenciaId, cultura, decisao, dto.PragaConfirmada,
                    dto.Municipio ?? string.Empty, dto.Uf ?? string.Empty, ParaUtc(dto.EmitidoEm), agora);

            foco.AplicarLaudo(decisao, dto.PragaConfirmada, agora);

            return foco;
        }

        // Validações da mensagem
        private static void ValidarCabecalho(EventoCabecalhoDto cabecalho)
        {
            if (cabecalho.EventoId == Guid.Empty)
                throw new BusinessRuleException("eventoId ausente.");

            if (!RabbitMqSettings.RoutingKeys.Contains(cabecalho.Tipo))
                throw new BusinessRuleException($"tipo de evento não tratado: {cabecalho.Tipo}.");

            if (cabecalho.Versao != RabbitMqSettings.VersaoSuportada)
                throw new BusinessRuleException($"versão {cabecalho.Versao} não suportada.");
        }

        private static Cultura ConverterCultura(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor) || !Enum.GetNames<Cultura>().Contains(valor.Trim(), StringComparer.OrdinalIgnoreCase))
                throw new BusinessRuleException("cultura inválida.");

            return Enum.Parse<Cultura>(valor.Trim(), ignoreCase: true);
        }

        private static DecisaoLaudo ConverterDecisao(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor) || !Enum.GetNames<DecisaoLaudo>().Contains(valor.Trim(), StringComparer.OrdinalIgnoreCase))
                throw new BusinessRuleException("decisão do laudo inválida.");

            return Enum.Parse<DecisaoLaudo>(valor.Trim(), ignoreCase: true);
        }

        private static T Desserializar<T>(ReadOnlyMemory<byte> corpo)
            => JsonSerializer.Deserialize<T>(corpo.Span, Json) ?? throw new JsonException("Corpo vazio.");

        private static DateTime ParaUtc(DateTime data) => data.Kind switch
        {
            DateTimeKind.Utc => data,
            DateTimeKind.Local => data.ToUniversalTime(),
            _ => DateTime.SpecifyKind(data, DateTimeKind.Utc)
        };
    }
}
