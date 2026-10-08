using System.Security.Claims;
using Pata.Application.Comum.Abstracoes;

namespace Pata.Api.Servicos;

internal sealed class ContextoTenantHttp(IHttpContextAccessor accessor) : IContextoTenant, IUsuarioAtual
{
    private ClaimsPrincipal Usuario => accessor.HttpContext?.User
        ?? throw new UnauthorizedAccessException("Requisicao sem contexto autenticado.");

    public Guid TenantId
    {
        get
        {
            var valor = Usuario.FindFirst("tenant_id")?.Value;
            return Guid.TryParse(valor, out var tenantId) && tenantId != Guid.Empty
                ? tenantId
                : throw new UnauthorizedAccessException("Token sem claim tenant_id valida.");
        }
    }

    public string Identificador =>
        Usuario.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? Usuario.FindFirst("sub")?.Value
        ?? throw new UnauthorizedAccessException("Token sem identificador de usuario.");

    public string Perfil => Usuario.FindFirst("perfil")?.Value
        ?? Usuario.FindFirst("role")?.Value
        ?? Usuario.FindFirst(ClaimTypes.Role)?.Value
        ?? "usuario";

    public IReadOnlyCollection<string> Papeis => Usuario.FindAll("role")
        .Concat(Usuario.FindAll(ClaimTypes.Role))
        .Select(claim => claim.Value).ToArray();
}
