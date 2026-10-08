using MediatR;
using Microsoft.EntityFrameworkCore;
using Pata.Application.Comum.Recursos;

namespace Pata.Infrastructure.Persistencia.Recursos;

internal sealed class ListarRegistrosManipulador<TEntity>(PataDbContext contexto)
    : IRequestHandler<ListarRegistrosConsulta<TEntity>, IReadOnlyList<TEntity>>
    where TEntity : class
{
    public async Task<IReadOnlyList<TEntity>> Handle(ListarRegistrosConsulta<TEntity> request,
        CancellationToken cancellationToken) =>
        await contexto.Set<TEntity>().AsNoTracking().ToListAsync(cancellationToken);
}

internal sealed class ObterRegistroManipulador<TEntity>(PataDbContext contexto)
    : IRequestHandler<ObterRegistroConsulta<TEntity>, TEntity?>
    where TEntity : class
{
    public Task<TEntity?> Handle(ObterRegistroConsulta<TEntity> request, CancellationToken cancellationToken) =>
        contexto.Set<TEntity>().AsNoTracking()
            .SingleOrDefaultAsync(item => EF.Property<Guid>(item, "Id") == request.Id, cancellationToken);
}

internal sealed class RegistrarRegistroManipulador<TEntity>(PataDbContext contexto)
    : IRequestHandler<RegistrarRegistroComando<TEntity>, Guid>
    where TEntity : class
{
    public async Task<Guid> Handle(RegistrarRegistroComando<TEntity> request, CancellationToken cancellationToken)
    {
        await contexto.Set<TEntity>().AddAsync(request.Entidade, cancellationToken);
        await contexto.SaveChangesAsync(cancellationToken);
        return contexto.Entry(request.Entidade).Property<Guid>("Id").CurrentValue;
    }
}
