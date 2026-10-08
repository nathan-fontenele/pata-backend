using Pata.Application.Comum.Mensagens;
using Pata.Application.Comum.Abstracoes;
using Pata.Domain.Excecoes;
using Pata.Domain.ObjetosValor;
using Pata.Domain.Repositorios;
using VeterinarioAgregado = Pata.Domain.Entidades.Veterinario.Veterinario;

namespace Pata.Application.Funcionalidades.Veterinario.Comandos.CriarVeterinario;

public sealed class CriarVeterinarioManipulador(
    IRepositorioVeterinario repositorioVeterinario,
    IContextoTenant? contextoTenant = null)
    : IManipuladorComando<CriarVeterinarioComando, Guid>
{
    public async Task<Guid> Handle(
        CriarVeterinarioComando comando,
        CancellationToken tokenCancelamento)
    {
        var crmv = new Crmv(comando.NumeroCrmv, comando.UfCrmv);

        if (await repositorioVeterinario.ExisteCrmvIncluindoExcluidosAsync(
                crmv,
                tokenCancelamento))
            throw new ConflitoException("Ja existe um veterinario com o CRMV informado.");

        var email = new Email(comando.Email);
        var telefone = new Telefone(comando.Telefone);
        var veterinario = contextoTenant is null
            ? new VeterinarioAgregado(comando.Nome, email, telefone, crmv, comando.Especialidade)
            : new VeterinarioAgregado(contextoTenant.TenantId, comando.Nome, email, telefone, crmv, comando.Especialidade);

        await repositorioVeterinario.AdicionarAsync(veterinario, tokenCancelamento);

        return veterinario.Id;
    }
}
