using GameficandoEstudos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Pgvector;

namespace GameficandoEstudos.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Materia> Materias { get; set; }
        public DbSet<CaixaFontes> CaixaFontes { get; set; }
        public DbSet<FonteChunks> FonteChunks { get; set; }
        public DbSet<Atividade> Atividades { get; set; }
        public DbSet<SessaoPratica> SessoesPratica { get; set; }
        public DbSet<TentativaResposta> TentativasResposta { get; set; }
        public DbSet<LogAuditoria> LogsAuditoria { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasPostgresExtension("vector");

            modelBuilder.Entity<Usuario>(b =>
            {
                b.HasKey(u => u.Id);
                b.HasIndex(u => u.Email).IsUnique(); 
                b.Property(u => u.Email).IsRequired().HasMaxLength(150);
                b.Property(u => u.Nome).IsRequired().HasMaxLength(100);
                b.Property(u => u.Senha).IsRequired();
                b.Property(u => u.TokenRedefinicaoSenhaHash).HasMaxLength(100);

                b.OwnsOne(u => u.Gamificacao, g =>
                {
                    g.Property(p => p.OfensivaAtual).HasColumnName("Gamificacao_OfensivaAtual");
                    g.Property(p => p.MaiorOfensiva).HasColumnName("Gamificacao_MaiorOfensiva");
                    g.Property(p => p.AtividadesFeitasHoje).HasColumnName("Gamificacao_AtividadesFeitasHoje");
                    g.Property(p => p.UltimaAtividadeRealizadaEm).HasColumnName("Gamificacao_UltimaAtividadeEm");
                });
            });

            modelBuilder.Entity<Materia>(b =>
            {
                b.HasKey(m => m.Id);
                b.HasIndex(m => m.Nome); 
                b.Property(m => m.Nome).IsRequired().HasMaxLength(100);
                b.Property(m => m.Dificuldade).IsRequired().HasMaxLength(50);

                b.HasOne(m => m.Professor)
                 .WithMany()
                 .HasForeignKey(m => m.IdProfessor)
                 .OnDelete(DeleteBehavior.Restrict); 
            });

            modelBuilder.Entity<Atividade>(b =>
            {
                b.HasKey(a => a.Id);
                b.Property(a => a.Nome).IsRequired().HasMaxLength(150);
                b.Property(a => a.Conteudo).IsRequired();
                b.Property(a => a.Gabarito).IsRequired();

                b.HasOne(a => a.Materia)
                 .WithMany()
                 .HasForeignKey(a => a.IdMateria)
                 .OnDelete(DeleteBehavior.Restrict); 
            });

            modelBuilder.Entity<CaixaFontes>(b =>
            {
                b.HasKey(c => c.Id);
                b.Property(c => c.Nome).IsRequired().HasMaxLength(100);

                b.HasOne(c => c.Materia)
                 .WithOne()
                 .HasForeignKey<CaixaFontes>(c => c.IdMateria)
                 .OnDelete(DeleteBehavior.Cascade);

                b.Metadata.FindNavigation(nameof(GameficandoEstudos.Domain.Entities.CaixaFontes.Chunks))!
                    .SetPropertyAccessMode(PropertyAccessMode.Field);
            });

            modelBuilder.Entity<FonteChunks>(b =>
            {
                b.HasKey(f => f.Id);
                b.Property(f => f.Conteudo).IsRequired();
                b.Property(f => f.Origem).IsRequired().HasMaxLength(200);

                b.Property(f => f.Embeddings)
                 .HasColumnType("vector(768)")
                 .HasConversion(
                     v => new Vector(v),    
                     v => v.ToArray()    
                 );

                b.HasOne(f => f.CaixaFonte)
                 .WithMany(c => c.Chunks)
                 .HasForeignKey(f => f.IdCaixaFonte)
                 .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<SessaoPratica>(b =>
            {
                b.HasKey(s => s.Id);

                b.HasOne(s => s.Usuario).WithMany().HasForeignKey(s => s.IdUsuario).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(s => s.Materia).WithMany().HasForeignKey(s => s.IdMateria).OnDelete(DeleteBehavior.Restrict);

                b.HasMany(s => s.Tentativas)
                 .WithOne()
                 .HasForeignKey(t => t.IdSessaoPratica)
                 .OnDelete(DeleteBehavior.Cascade); 

                b.Navigation(s => s.Tentativas).UsePropertyAccessMode(PropertyAccessMode.Field);
            });

            modelBuilder.Entity<TentativaResposta>(b =>
            {
                b.HasKey(t => t.Id);
                b.Property(t => t.RespostaAluno).IsRequired();

                b.HasOne(t => t.Atividade)
                 .WithMany()
                 .HasForeignKey(t => t.IdAtividade)
                 .OnDelete(DeleteBehavior.Restrict); 
            });
        }
    }
}