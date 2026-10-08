using Pata.Application.Funcionalidades.Animal.Cadastro;
using Pata.Domain.Entidades.Tutor;
using Pata.Domain.ObjetosValor;
using TutorAgregado = Pata.Domain.Entidades.Tutor.Tutor;

namespace Pata.Application.Tests.Apoio;

internal sealed class RepositorioCadastroAnimalFalso : IRepositorioCadastroAnimal
{
    public TutorAgregado? Tutor { get; set; }
    public AcessoPortalTutor? Acesso { get; set; }
    public int QuantidadeSalvamentos { get; private set; }
    public bool NovoTutorSalvo { get; private set; }
    public bool NovoAcessoSalvo { get; private set; }

    public Task<TutorAgregado?> ObterTutorPorCpfAsync(Cpf cpf, CancellationToken cancellationToken) =>
        Task.FromResult(Tutor);

    public Task<AcessoPortalTutor?> ObterAcessoTutorAsync(Guid tutorId, CancellationToken cancellationToken) =>
        Task.FromResult(Acesso);

    public Task SalvarAcessoAsync(AcessoPortalTutor acesso, bool novoAcesso, CancellationToken cancellationToken)
    {
        Acesso = acesso;
        NovoAcessoSalvo = novoAcesso;
        return Task.CompletedTask;
    }

    public Task SalvarCadastroAsync(TutorAgregado tutor, bool novoTutor, AcessoPortalTutor acesso,
        bool novoAcesso, CancellationToken cancellationToken)
    {
        Tutor = tutor;
        Acesso = acesso;
        NovoTutorSalvo = novoTutor;
        NovoAcessoSalvo = novoAcesso;
        QuantidadeSalvamentos++;
        return Task.CompletedTask;
    }
}

internal sealed class ContextoTenantFalso(Guid tenantId) : Pata.Application.Comum.Abstracoes.IContextoTenant
{
    public Guid TenantId { get; } = tenantId;
}
