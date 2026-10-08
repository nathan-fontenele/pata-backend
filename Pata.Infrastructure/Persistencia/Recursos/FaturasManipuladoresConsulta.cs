using MediatR;
using Microsoft.EntityFrameworkCore;
using Pata.Application.Funcionalidades.Cobranca;
using Pata.Domain.Entidades.Cobranca;

namespace Pata.Infrastructure.Persistencia.Recursos;

internal sealed class ListarFaturasManipulador(PataDbContext contexto)
    : IRequestHandler<ListarFaturasConsulta, IReadOnlyList<Fatura>>
{
    public async Task<IReadOnlyList<Fatura>> Handle(ListarFaturasConsulta request, CancellationToken cancellationToken) =>
        await contexto.Faturas.AsNoTracking().Include(item => item.Itens).ToListAsync(cancellationToken);
}

internal sealed class ObterFaturaManipulador(PataDbContext contexto)
    : IRequestHandler<ObterFaturaConsulta, Fatura?>
{
    public Task<Fatura?> Handle(ObterFaturaConsulta request, CancellationToken cancellationToken) =>
        contexto.Faturas.AsNoTracking().Include(item => item.Itens)
            .SingleOrDefaultAsync(item => item.Id == request.Id, cancellationToken);
}
