using Microsoft.EntityFrameworkCore;
using SistemaChamados.Shared.Entities;

namespace SistemaChamados.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    
    public DbSet<Usuario> Usuarios { get; set; }
    // DbSet<AlunoPerfil> removido
    // DbSet<ProfessorPerfil> removido
    public DbSet<Chamado> Chamados { get; set; }
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Prioridade> Prioridades { get; set; }
    public DbSet<Status> Status { get; set; }
    public DbSet<Comentario> Comentarios { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configuração da entidade Usuario
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.NomeCompleto).IsRequired().HasMaxLength(150);
            entity.Property(e => e.SenhaHash).IsRequired().HasMaxLength(255);
            entity.Property(e => e.TipoUsuario).IsRequired();
            entity.Property(e => e.DataCadastro).IsRequired().HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.Ativo).IsRequired().HasDefaultValue(true);
        });
        
        // (Bloco de AlunoPerfil removido)
        
        // (Bloco de ProfessorPerfil removido)
        
        // Configuração da entidade Chamado
        modelBuilder.Entity<Chamado>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Titulo).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Descricao).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.DataAbertura).IsRequired().HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.SolicitanteId).IsRequired();
            entity.Property(e => e.CategoriaId).IsRequired();
            entity.Property(e => e.PrioridadeId).IsRequired();
            entity.Property(e => e.StatusId).IsRequired();
        });
        
        // Configuração da entidade Categoria
        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Descricao).HasMaxLength(500);
            entity.Property(e => e.Ativo).IsRequired().HasDefaultValue(true);
            entity.Property(e => e.DataCadastro).IsRequired().HasDefaultValueSql("GETDATE()");
        });
        
        // Configuração da entidade Prioridade
        modelBuilder.Entity<Prioridade>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Descricao).HasMaxLength(200);
            entity.Property(e => e.Nivel).IsRequired();
            entity.Property(e => e.Ativo).IsRequired().HasDefaultValue(true);
            entity.Property(e => e.DataCadastro).IsRequired().HasDefaultValueSql("GETDATE()");
        });
        
        // Configuração da entidade Status
        modelBuilder.Entity<Status>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Descricao).HasMaxLength(200);
            entity.Property(e => e.Ativo).IsRequired().HasDefaultValue(true);
            entity.Property(e => e.DataCadastro).IsRequired().HasDefaultValueSql("GETDATE()");
        });
        
        // Configura o relacionamento do Chamado com o Usuário Solicitante
        modelBuilder.Entity<Chamado>()
            .HasOne(c => c.Solicitante)
            .WithMany() // Um usuário pode ter muitos chamados
            .HasForeignKey(c => c.SolicitanteId)
            .OnDelete(DeleteBehavior.Restrict); // Impede que um usuário seja deletado se tiver chamados
            
        // Configura o relacionamento do Chamado com o Usuário Técnico (opcional)
        modelBuilder.Entity<Chamado>()
            .HasOne(c => c.Tecnico)
            .WithMany()
            .HasForeignKey(c => c.TecnicoId)
            .OnDelete(DeleteBehavior.Restrict) // Impede que um técnico seja deletado se estiver associado a chamados
            .IsRequired(false); // Torna o relacionamento opcional
            
        // Configura o relacionamento do Chamado com Categoria
        modelBuilder.Entity<Chamado>()
            .HasOne(c => c.Categoria)
            .WithMany(cat => cat.Chamados)
            .HasForeignKey(c => c.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // Configura o relacionamento do Chamado com Prioridade
        modelBuilder.Entity<Chamado>()
            .HasOne(c => c.Prioridade)
            .WithMany(p => p.Chamados)
            .HasForeignKey(c => c.PrioridadeId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // Configura o relacionamento do Chamado com Status
        modelBuilder.Entity<Chamado>()
            .HasOne(c => c.Status)
            .WithMany(s => s.Chamados)
            .HasForeignKey(c => c.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configuração da entidade Comentario
        modelBuilder.Entity<Comentario>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Texto).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.DataCriacao).IsRequired().HasDefaultValueSql("GETDATE()");

            entity.HasOne(c => c.Chamado)
                  .WithMany() // Um chamado pode ter muitos comentários
                  .HasForeignKey(c => c.ChamadoId)
                  .OnDelete(DeleteBehavior.Cascade); // Deletar comentários se o chamado for deletado

            entity.HasOne(c => c.Usuario)
                  .WithMany() // Um usuário pode fazer muitos comentários
                  .HasForeignKey(c => c.UsuarioId)
                  .OnDelete(DeleteBehavior.Restrict); // Não deletar usuário se ele tiver comentários
        });
    }
}