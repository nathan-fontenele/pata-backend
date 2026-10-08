using MediatR;

namespace Pata.Application.Comum.Recursos;

public sealed record RegistrarRegistroComando<TEntity>(TEntity Entidade) : IRequest<Guid>
    where TEntity : class;
