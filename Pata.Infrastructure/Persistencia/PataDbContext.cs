using Microsoft.EntityFrameworkCore;
using Pata.Domain.Comum;
using Pata.Domain.Entidades.Animal;
using Pata.Domain.Entidades.Consulta;
using Pata.Domain.Entidades.Prontuario;
using Pata.Domain.Entidades.Tutor;
using Pata.Domain.Entidades.Veterinario;
using Pata.Domain.ObjetosValor;

namespace Pata.Infrastructure.Persistencia;

public sealed class PataDbContext(
    DbContextOptions<PataDbContext> options,
    TimeProvider timeProvider) : DbContext(options)
{
    public DbSet<Tutor> Tutores => Set<Tutor>();
    public DbSet<Veterinario> Veterinarios => Set<Veterinario>();
    public DbSet<Consulta> Consultas => Set<Consulta>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PataDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AtualizarAuditoria();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void AtualizarAuditoria()
    {
        var agora = timeProvider.GetUtcNow().UtcDateTime;
        foreach (var entry in ChangeTracker.Entries<IAuditavel>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(nameof(IAuditavel.CriadoEm)).CurrentValue = agora;
                entry.Property(nameof(IAuditavel.CriadoPor)).CurrentValue = "sistema";
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(nameof(IAuditavel.AtualizadoEm)).CurrentValue = agora;
                entry.Property(nameof(IAuditavel.AtualizadoPor)).CurrentValue = "sistema";
            }
        }
    }
}
