using Pata.Application.Tests.Apoio;
using Pata.Application.Funcionalidades.Animal.Comandos.CadastrarPet;
using Pata.Domain.Entidades.Animal.Enum;
using Pata.Domain.Entidades.Tutor;
using Pata.Domain.ObjetosValor;
using TutorAgregado = Pata.Domain.Entidades.Tutor.Tutor;

namespace Pata.Application.Tests.Funcionalidades.Animal;

public sealed class CadastrarPetTests
{
    private static readonly DateTime Agora = new(2026, 10, 7, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Guid TenantId = Guid.NewGuid();

    [Fact]
    public async Task DeveCriarTutorPetELinkQuandoTutorAindaNaoExiste()
    {
        var repositorio = new RepositorioCadastroAnimalFalso();
        var manipulador = CriarManipulador(repositorio);

        var resultado = await manipulador.Handle(CriarComando(), CancellationToken.None);

        Assert.True(resultado.NovoTutor);
        Assert.NotEqual(Guid.Empty, resultado.TutorId);
        Assert.NotEqual(Guid.Empty, resultado.AnimalId);
        Assert.NotNull(resultado.TokenAcesso);
        Assert.Equal(TenantId, repositorio.Tutor!.TenantId);
        Assert.Equal(TenantId, repositorio.Acesso!.TenantId);
        Assert.Equal(repositorio.Tutor.Id, repositorio.Acesso.TutorId);
        Assert.NotEqual(resultado.TokenAcesso, repositorio.Acesso.TokenHash);
        Assert.Equal(64, repositorio.Acesso.TokenHash.Length);
        Assert.Single(repositorio.Tutor.Animais);
        Assert.True(repositorio.NovoTutorSalvo);
        Assert.True(repositorio.NovoAcessoSalvo);
    }

    [Fact]
    public async Task DeveReutilizarTutorELinkParaPetsAdicionaisNaMesmaClinica()
    {
        var tutor = CriarTutor();
        var acesso = new AcessoPortalTutor(TenantId, tutor.Id, new string('A', 64), new DateTimeOffset(Agora, TimeSpan.Zero));
        var repositorio = new RepositorioCadastroAnimalFalso { Tutor = tutor, Acesso = acesso };
        var manipulador = CriarManipulador(repositorio);

        var primeiroResultado = await manipulador.Handle(CriarComando("Luna"), CancellationToken.None);
        var segundoResultado = await manipulador.Handle(CriarComando("Toby"), CancellationToken.None);

        Assert.False(primeiroResultado.NovoTutor);
        Assert.False(segundoResultado.NovoTutor);
        Assert.Null(primeiroResultado.TokenAcesso);
        Assert.Null(segundoResultado.TokenAcesso);
        Assert.Equal(primeiroResultado.TutorId, segundoResultado.TutorId);
        Assert.Equal(2, tutor.Animais.Count);
        Assert.Same(acesso, repositorio.Acesso);
        Assert.Equal(2, repositorio.QuantidadeSalvamentos);
    }

    [Fact]
    public async Task DeveGerarNovoLinkSeOAnteriorFoiRevogado()
    {
        var tutor = CriarTutor();
        var acesso = new AcessoPortalTutor(TenantId, tutor.Id, new string('A', 64), new DateTimeOffset(Agora, TimeSpan.Zero));
        acesso.Revogar(new DateTimeOffset(Agora.AddHours(1), TimeSpan.Zero));
        var repositorio = new RepositorioCadastroAnimalFalso { Tutor = tutor, Acesso = acesso };
        var manipulador = CriarManipulador(repositorio);

        var resultado = await manipulador.Handle(CriarComando(), CancellationToken.None);

        Assert.NotNull(resultado.TokenAcesso);
        Assert.NotEqual(new string('A', 64), repositorio.Acesso!.TokenHash);
        Assert.Null(repositorio.Acesso.RevogadoEm);
        Assert.False(repositorio.NovoAcessoSalvo);
    }

    private static CadastrarPetManipulador CriarManipulador(RepositorioCadastroAnimalFalso repositorio) =>
        new(repositorio, new ContextoTenantFalso(TenantId), new RelogioFalso(Agora));

    private static CadastrarPetComando CriarComando(string petNome = "Mel") => new(
        "Maria Silva", "52998224725", "maria@exemplo.com", "11912345678",
        petNome, Especie.Cachorro, "Vira-lata", new DateOnly(2022, 1, 1));

    private static TutorAgregado CriarTutor() => new(TenantId, "Maria Silva", new Cpf("52998224725"),
        new Email("maria@exemplo.com"), new Telefone("11912345678"));
}
