using Microsoft.EntityFrameworkCore;
using Pata.Application.Comum.Abstracoes;
using Pata.Application.Funcionalidades.Animal.Cadastro;
using Pata.Application.Funcionalidades.Animal.Portal;
using Pata.Domain.Entidades.Animal;
using Pata.Domain.Entidades.Consulta;
using Pata.Domain.Entidades.Tutor;
using Pata.Domain.ObjetosValor;

namespace Pata.Infrastructure.Persistencia.Recursos;

internal sealed class RepositorioCadastroAnimal(PataDbContext contexto, IContextoTenant contextoTenant)
    : IRepositorioCadastroAnimal, IConsultaPortalTutor
{
    public Task<Tutor?> ObterTutorPorCpfAsync(Cpf cpf, CancellationToken cancellationToken) =>
        contexto.Tutores.IgnoreQueryFilters().Include(tutor => tutor.Animais)
            .SingleOrDefaultAsync(tutor => tutor.TenantId == contextoTenant.TenantId && tutor.Cpf == cpf,
                cancellationToken);

    public Task<AcessoPortalTutor?> ObterAcessoTutorAsync(Guid tutorId, CancellationToken cancellationToken) =>
        contexto.AcessosPortalTutor.SingleOrDefaultAsync(acesso => acesso.TutorId == tutorId, cancellationToken);

    public async Task SalvarAcessoAsync(AcessoPortalTutor acesso, bool novoAcesso, CancellationToken cancellationToken)
    {
        if (novoAcesso)
            await contexto.AcessosPortalTutor.AddAsync(acesso, cancellationToken);
        await contexto.SaveChangesAsync(cancellationToken);
    }

    public async Task SalvarCadastroAsync(Tutor tutor, bool novoTutor, AcessoPortalTutor acesso,
        bool novoAcesso, CancellationToken cancellationToken)
    {
        if (novoTutor)
            await contexto.Tutores.AddAsync(tutor, cancellationToken);
        if (novoAcesso)
            await contexto.AcessosPortalTutor.AddAsync(acesso, cancellationToken);
        await contexto.SaveChangesAsync(cancellationToken);
    }

    public async Task<PortalTutorDto?> ConsultarAsync(string tokenHash, CancellationToken cancellationToken)
    {
        var acesso = await contexto.AcessosPortalTutor.IgnoreQueryFilters().AsNoTracking()
            .SingleOrDefaultAsync(item => item.TokenHash == tokenHash && item.RevogadoEm == null,
                cancellationToken);
        if (acesso is null)
            return null;

        var tutor = await contexto.Tutores.IgnoreQueryFilters().AsNoTracking()
            .Where(item => item.Id == acesso.TutorId && item.TenantId == acesso.TenantId && !item.Excluido)
            .Select(item => item.Nome)
            .SingleOrDefaultAsync(cancellationToken);
        if (tutor is null)
            return null;

        var registrosPet = await contexto.Set<Animal>().IgnoreQueryFilters().AsNoTracking()
            .Where(item => item.TutorId == acesso.TutorId && item.TenantId == acesso.TenantId)
            .Select(item => new { item.Id, item.Nome, item.Especie, item.Raca })
            .ToListAsync(cancellationToken);
        var pets = registrosPet.Select(item => new PetAcompanhamentoDto(item.Id, item.Nome,
            item.Especie.ToString(), item.Raca, "SemConsulta", null)).ToList();

        var ids = pets.Select(item => item.Id).ToArray();
        if (ids.Length == 0)
            return new PortalTutorDto(tutor, pets);

        var consultas = await contexto.Consultas.IgnoreQueryFilters().AsNoTracking()
            .Where(item => item.TenantId == acesso.TenantId && !item.Excluido && ids.Contains(item.AnimalId))
            .OrderByDescending(item => item.DataHora)
            .Select(item => new { item.AnimalId, item.Status, item.DataHora })
            .ToListAsync(cancellationToken);

        var statusRecente = consultas.GroupBy(item => item.AnimalId)
            .ToDictionary(grupo => grupo.Key, grupo => grupo.First());
        var resposta = pets.Select(pet => statusRecente.TryGetValue(pet.Id, out var consulta)
            ? pet with { Status = consulta.Status.ToString(), DataStatus = consulta.DataHora }
            : pet).ToArray();

        return new PortalTutorDto(tutor, resposta);
    }
}
