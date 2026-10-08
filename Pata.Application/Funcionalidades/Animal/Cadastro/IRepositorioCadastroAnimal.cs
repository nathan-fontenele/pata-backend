using Pata.Domain.Entidades.Tutor;
using Pata.Domain.ObjetosValor;
using TutorAgregado = Pata.Domain.Entidades.Tutor.Tutor;

namespace Pata.Application.Funcionalidades.Animal.Cadastro;

public interface IRepositorioCadastroAnimal
{
    Task<TutorAgregado?> ObterTutorPorCpfAsync(Cpf cpf, CancellationToken cancellationToken);
    Task<AcessoPortalTutor?> ObterAcessoTutorAsync(Guid tutorId, CancellationToken cancellationToken);
    Task SalvarAcessoAsync(AcessoPortalTutor acesso, bool novoAcesso, CancellationToken cancellationToken);
    Task SalvarCadastroAsync(TutorAgregado tutor, bool novoTutor, AcessoPortalTutor acesso, bool novoAcesso,
        CancellationToken cancellationToken);
}
