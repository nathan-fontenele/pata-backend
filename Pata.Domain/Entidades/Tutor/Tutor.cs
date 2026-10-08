using Pata.Domain.Comum;
using Pata.Domain.Entidades.Animal.Enum;
using Pata.Domain.Excecoes;
using Pata.Domain.ObjetosValor;

namespace Pata.Domain.Entidades.Tutor;

public class Tutor : RaizAgregadaAuditavel<Guid>
{
    private readonly List<Animal.Animal> _animais = [];

    public string Nome { get; private set; }
    public Cpf Cpf { get; private set; }
    public Email Email { get; private set; }
    public Telefone Telefone { get; private set; }
    public IReadOnlyCollection<Animal.Animal> Animais => _animais.AsReadOnly();

    private Tutor()
    {
        Nome = null!;
        Cpf = null!;
        Email = null!;
        Telefone = null!;
    }

    public Tutor(string nome, Cpf cpf, Email email, Telefone telefone) : base(Guid.NewGuid())
    {
        if (cpf is null)
            throw new ErroDeValidacao("CPF e obrigatorio.", nameof(cpf));
        if (email is null)
            throw new ErroDeValidacao("E-mail e obrigatorio.", nameof(email));
        if (telefone is null)
            throw new ErroDeValidacao("Telefone e obrigatorio.", nameof(telefone));

        Nome = ValidarNome(nome);
        Cpf = cpf;
        Email = email;
        Telefone = telefone;
    }

    public Tutor(Guid tenantId, string nome, Cpf cpf, Email email, Telefone telefone)
        : base(Guid.NewGuid(), tenantId)
    {
        if (cpf is null)
            throw new ErroDeValidacao("CPF e obrigatorio.", nameof(cpf));
        if (email is null)
            throw new ErroDeValidacao("E-mail e obrigatorio.", nameof(email));
        if (telefone is null)
            throw new ErroDeValidacao("Telefone e obrigatorio.", nameof(telefone));

        Nome = ValidarNome(nome);
        Cpf = cpf;
        Email = email;
        Telefone = telefone;
    }

    public void AlterarNome(string nome) => Nome = ValidarNome(nome);

    public void AlterarEmail(Email email)
    {
        if (email is null)
            throw new ErroDeValidacao("E-mail e obrigatorio.", nameof(email));
        Email = email;
    }

    public void AlterarTelefone(Telefone telefone)
    {
        if (telefone is null)
            throw new ErroDeValidacao("Telefone e obrigatorio.", nameof(telefone));
        Telefone = telefone;
    }

    public Animal.Animal AdicionarAnimal(
        string nome,
        Especie especie,
        string raca,
        DateOnly dataNascimento,
        DateOnly dataAtual)
    {
        var animal = new Animal.Animal(
            Guid.NewGuid(),
            Id,
            TenantId,
            nome,
            especie,
            raca,
            dataNascimento,
            dataAtual);
        _animais.Add(animal);
        return animal;
    }

    public void AlterarAnimal(
        Guid animalId,
        string nome,
        Especie especie,
        string raca,
        DateOnly dataNascimento,
        DateOnly dataAtual) =>
        ObterAnimal(animalId).AlterarDados(nome, especie, raca, dataNascimento, dataAtual);

    public void RemoverAnimal(Guid animalId)
    {
        var animal = ObterAnimal(animalId);
        _animais.Remove(animal);
    }

    private static string ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ErroDeValidacao("Nome e obrigatorio.", nameof(nome));
        return nome.Trim();
    }

    private Animal.Animal ObterAnimal(Guid animalId)
    {
        if (animalId == Guid.Empty)
            throw new ErroDeValidacao("Animal e obrigatorio.", nameof(animalId));

        return _animais.FirstOrDefault(animal => animal.Id == animalId)
               ?? throw new RecursoNaoEncontradoException("Animal nao encontrado neste tutor.");
    }
}
