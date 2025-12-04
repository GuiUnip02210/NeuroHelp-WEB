using Microsoft.EntityFrameworkCore;
using SistemaChamados.Shared.Entities;

namespace SistemaChamados.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<ChamadoHistorico> ChamadosHistorico { get; set; }
    
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
            entity.Property(e => e.DataCadastro).IsRequired().HasDefaultValueSql("NOW()");
            entity.Property(e => e.Ativo).IsRequired().HasDefaultValue(true);
        });
        
        // Configuração da entidade ChamadoHistorico
        modelBuilder.Entity<ChamadoHistorico>(entity =>
        {
            entity.HasKey(e => e.IdDoCaso);
            entity.Property(e => e.DataAbertura).IsRequired();
        });
    }
}