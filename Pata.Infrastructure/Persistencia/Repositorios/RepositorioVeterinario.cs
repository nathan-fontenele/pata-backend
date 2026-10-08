using Microsoft.EntityFrameworkCore;
using Pata.Domain.ObjetosValor;
using Pata.Domain.Repositorios;
using Pata.Infrastructure.Persistencia;
using VeterinarioAgregado = Pata.Domain.Entidades.Veterinario.Veterinario;

namespace Pata.Infrastructure.Persistencia.Repositorios;

internal sealed class RepositorioVeterinario(PataDbContext contexto) : IRepositorioVeterinario
{
    public Task<VeterinarioAgregado?> ObterPorIdAsync(Guid id, CancellationToken tokenCancelamento = default) =>
        contexto.Veterinarios.SingleOrDefaultAsync(veterinario => veterinario.Id == id, tokenCancelamento);

    public Task<VeterinarioAgregado?> ObterPorIdIncluindoExcluidosAsync(Guid id, CancellationToken tokenCancelamento = default) =>
        contexto.Veterinarios.IgnoreQueryFilters().SingleOrDefaultAsync(
            veterinario => veterinario.TenantId == contexto.TenantId && veterinario.Id == id, tokenCancelamento);

    public Task<VeterinarioAgregado?> ObterPorCrmvAsync(Crmv crmv, CancellationToken tokenCancelamento = default) =>
        contexto.Veterinarios.SingleOrDefaultAsync(veterinario => veterinario.Crmv == crmv, tokenCancelamento);

    public Task<VeterinarioAgregado?> ObterPorCrmvIncluindoExcluidosAsync(Crmv crmv, CancellationToken tokenCancelamento = default) =>
        contexto.Veterinarios.IgnoreQueryFilters().SingleOrDefaultAsync(
            veterinario => veterinario.TenantId == contexto.TenantId && veterinario.Crmv == crmv, tokenCancelamento);

    public Task<bool> ExisteCrmvAsync(Crmv crmv, CancellationToken tokenCancelamento = default) =>
        contexto.Veterinarios.AnyAsync(veterinario => veterinario.Crmv == crmv, tokenCancelamento);

    public Task<bool> ExisteCrmvIncluindoExcluidosAsync(Crmv crmv, CancellationToken tokenCancelamento = default) =>
        contexto.Veterinarios.IgnoreQueryFilters().AnyAsync(
            veterinario => veterinario.TenantId == contexto.TenantId && veterinario.Crmv == crmv, tokenCancelamento);

    public async Task AdicionarAsync(VeterinarioAgregado veterinario, CancellationToken tokenCancelamento = default)
    {
        await contexto.Veterinarios.AddAsync(veterinario, tokenCancelamento);
        await contexto.SaveChangesAsync(tokenCancelamento);
    }

    public async Task AtualizarAsync(VeterinarioAgregado veterinario, CancellationToken tokenCancelamento = default)
    {
        contexto.Veterinarios.Update(veterinario);
        await contexto.SaveChangesAsync(tokenCancelamento);
    }
}
