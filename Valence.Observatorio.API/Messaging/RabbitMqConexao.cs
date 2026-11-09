using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Valence.Observatorio.Application.Configs;

namespace Valence.Observatorio.API.Messaging
{
    public sealed class RabbitMqConexao : IAsyncDisposable
    {
        private readonly SemaphoreSlim _trava = new(1, 1);
        private readonly RabbitMqSettings _settings;
        private IConnection? _conexao;

        public RabbitMqConexao(IOptions<RabbitMqSettings> options)
        {
            _settings = options.Value;
        }

        public bool Configurada => !string.IsNullOrWhiteSpace(_settings.Uri);

        public async Task<IConnection> ObterAsync(CancellationToken cancellationToken = default)
        {
            if (_conexao is { IsOpen: true })
                return _conexao;

            await _trava.WaitAsync(cancellationToken);

            try
            {
                if (_conexao is { IsOpen: true })
                    return _conexao;

                if (_conexao is not null)
                    await _conexao.DisposeAsync();

                var fabrica = new ConnectionFactory
                {
                    Uri = new Uri(_settings.Uri),
                    ClientProvidedName = "valence-observatorio",
                    AutomaticRecoveryEnabled = false,
                    TopologyRecoveryEnabled = false,
                    RequestedConnectionTimeout = TimeSpan.FromSeconds(10)
                };

                _conexao = await fabrica.CreateConnectionAsync(cancellationToken);
                return _conexao;
            }
            finally
            {
                _trava.Release();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_conexao is not null)
                await _conexao.DisposeAsync();

            _trava.Dispose();
        }
    }
}
