namespace Valence.Observatorio.Application.Configs
{
    public class RateLimitSettings
    {
        public const string Secao = "RateLimiting";

        public LimiteSettings Publica { get; set; } = new() { Limite = 60, JanelaSegundos = 60 };
        public LimiteSettings Login { get; set; } = new() { Limite = 5, JanelaSegundos = 60 };
        public LimiteSettings Escrita { get; set; } = new() { Limite = 20, JanelaSegundos = 60 };

        public bool EhValido() => Publica.EhValido() && Login.EhValido() && Escrita.EhValido();
    }
}
