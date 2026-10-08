using Pata.Application.Comum.Mensagens;
using Pata.Application.Comum.Abstracoes;
using Pata.Domain.Entidades.Organizacao;
using Pata.Domain.Excecoes;
using Pata.Domain.Repositorios;
using OrganizacaoAgregado = Pata.Domain.Entidades.Organizacao.Organizacao;

namespace Pata.Application.Funcionalidades.Organizacao.Comandos.CriarOrganizacao;

public sealed class CriarOrganizacaoManipulador(IRepositorioOrganizacao repositorio, IContextoTenantDefinivel contextoTenant)
    : IManipuladorComando<CriarOrganizacaoComando, Guid>
{
    public async Task<Guid> Handle(CriarOrganizacaoComando comando, CancellationToken cancellationToken)
    {
        var organizacao = new OrganizacaoAgregado(comando.Nome, comando.Slug, comando.Cnpj, comando.Email,
            comando.LogoUrl);

        if (await repositorio.ExisteSlugAsync(organizacao.Slug, cancellationToken))
            throw new ConflitoException("Ja existe uma organizacao com o slug informado.");

        contextoTenant.DefinirTenant(organizacao.Id);
        var equipe = new Equipe(organizacao.Id, "Administradores", "Equipe inicial da clinica.");
        var admin = new Usuario(organizacao.Id, equipe.Id, comando.NomeAdmin, comando.SobrenomeAdmin,
            comando.EmailAdmin, null, "Administrador", StatusUsuario.Ativo, comando.Auth0Sub, PerfilUsuario.Admin);
        await repositorio.CriarComPrimeiroAdminAsync(organizacao, equipe, admin, cancellationToken);
        return organizacao.Id;
    }
}
