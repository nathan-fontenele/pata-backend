using MediatR;

namespace Pata.Application.Comum.Recursos;

public sealed record ListarRegistrosConsulta<TEntity>() : IRequest<IReadOnlyList<TEntity>>
    where TEntity : class;
