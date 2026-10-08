using Pata.Domain.Comum;
using Pata.Domain.Excecoes;
using Pata.Domain.ObjetosValor;

namespace Pata.Domain.Entidades.Veterinario;

public sealed class Veterinario : RaizAgregadaAuditavel<Guid>
{
    public const int TamanhoMaximoNome = 100;
    public const int TamanhoMaximoEspecialidade = 100;

    public string Nome { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public Telefone Telefone { get; private set; } = null!;
    public Crmv Crmv { get; private set; } = null!;
    public string Especialidade { get; private set; } = null!;

    public Veterinario(
        string nome,
        Email email,
        Telefone telefone,
        Crmv crmv,
        string especialidade) : base(Guid.NewGuid())
    {
        if (email is null)
            throw new ErroDeValidacao("E-mail e obrigatorio.", nameof(email));
        if (telefone is null)
            throw new ErroDeValidacao("Telefone e obrigatorio.", nameof(telefone));
        if (crmv is null)
            throw new ErroDeValidacao("CRMV e obrigatorio.", nameof(crmv));

        Nome = ValidarTexto(nome, TamanhoMaximoNome, nameof(nome));
        Email = email;
        Telefone = telefone;
        Crmv = crmv;
        Especialidade = ValidarTexto(
            especialidade,
            TamanhoMaximoEspecialidade,
            nameof(especialidade));
    }

    public Veterinario(
        Guid tenantId,
        string nome,
        Email email,
        Telefone telefone,
        Crmv crmv,
        string especialidade) : base(Guid.NewGuid(), tenantId)
    {
        if (email is null)
            throw new ErroDeValidacao("E-mail e obrigatorio.", nameof(email));
        if (telefone is null)
            throw new ErroDeValidacao("Telefone e obrigatorio.", nameof(telefone));
        if (crmv is null)
            throw new ErroDeValidacao("CRMV e obrigatorio.", nameof(crmv));

        Nome = ValidarTexto(nome, TamanhoMaximoNome, nameof(nome));
        Email = email;
        Telefone = telefone;
        Crmv = crmv;
        Especialidade = ValidarTexto(especialidade, TamanhoMaximoEspecialidade, nameof(especialidade));
    }

    public void AlterarNome(string nome) =>
        Nome = ValidarTexto(nome, TamanhoMaximoNome, nameof(nome));

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

    public void AlterarEspecialidade(string especialidade) =>
        Especialidade = ValidarTexto(
            especialidade,
            TamanhoMaximoEspecialidade,
            nameof(especialidade));

    private static string ValidarTexto(string valor, int tamanhoMaximo, string nomeParametro)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ErroDeValidacao("O campo e obrigatorio.", nomeParametro);

        var valorNormalizado = valor.Trim();

        if (valorNormalizado.Length > tamanhoMaximo)
            throw new ErroDeValidacao(
                $"O campo deve ter no maximo {tamanhoMaximo} caracteres.",
                nomeParametro);

        return valorNormalizado;
    }
}
