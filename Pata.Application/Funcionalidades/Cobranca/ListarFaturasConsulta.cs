using MediatR;
using Pata.Domain.Entidades.Cobranca;

namespace Pata.Application.Funcionalidades.Cobranca;

public sealed record ListarFaturasConsulta() : IRequest<IReadOnlyList<Fatura>>;
