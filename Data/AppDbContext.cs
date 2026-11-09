using Microsoft.EntityFrameworkCore;
using ZeladoriaUrbana.API.Models;

namespace ZeladoriaUrbana.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Ocorrencia> Ocorrencias => Set<Ocorrencia>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<Ocorrencia>()
            .HasOne(o => o.Categoria)
            .WithMany()
            .HasForeignKey(o => o.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
