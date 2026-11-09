using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Valence.Observatorio.Infrastructure.Data;

namespace Valence.Observatorio.Tests.Fakes
{
    public static class BancoEmMemoria
    {
        public static ApplicationContext Criar()
        {
            var opcoes = new DbContextOptionsBuilder<ApplicationContext>()
                .UseInMemoryDatabase($"observatorio-unidade-{Guid.NewGuid()}")
                .ConfigureWarnings(avisos => avisos.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            var contexto = new ApplicationContext(opcoes);
            contexto.Database.EnsureCreated();

            return contexto;
        }
    }
}
