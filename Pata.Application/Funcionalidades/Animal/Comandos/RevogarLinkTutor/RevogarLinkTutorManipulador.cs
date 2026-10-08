using Pata.Application.Comum.Mensagens;
using Pata.Application.Funcionalidades.Animal.Cadastro;
using Pata.Domain.Excecoes;

namespace Pata.Application.Funcionalidades.Animal.Comandos.RevogarLinkTutor;

public sealed class RevogarLinkTutorManipulador(
    IRepositorioCadastroAnimal repositorio,
    Pata.Application.Comum.Abstracoes.IRelogio relogio)
    : IManipuladorComando<RevogarLinkTutorComando>
{
    public async Task Handle(RevogarLinkTutorComando comando, CancellationToken cancellationToken)
    {
        var acesso = await repositorio.ObterAcessoTutorAsync(comando.TutorId, cancellationToken)
            ?? throw new RecursoNaoEncontradoException("Link de acompanhamento nao encontrado.");
        acesso.Revogar(new DateTimeOffset(relogio.UtcAgora, TimeSpan.Zero));
        await repositorio.SalvarAcessoAsync(acesso, novoAcesso: false, cancellationToken);
    }
}
