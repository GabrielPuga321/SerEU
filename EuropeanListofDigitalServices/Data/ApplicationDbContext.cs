using EuropeanListofDigitalServices.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EuropeanListofDigitalServices.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<ServicoDigital> ServicosDigitais { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Avaliacao> Avaliacoes { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Relação muitos-para-muitos: ServicoDigital <-> Tag
        builder.Entity<ServicoDigital>()
            .HasMany(s => s.Tags)
            .WithMany(t => t.Servicos)
            .UsingEntity(j => j.ToTable("ServicoTag"));

        // Relação muitos-para-um: ServicoDigital -> Categoria (impede eliminação em cascata da categoria)
        builder.Entity<ServicoDigital>()
            .HasOne(s => s.Categoria)
            .WithMany(c => c.Servicos)
            .HasForeignKey(s => s.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relação muitos-para-um: Avaliacao -> ServicoDigital (elimina avaliações ao eliminar serviço)
        builder.Entity<Avaliacao>()
            .HasOne(a => a.ServicoDigital)
            .WithMany(s => s.Avaliacoes)
            .HasForeignKey(a => a.ServicoDigitalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
