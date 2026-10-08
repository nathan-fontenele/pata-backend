using System.Security.Claims;
using Pata.Application.Comum.Abstracoes;

namespace Pata.Api.Servicos;

internal sealed class ContextoTenantHttp(IHttpContextAccessor accessor) : IContextoTenantDefinivel, IUsuarioAtual
{
    private Guid? _tenantDefinido;
    private ClaimsPrincipal Usuario => accessor.HttpContext?.User
        ?? throw new UnauthorizedAccessException("Requisicao sem contexto autenticado.");

    public Guid TenantId
    {
        get
        {
            if (_tenantDefinido is Guid tenantDefinido)
                return tenantDefinido;

            var valor = Usuario.FindFirst("tenant_id")?.Value;
            return Guid.TryParse(valor, out var tenantId) && tenantId != Guid.Empty
                ? tenantId
                : throw new UnauthorizedAccessException("Token sem claim tenant_id valida.");
        }
    }

    public void DefinirTenant(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("tenant_id nao pode ser vazio.", nameof(tenantId));
        if (_tenantDefinido.HasValue && _tenantDefinido.Value != tenantId)
            throw new InvalidOperationException("O tenant ja foi definido nesta requisicao.");
        _tenantDefinido = tenantId;
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
