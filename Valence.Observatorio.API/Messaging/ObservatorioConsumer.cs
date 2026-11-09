using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Valence.Observatorio.Application.Configs;
using Valence.Observatorio.Application.Dtos.Eventos;
using Valence.Observatorio.Application.Interfaces;

namespace Valence.Observatorio.API.Messaging
{
    public class ObservatorioConsumer : BackgroundService
    {
        private static readonly TimeSpan EsperaReconexao = TimeSpan.FromSeconds(15);
        private static readonly TimeSpan EsperaReprocessamento = TimeSpan.FromSeconds(5);

        private readonly RabbitMqConexao _conexao;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly RabbitMqSettings _settings;
        private readonly ILogger<ObservatorioConsumer> _logger;

        public ObservatorioConsumer(RabbitMqConexao conexao, IServiceScopeFactory scopeFactory, IOptions<RabbitMqSettings> options, ILogger<ObservatorioConsumer> logger)
        {
            _conexao = conexao;
            _scopeFactory = scopeFactory;
            _settings = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_conexao.Configurada)
            {
                _logger.LogWarning("RabbitMq:Uri não configurada; o consumidor de eventos não foi iniciado");
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ConsumirAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "RabbitMQ indisponível; nova tentativa em {Segundos} s", EsperaReconexao.TotalSeconds);
                }

                try
                {
                    await Task.Delay(EsperaReconexao, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        private async Task ConsumirAsync(CancellationToken stoppingToken)
        {
            var conexao = await _conexao.ObterAsync(stoppingToken);
            await using var canal = await conexao.CreateChannelAsync(cancellationToken: stoppingToken);

            // Topologia
            await canal.ExchangeDeclareAsync(exchange: RabbitMqSettings.Exchange, type: ExchangeType.Topic, durable: true, autoDelete: false, cancellationToken: stoppingToken);
            await canal.QueueDeclareAsync(queue: _settings.Fila, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: stoppingToken);

            foreach (var rota in RabbitMqSettings.RoutingKeys)
                await canal.QueueBindAsync(queue: _settings.Fila, exchange: RabbitMqSettings.Exchange, routingKey: rota, cancellationToken: stoppingToken);

            await canal.BasicQosAsync(prefetchSize: 0, prefetchCount: RabbitMqSettings.Prefetch, global: false, cancellationToken: stoppingToken);

            var encerrado = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            canal.ChannelShutdownAsync += (_, _) =>
            {
                encerrado.TrySetResult();
                return Task.CompletedTask;
            };

            var consumidor = new AsyncEventingBasicConsumer(canal);
            consumidor.ReceivedAsync += (_, mensagem) => TratarAsync(canal, mensagem, stoppingToken);

            await canal.BasicConsumeAsync(queue: _settings.Fila, autoAck: false, consumer: consumidor, cancellationToken: stoppingToken);

            _logger.LogInformation("Consumindo a fila {Fila}", _settings.Fila);

            await encerrado.Task.WaitAsync(stoppingToken);

            _logger.LogWarning("Canal do RabbitMQ encerrado; reconectando");
        }

        private async Task TratarAsync(IChannel canal, BasicDeliverEventArgs mensagem, CancellationToken stoppingToken)
        {
            ResultadoEvento resultado;

            try
            {
                await using var escopo = _scopeFactory.CreateAsyncScope();
                var useCase = escopo.ServiceProvider.GetRequiredService<IEventoUseCase>();
                resultado = await useCase.ProcessarAsync(mensagem.Body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha inesperada ao processar mensagem de {RoutingKey}", mensagem.RoutingKey);
                resultado = ResultadoEvento.FalhaTemporaria;
            }

            switch (resultado)
            {
                case ResultadoEvento.Processado:
                case ResultadoEvento.Duplicado:
                    await canal.BasicAckAsync(mensagem.DeliveryTag, multiple: false, stoppingToken);
                    break;

                case ResultadoEvento.Invalido:
                    await canal.BasicNackAsync(mensagem.DeliveryTag, multiple: false, requeue: false, stoppingToken);
                    break;

                default:
                    await Task.Delay(EsperaReprocessamento, stoppingToken);
                    await canal.BasicNackAsync(mensagem.DeliveryTag, multiple: false, requeue: true, stoppingToken);
                    break;
            }
        }
    }
}
