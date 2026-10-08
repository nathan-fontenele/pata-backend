using MediatR;

namespace Pata.Application.Comum.Recursos;

public sealed record ObterRegistroConsulta<TEntity>(Guid Id) : IRequest<TEntity?>
    where TEntity : class;
