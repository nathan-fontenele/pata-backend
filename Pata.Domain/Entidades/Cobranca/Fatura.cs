using Pata.Domain.Comum;
using Pata.Domain.Excecoes;

namespace Pata.Domain.Entidades.Cobranca;

public enum StatusFatura { Rascunho, Aberta, Paga, Anulada, Incobravel }

public sealed class Fatura : EntidadeTenant<Guid>
{
    private readonly List<ItemFatura> _itens = [];
    private Fatura() { }

    public Fatura(Guid tenantId, Guid assinaturaId, string numero, string moeda,
        DateTimeOffset emitidaEm, DateTimeOffset vencimentoEm) : base(Guid.NewGuid(), tenantId)
    {
        if (assinaturaId == Guid.Empty) throw new ErroDeValidacao("Assinatura e obrigatoria.", nameof(assinaturaId));
        if (vencimentoEm < emitidaEm) throw new ErroDeValidacao("Vencimento nao pode anteceder emissao.", nameof(vencimentoEm));
        AssinaturaId = assinaturaId;
        Numero = Plano.Obrigatorio(numero, nameof(numero), 80);
        Moeda = PrecoPlano.NormalizarMoeda(moeda);
        EmitidaEm = emitidaEm;
        VencimentoEm = vencimentoEm;
        Status = StatusFatura.Rascunho;
    }

    public Guid AssinaturaId { get; private set; }
    public string Numero { get; private set; } = null!;
    public StatusFatura Status { get; private set; }
    public string Moeda { get; private set; } = null!;
    public decimal Subtotal { get; private set; }
    public decimal DescontoTotal { get; private set; }
    public decimal Total { get; private set; }
    public DateTimeOffset EmitidaEm { get; private set; }
    public DateTimeOffset VencimentoEm { get; private set; }
    public DateTimeOffset? QuitadaEm { get; private set; }
    public IReadOnlyCollection<ItemFatura> Itens => _itens.AsReadOnly();

    public ItemFatura AdicionarItem(Guid? precoPlanoId, string descricao, decimal quantidade,
        decimal valorUnitario, decimal desconto, DateTimeOffset? periodoInicio, DateTimeOffset? periodoFim)
    {
        if (Status != StatusFatura.Rascunho) throw new RegraDeNegocioException("Somente faturas em rascunho aceitam itens.");
        var item = new ItemFatura(TenantId, Id, precoPlanoId, descricao, quantidade, valorUnitario,
            desconto, periodoInicio, periodoFim);
        _itens.Add(item);
        Recalcular();
        return item;
    }

    private void Recalcular()
    {
        Subtotal = _itens.Sum(item => item.Quantidade * item.ValorUnitario);
        DescontoTotal = _itens.Sum(item => item.Desconto);
        Total = Subtotal - DescontoTotal;
    }
}

public sealed class ItemFatura : EntidadeTenant<Guid>
{
    private ItemFatura() { }

    internal ItemFatura(Guid tenantId, Guid faturaId, Guid? precoPlanoId, string descricao,
        decimal quantidade, decimal valorUnitario, decimal desconto,
        DateTimeOffset? periodoInicio, DateTimeOffset? periodoFim) : base(Guid.NewGuid(), tenantId)
    {
        if (tenantId == Guid.Empty || faturaId == Guid.Empty) throw new ErroDeValidacao("Tenant e fatura sao obrigatorios.");
        if (quantidade <= 0) throw new ErroDeValidacao("Quantidade deve ser positiva.", nameof(quantidade));
        if (valorUnitario < 0 || desconto < 0 || desconto > quantidade * valorUnitario)
            throw new ErroDeValidacao("Valores do item da fatura invalidos.");
        if (periodoInicio.HasValue != periodoFim.HasValue || (periodoInicio.HasValue && periodoFim <= periodoInicio))
            throw new ErroDeValidacao("Periodo do item da fatura invalido.");
        FaturaId = faturaId;
        PrecoPlanoId = precoPlanoId;
        Descricao = Plano.Obrigatorio(descricao, nameof(descricao), 500);
        Quantidade = quantidade;
        ValorUnitario = valorUnitario;
        Desconto = desconto;
        Total = quantidade * valorUnitario - desconto;
        PeriodoInicio = periodoInicio;
        PeriodoFim = periodoFim;
    }

    public Guid FaturaId { get; private set; }
    public Guid? PrecoPlanoId { get; private set; }
    public string Descricao { get; private set; } = null!;
    public decimal Quantidade { get; private set; }
    public decimal ValorUnitario { get; private set; }
    public decimal Desconto { get; private set; }
    public decimal Total { get; private set; }
    public DateTimeOffset? PeriodoInicio { get; private set; }
    public DateTimeOffset? PeriodoFim { get; private set; }
}
