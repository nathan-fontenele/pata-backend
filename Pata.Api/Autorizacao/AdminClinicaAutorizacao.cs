using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Pata.Application.Comum.Abstracoes;
using Pata.Domain.Entidades.Organizacao;
using Pata.Infrastructure.Persistencia;

namespace Pata.Api.Autorizacao;

internal sealed class AdminClinicaRequisito : IAuthorizationRequirement { }

internal sealed class AdminClinicaHandler(
    PataDbContext contexto,
    IContextoTenant contextoTenant,
    IUsuarioAtual usuarioAtual) : AuthorizationHandler<AdminClinicaRequisito>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        AdminClinicaRequisito requirement)
    {
        try
        {
            var autorizado = await contexto.Usuarios.IgnoreQueryFilters().AnyAsync(usuario =>
                usuario.TenantId == contextoTenant.TenantId
                && usuario.Auth0Sub == usuarioAtual.Identificador
                && usuario.Status == StatusUsuario.Ativo
                && usuario.Perfil == PerfilUsuario.Admin);
            if (autorizado)
                context.Succeed(requirement);
        }
        catch (UnauthorizedAccessException)
        {
            // Sem sub ou tenant validados, a politica nao concede acesso.
        }
    }
}
