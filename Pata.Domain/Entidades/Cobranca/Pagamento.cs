using Pata.Domain.Comum;
using Pata.Domain.Excecoes;

namespace Pata.Domain.Entidades.Cobranca;

public enum StatusPagamento { Pendente, Processando, Confirmado, Falhou, Cancelado, Expirado }

public sealed class Pagamento : EntidadeTenant<Guid>
{
    private Pagamento() { }

    public Pagamento(Guid tenantId, Guid faturaId, decimal valor, string moeda,
        string metodo, string provedor, string chaveIdempotencia) : base(Guid.NewGuid(), tenantId)
    {
        if (faturaId == Guid.Empty) throw new ErroDeValidacao("Fatura e obrigatoria.", nameof(faturaId));
        if (valor <= 0) throw new ErroDeValidacao("Valor deve ser positivo.", nameof(valor));
        FaturaId = faturaId;
        Valor = valor;
        Moeda = PrecoPlano.NormalizarMoeda(moeda);
        Metodo = Plano.Obrigatorio(metodo, nameof(metodo), 50);
        Provedor = Plano.Obrigatorio(provedor, nameof(provedor), 80);
        ChaveIdempotencia = Plano.Obrigatorio(chaveIdempotencia, nameof(chaveIdempotencia), 120);
        Status = StatusPagamento.Pendente;
    }

    public Guid FaturaId { get; private set; }
    public StatusPagamento Status { get; private set; }
    public decimal Valor { get; private set; }
    public string Moeda { get; private set; } = null!;
    public string Metodo { get; private set; } = null!;
    public string Provedor { get; private set; } = null!;
    public string? ReferenciaExterna { get; private set; }
    public string ChaveIdempotencia { get; private set; } = null!;
    public DateTimeOffset? ConfirmadaEm { get; private set; }
    public DateTimeOffset? FalhouEm { get; private set; }
    public string? CodigoFalha { get; private set; }
    public string? MensagemFalha { get; private set; }
}
