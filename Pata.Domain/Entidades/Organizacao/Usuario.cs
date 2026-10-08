using Pata.Domain.Comum;
using Pata.Domain.Excecoes;

namespace Pata.Domain.Entidades.Organizacao;

public enum StatusUsuario { Ativo, Bloqueado, Desativado, Pendente }
public enum PerfilUsuario { Admin, Usuario }

public sealed class Usuario : EntidadeTenant<Guid>
{
    private Usuario() { }

    public Usuario(Guid tenantId, Guid equipeId, string nome, string sobrenome, string email,
        string? telefone, string? cargo, StatusUsuario status = StatusUsuario.Pendente,
        string? auth0Sub = null, PerfilUsuario perfil = PerfilUsuario.Usuario)
        : base(Guid.NewGuid(), tenantId)
    {
        if (equipeId == Guid.Empty) throw new ErroDeValidacao("Equipe e obrigatoria.", nameof(equipeId));
        EquipeId = equipeId;
        Nome = Obrigatorio(nome, nameof(nome), 100);
        Sobrenome = Obrigatorio(sobrenome, nameof(sobrenome), 100);
        Email = Obrigatorio(email, nameof(email), 254).ToLowerInvariant();
        Telefone = string.IsNullOrWhiteSpace(telefone) ? null : telefone.Trim();
        Cargo = string.IsNullOrWhiteSpace(cargo) ? null : cargo.Trim();
        Status = status;
        Auth0Sub = string.IsNullOrWhiteSpace(auth0Sub) ? null : Obrigatorio(auth0Sub, nameof(auth0Sub), 200);
        Perfil = perfil;
    }

    public Guid EquipeId { get; private set; }
    public string Nome { get; private set; } = null!;
    public string Sobrenome { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string? Telefone { get; private set; }
    public string? Cargo { get; private set; }
    public DateTimeOffset? UltimoAcessoEm { get; private set; }
    public StatusUsuario Status { get; private set; }
    public string? Auth0Sub { get; private set; }
    public PerfilUsuario Perfil { get; private set; }

    public void VincularIdentidadeAuth0(string auth0Sub, DateTimeOffset acessoEm)
    {
        if (Status != StatusUsuario.Pendente || !string.IsNullOrWhiteSpace(Auth0Sub))
            throw new RegraDeNegocioException("O usuario nao esta pendente de vinculo de identidade.");
        Auth0Sub = Obrigatorio(auth0Sub, nameof(auth0Sub), 200);
        Status = StatusUsuario.Ativo;
        UltimoAcessoEm = acessoEm;
    }

    private static string Obrigatorio(string valor, string campo, int limite) =>
        string.IsNullOrWhiteSpace(valor) || valor.Trim().Length > limite
            ? throw new ErroDeValidacao($"{campo} e obrigatorio e deve ter ate {limite} caracteres.", campo)
            : valor.Trim();
}
