using Microsoft.EntityFrameworkCore;
using Pata.Domain.ObjetosValor;
using Pata.Domain.Repositorios;
using Pata.Infrastructure.Persistencia;
using TutorAgregado = Pata.Domain.Entidades.Tutor.Tutor;

namespace Pata.Infrastructure.Persistencia.Repositorios;

internal sealed class RepositorioTutor(PataDbContext contexto) : IRepositorioTutor
{
    public Task<TutorAgregado?> ObterPorIdAsync(Guid id, CancellationToken tokenCancelamento = default) =>
        contexto.Tutores.SingleOrDefaultAsync(tutor => tutor.Id == id, tokenCancelamento);

    public Task<TutorAgregado?> ObterPorIdIncluindoExcluidosAsync(Guid id, CancellationToken tokenCancelamento = default) =>
        contexto.Tutores.IgnoreQueryFilters().SingleOrDefaultAsync(tutor => tutor.Id == id, tokenCancelamento);

    public Task<TutorAgregado?> ObterPorCpfAsync(Cpf cpf, CancellationToken tokenCancelamento = default) =>
        contexto.Tutores.SingleOrDefaultAsync(tutor => tutor.Cpf == cpf, tokenCancelamento);

    public Task<TutorAgregado?> ObterPorCpfIncluindoExcluidosAsync(Cpf cpf, CancellationToken tokenCancelamento = default) =>
        contexto.Tutores.IgnoreQueryFilters().SingleOrDefaultAsync(tutor => tutor.Cpf == cpf, tokenCancelamento);

    public Task<bool> ExisteCpfAsync(Cpf cpf, CancellationToken tokenCancelamento = default) =>
        contexto.Tutores.AnyAsync(tutor => tutor.Cpf == cpf, tokenCancelamento);

    public Task<bool> ExisteCpfIncluindoExcluidosAsync(Cpf cpf, CancellationToken tokenCancelamento = default) =>
        contexto.Tutores.IgnoreQueryFilters().AnyAsync(tutor => tutor.Cpf == cpf, tokenCancelamento);

    public async Task AdicionarAsync(TutorAgregado tutor, CancellationToken tokenCancelamento = default)
    {
        await contexto.Tutores.AddAsync(tutor, tokenCancelamento);
        await contexto.SaveChangesAsync(tokenCancelamento);
    }

    public async Task AtualizarAsync(TutorAgregado tutor, CancellationToken tokenCancelamento = default)
    {
        contexto.Tutores.Update(tutor);
        await contexto.SaveChangesAsync(tokenCancelamento);
    }
}
