using Pata.Domain.Comum;
using Pata.Domain.Excecoes;

namespace Pata.Domain.Entidades.Organizacao;

public enum StatusUsuario { Ativo, Bloqueado, Desativado, Pendente }

public sealed class Usuario : EntidadeTenant<Guid>
{
    private Usuario() { }

    public Usuario(Guid tenantId, Guid equipeId, string nome, string sobrenome, string email,
        string? telefone, string? cargo, StatusUsuario status = StatusUsuario.Pendente)
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
    }

    public Guid EquipeId { get; private set; }
    public string Nome { get; private set; } = null!;
    public string Sobrenome { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string? Telefone { get; private set; }
    public string? Cargo { get; private set; }
    public DateTimeOffset? UltimoAcessoEm { get; private set; }
    public StatusUsuario Status { get; private set; }

    private static string Obrigatorio(string valor, string campo, int limite) =>
        string.IsNullOrWhiteSpace(valor) || valor.Trim().Length > limite
            ? throw new ErroDeValidacao($"{campo} e obrigatorio e deve ter ate {limite} caracteres.", campo)
            : valor.Trim();
}
