using Pata.Application.Comum.Mensagens;
using Pata.Application.Comum.Abstracoes;
using Pata.Domain.Excecoes;
using Pata.Domain.ObjetosValor;
using Pata.Domain.Repositorios;
using TutorAgregado = Pata.Domain.Entidades.Tutor.Tutor;

namespace Pata.Application.Funcionalidades.Tutor.Comandos.CriarTutor;

public sealed class CriarTutorManipulador(IRepositorioTutor repositorioTutor, IContextoTenant? contextoTenant = null)
    : IManipuladorComando<CriarTutorComando, Guid>
{
    public async Task<Guid> Handle(
        CriarTutorComando comando,
        CancellationToken tokenCancelamento)
    {
        var cpf = new Cpf(comando.Cpf);

        if (await repositorioTutor.ExisteCpfIncluindoExcluidosAsync(cpf, tokenCancelamento))
            throw new ConflitoException("Ja existe um tutor com o CPF informado.");

        var email = new Email(comando.Email);
        var telefone = new Telefone(comando.Telefone);
        var tutor = contextoTenant is null
            ? new TutorAgregado(comando.Nome, cpf, email, telefone)
            : new TutorAgregado(contextoTenant.TenantId, comando.Nome, cpf, email, telefone);

        await repositorioTutor.AdicionarAsync(tutor, tokenCancelamento);

        return tutor.Id;
    }
}
