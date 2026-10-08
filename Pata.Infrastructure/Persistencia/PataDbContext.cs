using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Pata.Domain.Comum;
using Pata.Domain.Entidades.Animal;
using Pata.Domain.Entidades.Consulta;
using Pata.Domain.Entidades.Prontuario;
using Pata.Domain.Entidades.Tutor;
using Pata.Domain.Entidades.Veterinario;
using Pata.Domain.Entidades.Organizacao;
using Pata.Domain.Entidades.Cobranca;
using Pata.Domain.Entidades.Auditoria;
using System.Text.Json;
using Pata.Application.Comum.Abstracoes;
using Pata.Domain.ObjetosValor;

namespace Pata.Infrastructure.Persistencia;

public sealed class PataDbContext(
    DbContextOptions<PataDbContext> options,
    TimeProvider timeProvider,
    IContextoTenant contextoTenant,
    IUsuarioAtual usuarioAtual) : DbContext(options)
{
    public DbSet<Tutor> Tutores => Set<Tutor>();
    public DbSet<AcessoPortalTutor> AcessosPortalTutor => Set<AcessoPortalTutor>();
    public DbSet<Veterinario> Veterinarios => Set<Veterinario>();
    public DbSet<Consulta> Consultas => Set<Consulta>();
    public DbSet<Organizacao> Organizacoes => Set<Organizacao>();
    public DbSet<Equipe> Equipes => Set<Equipe>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<ConviteUsuario> ConvitesUsuario => Set<ConviteUsuario>();
    public DbSet<Plano> Planos => Set<Plano>();
    public DbSet<PrecoPlano> PrecosPlano => Set<PrecoPlano>();
    public DbSet<Assinatura> Assinaturas => Set<Assinatura>();
    public DbSet<Fatura> Faturas => Set<Fatura>();
    public DbSet<ItemFatura> ItensFatura => Set<ItemFatura>();
    public DbSet<Pagamento> Pagamentos => Set<Pagamento>();
    public DbSet<Auditoria> Auditorias => Set<Auditoria>();

    public Guid TenantId => contextoTenant.TenantId;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PataDbContext).Assembly);
        modelBuilder.Entity<Tutor>().HasQueryFilter(item => item.TenantId == TenantId && !item.Excluido);
        modelBuilder.Entity<AcessoPortalTutor>().HasQueryFilter(item => item.TenantId == TenantId);
        modelBuilder.Entity<Veterinario>().HasQueryFilter(item => item.TenantId == TenantId && !item.Excluido);
        modelBuilder.Entity<Consulta>().HasQueryFilter(item => item.TenantId == TenantId && !item.Excluido);
        modelBuilder.Entity<Animal>().HasQueryFilter(item => item.TenantId == TenantId);
        modelBuilder.Entity<Organizacao>().HasQueryFilter(item => item.Id == TenantId);
        modelBuilder.Entity<Equipe>().HasQueryFilter(item => item.TenantId == TenantId);
        modelBuilder.Entity<Usuario>().HasQueryFilter(item => item.TenantId == TenantId);
        modelBuilder.Entity<ConviteUsuario>().HasQueryFilter(item => item.TenantId == TenantId);
        modelBuilder.Entity<Assinatura>().HasQueryFilter(item => item.TenantId == TenantId);
        modelBuilder.Entity<Fatura>().HasQueryFilter(item => item.TenantId == TenantId);
        modelBuilder.Entity<ItemFatura>().HasQueryFilter(item => item.TenantId == TenantId);
        modelBuilder.Entity<Pagamento>().HasQueryFilter(item => item.TenantId == TenantId);
        modelBuilder.Entity<Auditoria>().HasQueryFilter(item => item.TenantId == TenantId);
        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges() => SaveChanges(acceptAllChangesOnSuccess: true);

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        PrepararGravacao();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        SaveChangesAsync(acceptAllChangesOnSuccess: true, cancellationToken);

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        PrepararGravacao();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void PrepararGravacao()
    {
        AtualizarAuditoria();
        RegistrarEventosAuditoria();
    }

    private void AtualizarAuditoria()
    {
        foreach (var entry in ChangeTracker.Entries().Where(entry => entry.Entity is ITenantEntity))
        {
            var tenantEntity = (ITenantEntity)entry.Entity;
            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
                continue;

            if (tenantEntity is Organizacao organizacao)
            {
                var tenantEsperado = entry.State == EntityState.Added ? organizacao.Id : TenantId;
                if (organizacao.TenantId != organizacao.Id || organizacao.Id != tenantEsperado)
                    throw new InvalidOperationException("A organizacao nao pertence ao tenant autenticado.");
            }
            else if (tenantEntity.TenantId != TenantId)
            {
                throw new InvalidOperationException("A entidade pertence a outro tenant ou nao possui tenant_id.");
            }
        }

        var agora = timeProvider.GetUtcNow().UtcDateTime;
        foreach (var entry in ChangeTracker.Entries<IAuditavel>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(nameof(IAuditavel.CriadoEm)).CurrentValue = agora;
                entry.Property(nameof(IAuditavel.CriadoPor)).CurrentValue = usuarioAtual.Identificador;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(nameof(IAuditavel.AtualizadoEm)).CurrentValue = agora;
                entry.Property(nameof(IAuditavel.AtualizadoPor)).CurrentValue = "sistema";
            }
        }
    }

    private void RegistrarEventosAuditoria()
    {
        var agora = timeProvider.GetUtcNow();
        var alteracoes = ChangeTracker.Entries()
            .Where(entry => entry.Entity is ITenantEntity
                            && entry.Entity is not Auditoria
                            && (entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
            .ToArray();

        foreach (var entry in alteracoes)
        {
            var tenant = (ITenantEntity)entry.Entity;
            var excluida = entry.Entity is IExcluivel { Excluido: true }
                           && entry.State == EntityState.Modified;
            var acao = entry.State == EntityState.Added
                ? AcaoAuditoria.Criado
                : excluida || entry.State == EntityState.Deleted
                    ? AcaoAuditoria.Excluido
                    : AcaoAuditoria.Editado;
            var antes = entry.State == EntityState.Added ? null : Serializar(entry.OriginalValues);
            var depois = entry.State == EntityState.Deleted ? null : Serializar(entry.CurrentValues);
            var excluivel = entry.Entity as IExcluivel;

            Auditorias.Add(new Auditoria(
                tenant.TenantId,
                (Guid)entry.Property("Id").CurrentValue!,
                entry.Metadata.ClrType.Name,
                acao,
                agora,
                usuarioAtual.Identificador,
                antes,
                depois,
                acao == AcaoAuditoria.Editado ? agora : null,
                acao == AcaoAuditoria.Editado ? usuarioAtual.Identificador : null,
                excluida ? agora : excluivel?.ExcluidoEm is DateTime excluidoEm
                    ? new DateTimeOffset(DateTime.SpecifyKind(excluidoEm, DateTimeKind.Utc)) : null,
                excluida ? usuarioAtual.Identificador : excluivel?.ExcluidoPor));
        }
    }

    private static string Serializar(PropertyValues values) =>
        JsonSerializer.Serialize(values.Properties.ToDictionary(property => property.Name, property => values[property]));
}
