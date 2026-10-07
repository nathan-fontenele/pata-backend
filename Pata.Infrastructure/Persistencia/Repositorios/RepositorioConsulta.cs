using Microsoft.EntityFrameworkCore;
using Pata.Domain.Repositorios;
using Pata.Infrastructure.Persistencia;
using ConsultaAgregado = Pata.Domain.Entidades.Consulta.Consulta;

namespace Pata.Infrastructure.Persistencia.Repositorios;

internal sealed class RepositorioConsulta(PataDbContext contexto) : IRepositorioConsulta
{
    public Task<ConsultaAgregado?> ObterPorIdAsync(Guid id, CancellationToken tokenCancelamento = default) =>
        contexto.Consultas.Include(consulta => consulta.Prontuario).ThenInclude(prontuario => prontuario!.Sintomas)
            .SingleOrDefaultAsync(consulta => consulta.Id == id, tokenCancelamento);

    public Task<ConsultaAgregado?> ObterPorIdIncluindoExcluidosAsync(Guid id, CancellationToken tokenCancelamento = default) =>
        ObterPorIdAsync(id, tokenCancelamento);

    public async Task AdicionarAsync(ConsultaAgregado consulta, CancellationToken tokenCancelamento = default)
    {
        await contexto.Consultas.AddAsync(consulta, tokenCancelamento);
        await contexto.SaveChangesAsync(tokenCancelamento);
    }

    public async Task AtualizarAsync(ConsultaAgregado consulta, CancellationToken tokenCancelamento = default)
    {
        contexto.Consultas.Update(consulta);
        await contexto.SaveChangesAsync(tokenCancelamento);
    }
}
