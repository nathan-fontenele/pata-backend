using Pata.Domain.Comum;
using Pata.Domain.Excecoes;

namespace Pata.Domain.Entidades.Cobranca;

public enum StatusPlano { Rascunho, Ativo, Arquivado }
public enum StatusPrecoPlano { Rascunho, Ativo, Inativo }
public enum IntervaloUnidade { Dia, Semana, Mes, Ano }

public sealed class Plano : Entidade<Guid>
{
    private Plano() { }

    public Plano(string codigo, string nome, string? descricao = null) : base(Guid.NewGuid())
    {
        Codigo = Obrigatorio(codigo, nameof(codigo), 50);
        Nome = Obrigatorio(nome, nameof(nome), 120);
        Descricao = string.IsNullOrWhiteSpace(descricao) ? null : descricao.Trim();
        Status = StatusPlano.Rascunho;
    }

    public string Codigo { get; private set; } = null!;
    public string Nome { get; private set; } = null!;
    public string? Descricao { get; private set; }
    public StatusPlano Status { get; private set; }

    internal static string Obrigatorio(string valor, string campo, int limite) =>
        string.IsNullOrWhiteSpace(valor) || valor.Trim().Length > limite
            ? throw new ErroDeValidacao($"{campo} e obrigatorio e deve ter ate {limite} caracteres.", campo)
            : valor.Trim();
}

public sealed class PrecoPlano : Entidade<Guid>
{
    private PrecoPlano() { }

    public PrecoPlano(Guid planoId, string codigo, decimal valor, string moeda, int diasTeste,
        IntervaloUnidade intervaloUnidade, int intervaloQuantidade) : base(Guid.NewGuid())
    {
        if (planoId == Guid.Empty) throw new ErroDeValidacao("Plano e obrigatorio.", nameof(planoId));
        if (valor < 0) throw new ErroDeValidacao("Valor nao pode ser negativo.", nameof(valor));
        if (diasTeste < 0) throw new ErroDeValidacao("Dias de teste nao pode ser negativo.", nameof(diasTeste));
        if (intervaloQuantidade <= 0) throw new ErroDeValidacao("Intervalo deve ser positivo.", nameof(intervaloQuantidade));
        PlanoId = planoId;
        Codigo = Plano.Obrigatorio(codigo, nameof(codigo), 50);
        Valor = valor;
        Moeda = NormalizarMoeda(moeda);
        DiasTeste = diasTeste;
        IntervaloUnidade = intervaloUnidade;
        IntervaloQuantidade = intervaloQuantidade;
        Status = StatusPrecoPlano.Rascunho;
    }

    public Guid PlanoId { get; private set; }
    public string Codigo { get; private set; } = null!;
    public decimal Valor { get; private set; }
    public string Moeda { get; private set; } = null!;
    public int DiasTeste { get; private set; }
    public IntervaloUnidade IntervaloUnidade { get; private set; }
    public int IntervaloQuantidade { get; private set; }
    public StatusPrecoPlano Status { get; private set; }

    internal static string NormalizarMoeda(string moeda)
    {
        var valor = (moeda ?? string.Empty).Trim().ToUpperInvariant();
        return valor.Length == 3 && valor.All(char.IsAsciiLetter)
            ? valor
            : throw new ErroDeValidacao("Moeda deve ser um codigo ISO de tres letras.", nameof(moeda));
    }
}
