using Pata.Domain.Comum;
using Pata.Domain.Excecoes;

namespace Pata.Domain.Entidades.Organizacao;

public enum StatusEquipe { Ativa, Arquivada }

public sealed class Equipe : EntidadeTenant<Guid>
{
    private Equipe() { }

    public Equipe(Guid tenantId, string nome, string? descricao = null) : base(Guid.NewGuid(), tenantId)
    {
        Nome = string.IsNullOrWhiteSpace(nome) ? throw new ErroDeValidacao("Nome e obrigatorio.", nameof(nome)) : nome.Trim();
        OrganizacaoId = tenantId;
        Descricao = string.IsNullOrWhiteSpace(descricao) ? null : descricao.Trim();
        Status = StatusEquipe.Ativa;
    }

    public Guid OrganizacaoId { get; private set; }
    public string Nome { get; private set; } = null!;
    public string? Descricao { get; private set; }
    public StatusEquipe Status { get; private set; }
}
