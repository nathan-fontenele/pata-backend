using Microsoft.EntityFrameworkCore;
using Npgsql;
using Pata.Application.Comum.Abstracoes;
using Pata.Domain.Entidades.Organizacao;
using Pata.Domain.Excecoes;
using Pata.Domain.Repositorios;

namespace Pata.Infrastructure.Persistencia.Repositorios;

internal sealed class RepositorioConviteUsuario(PataDbContext contexto, IContextoTenantDefinivel contextoTenant)
    : IRepositorioConviteUsuario
{
    public async Task<AcessoOrganizacao> AceitarAuth0Async(string tokenHash, string auth0Sub, string email,
        DateTimeOffset agora, CancellationToken cancellationToken = default)
    {
        var convite = await contexto.ConvitesUsuario.IgnoreQueryFilters()
            .SingleOrDefaultAsync(item => item.TokenHash == tokenHash, cancellationToken)
            ?? throw new RecursoNaoEncontradoException("Convite nao encontrado.");
        var usuario = await contexto.Usuarios.IgnoreQueryFilters()
            .SingleOrDefaultAsync(item => item.Id == convite.UsuarioId && item.TenantId == convite.TenantId,
                cancellationToken)
            ?? throw new RecursoNaoEncontradoException("Usuario do convite nao encontrado.");

        if (!string.Equals(usuario.Email, email, StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("O e-mail autenticado nao corresponde ao convite.");

        contextoTenant.DefinirTenant(convite.TenantId);
        convite.Aceitar(agora);
        usuario.VincularIdentidadeAuth0(auth0Sub, agora);
        try
        {
            await contexto.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException postgres
                                                    && postgres.SqlState == PostgresErrorCodes.UniqueViolation
                                                    && postgres.ConstraintName == "IX_usuarios_tenant_id_Auth0Sub")
        {
            throw new ConflitoException("A identidade Auth0 ja esta vinculada a um usuario desta clinica.");
        }

        var organizacao = await contexto.Organizacoes.IgnoreQueryFilters()
            .SingleAsync(item => item.Id == convite.TenantId, cancellationToken);
        return new AcessoOrganizacao(usuario.Id, usuario.TenantId, organizacao.Nome, organizacao.Slug,
            usuario.Perfil, usuario.Status);
    }
}
