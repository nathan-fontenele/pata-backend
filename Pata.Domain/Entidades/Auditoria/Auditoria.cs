using Pata.Domain.Comum;
using Pata.Domain.Excecoes;

namespace Pata.Domain.Entidades.Auditoria;

public enum AcaoAuditoria { Criado, Editado, Excluido }

public sealed class Auditoria : EntidadeTenant<Guid>
{
    private Auditoria() { }

    public Auditoria(Guid tenantId, Guid entidadeId, string entidadeTipo, AcaoAuditoria acao,
        DateTimeOffset criadoEm, string criadoPor, string? entidadeAntes, string? entidadeDepois,
        DateTimeOffset? editadoEm = null, string? editadoPor = null,
        DateTimeOffset? excluidoEm = null, string? excluidoPor = null)
        : base(Guid.NewGuid(), tenantId)
    {
        if (tenantId == Guid.Empty || entidadeId == Guid.Empty) throw new ErroDeValidacao("Tenant e entidade sao obrigatorios.");
        EntidadeId = entidadeId;
        EntidadeTipo = Obrigatorio(entidadeTipo, nameof(entidadeTipo), 150);
        Acao = acao;
        CriadoEm = criadoEm;
        CriadoPor = Obrigatorio(criadoPor, nameof(criadoPor), 150);
        EntidadeAntes = entidadeAntes;
        EntidadeDepois = entidadeDepois;
        EditadoEm = editadoEm;
        EditadoPor = editadoPor;
        ExcluidoEm = excluidoEm;
        ExcluidoPor = excluidoPor;
    }

    public Guid EntidadeId { get; private set; }
    public string EntidadeTipo { get; private set; } = null!;
    public AcaoAuditoria Acao { get; private set; }
    public DateTimeOffset CriadoEm { get; private set; }
    public string CriadoPor { get; private set; } = null!;
    public DateTimeOffset? EditadoEm { get; private set; }
    public string? EditadoPor { get; private set; }
    public DateTimeOffset? ExcluidoEm { get; private set; }
    public string? ExcluidoPor { get; private set; }
    public string? EntidadeAntes { get; private set; }
    public string? EntidadeDepois { get; private set; }

    private static string Obrigatorio(string valor, string nome, int limite) =>
        string.IsNullOrWhiteSpace(valor) || valor.Trim().Length > limite
            ? throw new ErroDeValidacao($"{nome} e obrigatorio.", nome)
            : valor.Trim();
}
