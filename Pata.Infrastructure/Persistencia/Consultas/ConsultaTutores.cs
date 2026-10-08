using Microsoft.EntityFrameworkCore;
using Pata.Application.Comum.Modelos;
using Pata.Application.Funcionalidades.Tutor.Consultas;
using Pata.Domain.ObjetosValor;
using Pata.Application.Comum.Abstracoes;

namespace Pata.Infrastructure.Persistencia.Consultas;

internal sealed class ConsultaTutores(PataDbContext contexto, IContextoTenant contextoTenant) : IConsultaTutores
{
    public async Task<TutorResumoDto?> ObterPorCpfAsync(Cpf cpf, CancellationToken tokenCancelamento = default) =>
        Projetar(await contexto.Tutores.AsNoTracking()
            .Where(tutor => tutor.Cpf == cpf)
            .SingleOrDefaultAsync(tokenCancelamento));

    public Task<RespostaPaginada<TutorResumoDto>> ListarAsync(int pagina, int tamanhoPagina, CancellationToken tokenCancelamento = default) =>
        Listar(pagina, tamanhoPagina, excluidos: false, tokenCancelamento);

    public Task<RespostaPaginada<TutorResumoDto>> ListarExcluidosAsync(int pagina, int tamanhoPagina, CancellationToken tokenCancelamento = default) =>
        Listar(pagina, tamanhoPagina, excluidos: true, tokenCancelamento);

    private async Task<RespostaPaginada<TutorResumoDto>> Listar(
        int pagina, int tamanhoPagina, bool excluidos, CancellationToken tokenCancelamento)
    {
        var query = contexto.Tutores.IgnoreQueryFilters().AsNoTracking()
            .Where(tutor => tutor.TenantId == contextoTenant.TenantId && tutor.Excluido == excluidos);
        var total = await query.CountAsync(tokenCancelamento);
        var itens = await query.OrderBy(tutor => tutor.Nome)
            .Skip((pagina - 1) * tamanhoPagina).Take(tamanhoPagina)
            .ToListAsync(tokenCancelamento);
        return new RespostaPaginada<TutorResumoDto>(itens.Select(tutor => Projetar(tutor)!), pagina, tamanhoPagina, total);
    }

    private static TutorResumoDto? Projetar(Pata.Domain.Entidades.Tutor.Tutor? tutor) => tutor is null
        ? null
        : new TutorResumoDto(tutor.Id, tutor.Nome, tutor.Cpf.Valor, tutor.Email.Valor,
            tutor.Telefone.Valor, tutor.Excluido, tutor.ExcluidoEm);
}
