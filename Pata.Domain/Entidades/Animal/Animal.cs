using Pata.Domain.Comum;
using Pata.Domain.Entidades.Animal.Enum;
using Pata.Domain.Excecoes;

namespace Pata.Domain.Entidades.Animal;

public sealed class Animal : Entidade<Guid>, ITenantEntity
{
    public const int TamanhoMaximoNome = 100;
    public const int TamanhoMaximoRaca = 100;

    private Animal()
    {
    }

    internal Animal(
        Guid id,
        Guid tutorId,
        string nome,
        Especie especie,
        string raca,
        DateOnly dataNascimento,
        DateOnly dataAtual) : base(id)
    {
        if (tutorId == Guid.Empty)
            throw new ErroDeValidacao("Tutor e obrigatorio.", nameof(tutorId));

        TutorId = tutorId;
        AplicarDados(nome, especie, raca, dataNascimento, dataAtual);
    }

    internal Animal(
        Guid id,
        Guid tutorId,
        Guid tenantId,
        string nome,
        Especie especie,
        string raca,
        DateOnly dataNascimento,
        DateOnly dataAtual) : base(id)
    {
        if (tutorId == Guid.Empty)
            throw new ErroDeValidacao("Tutor e obrigatorio.", nameof(tutorId));
        if (tenantId == Guid.Empty)
            throw new ErroDeValidacao("Tenant e obrigatorio.", nameof(tenantId));

        TutorId = tutorId;
        TenantId = tenantId;
        AplicarDados(nome, especie, raca, dataNascimento, dataAtual);
    }

    public Guid TutorId { get; private set; }
    public Guid TenantId { get; private set; }
    public string Nome { get; private set; } = null!;
    public Especie Especie { get; private set; }
    public string Raca { get; private set; } = null!;
    public DateOnly DataNascimento { get; private set; }

    public int CalcularIdade(DateOnly dataReferencia)
    {
        if (dataReferencia < DataNascimento)
            throw new ErroDeValidacao(
                "A data de referencia nao pode ser anterior ao nascimento.",
                nameof(dataReferencia));

        var idade = dataReferencia.Year - DataNascimento.Year;

        if (DataNascimento > dataReferencia.AddYears(-idade))
            idade--;

        return idade;
    }

    internal void AlterarDados(
        string nome,
        Especie especie,
        string raca,
        DateOnly dataNascimento,
        DateOnly dataAtual) => AplicarDados(nome, especie, raca, dataNascimento, dataAtual);

    private void AplicarDados(
        string nome,
        Especie especie,
        string raca,
        DateOnly dataNascimento,
        DateOnly dataAtual)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ErroDeValidacao("Nome e obrigatorio.", nameof(nome));

        var nomeNormalizado = nome.Trim();

        if (nomeNormalizado.Length > TamanhoMaximoNome)
            throw new ErroDeValidacao(
                $"O nome deve ter no maximo {TamanhoMaximoNome} caracteres.",
                nameof(nome));

        if (!System.Enum.IsDefined(especie))
            throw new ErroDeValidacao("Especie invalida.", nameof(especie));

        if (string.IsNullOrWhiteSpace(raca))
            throw new ErroDeValidacao("Raca e obrigatoria.", nameof(raca));

        var racaNormalizada = raca.Trim();

        if (racaNormalizada.Length > TamanhoMaximoRaca)
            throw new ErroDeValidacao(
                $"A raca deve ter no maximo {TamanhoMaximoRaca} caracteres.",
                nameof(raca));

        if (dataNascimento > dataAtual)
            throw new ErroDeValidacao(
                "A data de nascimento nao pode estar no futuro.",
                nameof(dataNascimento));

        Nome = nomeNormalizado;
        Especie = especie;
        Raca = racaNormalizada;
        DataNascimento = dataNascimento;
    }
}
