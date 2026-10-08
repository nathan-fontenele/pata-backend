using Pata.Domain.Comum;
using Pata.Domain.Excecoes;

namespace Pata.Domain.Entidades.Organizacao;

public sealed class Organizacao : EntidadeTenant<Guid>
{
    private Organizacao() { }

    public Organizacao(string nome, string slug, string cnpj, string email, string? logoUrl = null)
        : this(Guid.NewGuid(), nome, slug, cnpj, email, logoUrl) { }

    private Organizacao(Guid id, string nome, string slug, string cnpj, string email, string? logoUrl)
        : base(id, id)
    {
        Nome = Obrigatorio(nome, nameof(nome), 150);
        Slug = NormalizarSlug(slug);
        Cnpj = NormalizarCnpj(cnpj);
        Email = Obrigatorio(email, nameof(email), 254);
        LogoUrl = string.IsNullOrWhiteSpace(logoUrl) ? null : logoUrl.Trim();
        Status = StatusOrganizacao.Ativa;
    }

    public string Nome { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public StatusOrganizacao Status { get; private set; }
    public string Cnpj { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string? LogoUrl { get; private set; }

    private static string Obrigatorio(string valor, string campo, int max)
    {
        if (string.IsNullOrWhiteSpace(valor) || valor.Trim().Length > max)
            throw new ErroDeValidacao($"{campo} e obrigatorio e deve ter ate {max} caracteres.", campo);
        return valor.Trim();
    }

    private static string NormalizarSlug(string slug)
    {
        var normalizado = Obrigatorio(slug, nameof(slug), 80).ToLowerInvariant();
        if (normalizado.Any(c => !char.IsAsciiLetterOrDigit(c) && c is not '-' and not '_'))
            throw new ErroDeValidacao("Slug contem caracteres invalidos.", nameof(slug));
        return normalizado;
    }

    private static string NormalizarCnpj(string cnpj)
    {
        var digitos = string.Concat((cnpj ?? string.Empty).Where(char.IsDigit));
        if (digitos.Length != 14)
            throw new ErroDeValidacao("CNPJ deve conter 14 digitos.", nameof(cnpj));
        return digitos;
    }
}
