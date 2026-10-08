using System.Security.Cryptography;
using System.Text;
using Pata.Application.Comum.Abstracoes;
using Pata.Application.Comum.Mensagens;
using Pata.Application.Funcionalidades.Animal.Cadastro;
using Pata.Domain.Entidades.Tutor;
using Pata.Domain.Excecoes;
using Pata.Domain.ObjetosValor;
using TutorAgregado = Pata.Domain.Entidades.Tutor.Tutor;

namespace Pata.Application.Funcionalidades.Animal.Comandos.CadastrarPet;

public sealed class CadastrarPetManipulador(
    IRepositorioCadastroAnimal repositorio,
    IContextoTenant contextoTenant,
    IRelogio relogio) : IManipuladorComando<CadastrarPetComando, PetCadastrado>
{
    public async Task<PetCadastrado> Handle(CadastrarPetComando comando, CancellationToken cancellationToken)
    {
        var cpf = new Cpf(comando.TutorCpf);
        var tutor = await repositorio.ObterTutorPorCpfAsync(cpf, cancellationToken);
        var novoTutor = tutor is null;
        tutor ??= new TutorAgregado(contextoTenant.TenantId, comando.TutorNome, cpf,
            new Email(comando.TutorEmail), new Telefone(comando.TutorTelefone));

        if (tutor.Excluido)
            throw new ConflitoException("O tutor existe nesta clinica, mas esta excluido.");

        var animal = tutor.AdicionarAnimal(comando.Nome, comando.Especie, comando.Raca,
            comando.DataNascimento, DateOnly.FromDateTime(relogio.UtcAgora));

        var acesso = await repositorio.ObterAcessoTutorAsync(tutor.Id, cancellationToken);
        var tokenAcesso = acesso is null || acesso.RevogadoEm.HasValue ? GerarToken() : null;
        var novoAcesso = acesso is null;
        var agora = new DateTimeOffset(relogio.UtcAgora, TimeSpan.Zero);
        if (acesso is null)
            acesso = new AcessoPortalTutor(contextoTenant.TenantId, tutor.Id, CalcularHash(tokenAcesso!), agora);
        else if (tokenAcesso is not null)
            acesso.RotacionarToken(CalcularHash(tokenAcesso), agora);

        await repositorio.SalvarCadastroAsync(tutor, novoTutor, acesso, novoAcesso, cancellationToken);
        return new PetCadastrado(animal.Id, tutor.Id, tutor.Nome, animal.Nome, tokenAcesso, novoTutor);
    }

    internal static string GerarToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    internal static string CalcularHash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
