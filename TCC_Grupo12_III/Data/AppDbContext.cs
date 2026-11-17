using Microsoft.EntityFrameworkCore;
using TCC_Grupo12_III.Models;

namespace TCC_Grupo12_III.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Especie> Especies { get; set; }
        public DbSet<Denuncia> Denuncias { get; set; }
        public DbSet<Campanha> Campanhas { get; set; }
        public DbSet<CampanhaParticipacao> CampanhaParticipacoes { get; set; }
        public DbSet<NivelJogo> NivelJogos { get; set; }
        public DbSet<JogoTentativa> JogoTentativas { get; set; }
        public DbSet<ForumTopico> ForumTopicos { get; set; }
        public DbSet<ForumComunidade> ForumComunidades { get; set; }
        public DbSet<ForumResposta> ForumRespostas { get; set; }
        public DbSet<ForumCurtidaTopico> ForumCurtidasTopico { get; set; }
        public DbSet<ForumCurtidaResposta> ForumCurtidasResposta { get; set; }
        public DbSet<Avistamento> Avistamentos { get; set; }
        public DbSet<AvistamentoMidia> AvistamentoMidias { get; set; }
        public DbSet<EspecieQuizPergunta> EspecieQuizPerguntas { get; set; }
        public DbSet<PokedexRegistro> PokedexRegistros { get; set; }
        public DbSet<ReservaAvistamento> ReservasAvistamento { get; set; }
        public DbSet<NotaEstudo> NotasEstudo { get; set; }
        public DbSet<EstudoCientifico> EstudosCientificos { get; set; }
        public DbSet<FaqTicket> FaqTickets { get; set; }
        public DbSet<FaqMensagem> FaqMensagens { get; set; }

        public DbSet<JornadaPergunta> JornadaPerguntas { get; set; }
        public DbSet<JornadaTentativa> JornadaTentativas { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            JornadaModelSnapshot.Configurar(modelBuilder);

            modelBuilder.Entity<PokedexRegistro>()
                .HasOne(r => r.Usuario)
                .WithMany()
                .HasForeignKey(r => r.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PokedexRegistro>()
                .HasOne(r => r.Especie)
                .WithMany()
                .HasForeignKey(r => r.EspecieId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PokedexRegistro>()
                .HasIndex(r => new { r.UsuarioId, r.EspecieId })
                .IsUnique();

            modelBuilder.Entity<EspecieQuizPergunta>()
                .HasOne(p => p.Especie)
                .WithMany()
                .HasForeignKey(p => p.EspecieId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<JogoTentativa>()
                .HasOne(t => t.Nivel)
                .WithMany()
                .HasForeignKey(t => t.NivelJogoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<JogoTentativa>()
                .HasOne(t => t.Usuario)
                .WithMany()
                .HasForeignKey(t => t.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<JogoTentativa>()
                .HasIndex(t => new { t.UsuarioId, t.NivelJogoId, t.Dia })
                .IsUnique();

            modelBuilder.Entity<ForumTopico>()
                .HasOne(t => t.Usuario)
                .WithMany()
                .HasForeignKey(t => t.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ForumComunidade>()
                .HasOne(c => c.Criador)
                .WithMany()
                .HasForeignKey(c => c.CriadorId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ForumComunidade>()
                .HasIndex(c => c.Slug)
                .IsUnique();

            modelBuilder.Entity<ForumTopico>()
                .HasOne(t => t.Comunidade)
                .WithMany(c => c.Topicos)
                .HasForeignKey(t => t.ComunidadeId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ForumResposta>()
                .HasOne(r => r.ForumTopico)
                .WithMany(t => t.Respostas)
                .HasForeignKey(r => r.ForumTopicoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ForumResposta>()
                .HasOne(r => r.Usuario)
                .WithMany()
                .HasForeignKey(r => r.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ForumCurtidaTopico>()
                .HasOne(c => c.ForumTopico)
                .WithMany(t => t.Curtidas)
                .HasForeignKey(c => c.ForumTopicoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ForumCurtidaTopico>()
                .HasOne(c => c.Usuario)
                .WithMany()
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ForumCurtidaTopico>()
                .HasIndex(c => new { c.ForumTopicoId, c.UsuarioId })
                .IsUnique();

            modelBuilder.Entity<ForumCurtidaResposta>()
                .HasOne(c => c.ForumResposta)
                .WithMany(r => r.Curtidas)
                .HasForeignKey(c => c.ForumRespostaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ForumCurtidaResposta>()
                .HasOne(c => c.Usuario)
                .WithMany()
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ForumCurtidaResposta>()
                .HasIndex(c => new { c.ForumRespostaId, c.UsuarioId })
                .IsUnique();

            modelBuilder.Entity<Avistamento>()
                .HasOne(a => a.Especie)
                .WithMany()
                .HasForeignKey(a => a.EspecieId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Avistamento>()
                .HasOne(a => a.Usuario)
                .WithMany()
                .HasForeignKey(a => a.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<AvistamentoMidia>()
                .HasOne(m => m.Avistamento)
                .WithMany(a => a.Midias)
                .HasForeignKey(m => m.AvistamentoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Campanha>()
                .HasOne(c => c.Usuario)
                .WithMany()
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Campanha>()
                .HasOne(c => c.Comunidade)
                .WithMany(com => com.Campanhas)
                .HasForeignKey(c => c.ComunidadeId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<CampanhaParticipacao>()
                .HasOne(p => p.Campanha)
                .WithMany()
                .HasForeignKey(p => p.CampanhaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CampanhaParticipacao>()
                .HasOne(p => p.Usuario)
                .WithMany()
                .HasForeignKey(p => p.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CampanhaParticipacao>()
                .HasIndex(p => new { p.CampanhaId, p.UsuarioId })
                .IsUnique();

            modelBuilder.Entity<ReservaAvistamento>()
                .HasOne(r => r.Usuario)
                .WithMany()
                .HasForeignKey(r => r.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ReservaAvistamento>()
                .HasOne(r => r.Especie)
                .WithMany()
                .HasForeignKey(r => r.EspecieId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ReservaAvistamento>()
                .HasIndex(r => new { r.UsuarioId, r.EspecieId })
                .IsUnique();

            modelBuilder.Entity<NotaEstudo>()
                .HasOne(n => n.Usuario)
                .WithMany()
                .HasForeignKey(n => n.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EstudoCientifico>()
                .HasOne(e => e.Usuario)
                .WithMany()
                .HasForeignKey(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EstudoCientifico>()
                .HasOne(e => e.Especie)
                .WithMany()
                .HasForeignKey(e => e.EspecieId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<FaqTicket>()
                .HasOne(t => t.Usuario)
                .WithMany()
                .HasForeignKey(t => t.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FaqMensagem>()
                .HasOne(m => m.FaqTicket)
                .WithMany(t => t.Mensagens)
                .HasForeignKey(m => m.FaqTicketId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FaqMensagem>()
                .HasOne(m => m.Usuario)
                .WithMany()
                .HasForeignKey(m => m.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
