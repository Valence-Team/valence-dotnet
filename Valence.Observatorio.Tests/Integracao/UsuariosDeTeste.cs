namespace Valence.Observatorio.Tests.Integracao
{
    public static class UsuariosDeTeste
    {
        public const string Senha = "Teste@Integracao1";

        public static readonly (int Id, string Email) Gestor = (101, "gestor.teste@valence.local");
        public static readonly (int Id, string Email) OutroGestor = (102, "gestor2.teste@valence.local");
        public static readonly (int Id, string Email) Agronomo = (103, "agronomo.teste@valence.local");
    }
}
