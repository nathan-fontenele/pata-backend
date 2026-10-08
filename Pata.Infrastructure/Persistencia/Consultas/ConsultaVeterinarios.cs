using Microsoft.EntityFrameworkCore;
using Pata.Application.Comum.Modelos;
using Pata.Application.Funcionalidades.Veterinario.Consultas;
using Pata.Domain.ObjetosValor;
using Pata.Application.Comum.Abstracoes;

namespace Pata.Infrastructure.Persistencia.Consultas;

internal sealed class ConsultaVeterinarios(PataDbContext contexto, IContextoTenant contextoTenant) : IConsultaVeterinarios
{
    public async Task<VeterinarioResumoDto?> ObterPorCrmvAsync(Crmv crmv, CancellationToken tokenCancelamento = default) =>
        Projetar(await contexto.Veterinarios.AsNoTracking()
            .Where(veterinario => veterinario.Crmv == crmv)
            .SingleOrDefaultAsync(tokenCancelamento));

    public Task<RespostaPaginada<VeterinarioResumoDto>> ListarAsync(int pagina, int tamanhoPagina, CancellationToken tokenCancelamento = default) =>
        Listar(pagina, tamanhoPagina, excluidos: false, tokenCancelamento);

    public Task<RespostaPaginada<VeterinarioResumoDto>> ListarExcluidosAsync(int pagina, int tamanhoPagina, CancellationToken tokenCancelamento = default) =>
        Listar(pagina, tamanhoPagina, excluidos: true, tokenCancelamento);

    private async Task<RespostaPaginada<VeterinarioResumoDto>> Listar(
        int pagina, int tamanhoPagina, bool excluidos, CancellationToken tokenCancelamento)
    {
        var query = contexto.Veterinarios.IgnoreQueryFilters().AsNoTracking()
            .Where(veterinario => veterinario.TenantId == contextoTenant.TenantId
                                  && veterinario.Excluido == excluidos);
        var total = await query.CountAsync(tokenCancelamento);
        var itens = await query.OrderBy(veterinario => veterinario.Nome)
            .Skip((pagina - 1) * tamanhoPagina).Take(tamanhoPagina)
            .ToListAsync(tokenCancelamento);
        return new RespostaPaginada<VeterinarioResumoDto>(itens.Select(veterinario => Projetar(veterinario)!), pagina, tamanhoPagina, total);
    }

    private static VeterinarioResumoDto? Projetar(Pata.Domain.Entidades.Veterinario.Veterinario? veterinario) =>
        veterinario is null
            ? null
            : new(veterinario.Id, veterinario.Nome, veterinario.Email.Valor, veterinario.Telefone.Valor,
                veterinario.Crmv.Valor, veterinario.Especialidade, veterinario.Excluido, veterinario.ExcluidoEm);
}
