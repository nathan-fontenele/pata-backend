namespace Pata.Domain.Entidades.Organizacao;

public sealed record AcessoOrganizacao(Guid UsuarioId, Guid TenantId, string Nome, string Slug,
    PerfilUsuario Perfil, StatusUsuario Status);
