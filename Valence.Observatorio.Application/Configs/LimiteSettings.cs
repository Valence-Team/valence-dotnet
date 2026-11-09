namespace Valence.Observatorio.Application.Configs
{
    public class LimiteSettings
    {
        public int Limite { get; set; }
        public int JanelaSegundos { get; set; }

        public bool EhValido() => Limite > 0 && JanelaSegundos > 0;
    }
}
