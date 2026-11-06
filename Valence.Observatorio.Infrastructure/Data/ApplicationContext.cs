using Microsoft.EntityFrameworkCore;
using Valence.Observatorio.Domain.Entities;
using Valence.Observatorio.Domain.Enums;

namespace Valence.Observatorio.Infrastructure.Data
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
        }

        public DbSet<FocoAnonimizadoEntity> Foco { get; set; }
        public DbSet<BoletimEntity> Boletim { get; set; }
        public DbSet<EventoProcessadoEntity> EventoProcessado { get; set; }
        public DbSet<RequisicaoIdempotenteEntity> RequisicaoIdempotente { get; set; }
        public DbSet<UsuarioGestorEntity> UsuarioGestor { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Enums gravados como texto
            modelBuilder.Entity<FocoAnonimizadoEntity>()
                .Property(f => f.Cultura)
                .HasConversion<string>()
                .HasMaxLength(20);

            modelBuilder.Entity<FocoAnonimizadoEntity>()
                .Property(f => f.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            modelBuilder.Entity<UsuarioGestorEntity>()
                .Property(u => u.Papel)
                .HasConversion<string>()
                .HasMaxLength(20);

            // Usuários de teste
            modelBuilder.Entity<UsuarioGestorEntity>().HasData(
                new UsuarioGestorEntity { Id = 1, Email = "gestor1@valence.local", SenhaHash = "$2a$11$K.r20tSB2PUqshozizTX3uCvcteJhr1Lut3CbliyDZYGhtoE842ba", Papel = Papel.GESTOR },
                new UsuarioGestorEntity { Id = 2, Email = "gestor2@valence.local", SenhaHash = "$2a$11$qxwNpdY08uO/ha4OLD8J8.42cg.4V/.9u7WHP98qlnk27Ww0IsAb.", Papel = Papel.GESTOR },
                new UsuarioGestorEntity { Id = 3, Email = "agronomo@valence.local", SenhaHash = "$2a$11$eXJuSbsmKu9hyku7F5CVp.1ZUK7dnb4DQMBqxEk/y19YPdt9plCTS", Papel = Papel.AGRONOMO });
        }
    }
}

