using Microsoft.EntityFrameworkCore;
using Npgsql;
using Pata.Domain.Entidades.Organizacao;
using Pata.Domain.Excecoes;
using Pata.Domain.Repositorios;

namespace Pata.Infrastructure.Persistencia.Repositorios;

internal sealed class RepositorioOrganizacao(PataDbContext contexto) : IRepositorioOrganizacao
{
    public Task<bool> ExisteSlugAsync(string slug, CancellationToken cancellationToken = default) =>
        contexto.Organizacoes.IgnoreQueryFilters().AnyAsync(item => item.Slug == slug, cancellationToken);

    public async Task CriarComPrimeiroAdminAsync(Organizacao organizacao, Equipe equipe, Usuario admin,
        CancellationToken cancellationToken = default)
    {
        await contexto.Organizacoes.AddAsync(organizacao, cancellationToken);
        await contexto.Equipes.AddAsync(equipe, cancellationToken);
        await contexto.Usuarios.AddAsync(admin, cancellationToken);
        try
        {
            await contexto.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException postgres
                                                    && postgres.SqlState == PostgresErrorCodes.UniqueViolation
                                                    && postgres.ConstraintName == "IX_organizacoes_Slug")
        {
            throw new ConflitoException("Ja existe uma organizacao com o slug informado.");
        }
    }

    public async Task<IReadOnlyList<AcessoOrganizacao>> ListarAcessosAuth0Async(string auth0Sub,
        CancellationToken cancellationToken = default) =>
        await contexto.Usuarios.IgnoreQueryFilters()
            .Where(item => item.Auth0Sub == auth0Sub && item.Status == StatusUsuario.Ativo)
            .Join(contexto.Organizacoes.IgnoreQueryFilters(), usuario => usuario.TenantId,
                organizacao => organizacao.Id,
                (usuario, organizacao) => new AcessoOrganizacao(usuario.Id, usuario.TenantId,
                    organizacao.Nome, organizacao.Slug, usuario.Perfil, usuario.Status))
            .ToListAsync(cancellationToken);

    public Task<AcessoOrganizacao?> ObterAcessoAuth0Async(string auth0Sub, Guid tenantId,
        CancellationToken cancellationToken = default) =>
        contexto.Usuarios.IgnoreQueryFilters()
            .Where(item => item.Auth0Sub == auth0Sub && item.TenantId == tenantId && item.Status == StatusUsuario.Ativo)
            .Join(contexto.Organizacoes.IgnoreQueryFilters(), usuario => usuario.TenantId,
                organizacao => organizacao.Id,
                (usuario, organizacao) => new AcessoOrganizacao(usuario.Id, usuario.TenantId,
                    organizacao.Nome, organizacao.Slug, usuario.Perfil, usuario.Status))
            .SingleOrDefaultAsync(cancellationToken);
}
