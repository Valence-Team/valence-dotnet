namespace Valence.Observatorio.Tests.Fakes
{
    public class TempoFixo : TimeProvider
    {
        public static readonly DateTime Agora = new(2026, 11, 10, 12, 0, 0, DateTimeKind.Utc);

        private readonly DateTimeOffset _agora;

        public TempoFixo() : this(Agora)
        {
        }

        public TempoFixo(DateTime agora)
        {
            _agora = new DateTimeOffset(DateTime.SpecifyKind(agora, DateTimeKind.Utc));
        }

        public override DateTimeOffset GetUtcNow() => _agora;
    }
}
