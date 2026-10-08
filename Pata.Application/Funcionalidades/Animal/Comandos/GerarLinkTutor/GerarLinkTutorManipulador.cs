using Pata.Application.Comum.Mensagens;
using Pata.Application.Funcionalidades.Animal.Cadastro;
using Pata.Application.Funcionalidades.Animal.Comandos.CadastrarPet;
using Pata.Domain.Entidades.Tutor;
using Pata.Domain.Excecoes;
using Pata.Domain.Repositorios;

namespace Pata.Application.Funcionalidades.Animal.Comandos.GerarLinkTutor;

public sealed class GerarLinkTutorManipulador(
    IRepositorioTutor repositorioTutor,
    IRepositorioCadastroAnimal repositorioCadastro,
    Pata.Application.Comum.Abstracoes.IRelogio relogio)
    : IManipuladorComando<GerarLinkTutorComando, string>
{
    public async Task<string> Handle(GerarLinkTutorComando comando, CancellationToken cancellationToken)
    {
        var tutor = await repositorioTutor.ObterPorIdAsync(comando.TutorId, cancellationToken)
            ?? throw new RecursoNaoEncontradoException("Tutor nao encontrado.");
        var acesso = await repositorioCadastro.ObterAcessoTutorAsync(tutor.Id, cancellationToken);
        var token = CadastrarPetManipulador.GerarToken();
        var agora = new DateTimeOffset(relogio.UtcAgora, TimeSpan.Zero);
        var novoAcesso = acesso is null;
        if (acesso is null)
            acesso = new AcessoPortalTutor(tutor.TenantId, tutor.Id,
                CadastrarPetManipulador.CalcularHash(token), agora);
        else
            acesso.RotacionarToken(CadastrarPetManipulador.CalcularHash(token), agora);

        await repositorioCadastro.SalvarAcessoAsync(acesso, novoAcesso, cancellationToken);
        return token;
    }
}
