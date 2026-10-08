using MediatR;
using Pata.Domain.Entidades.Cobranca;

namespace Pata.Application.Funcionalidades.Cobranca;

public sealed record AdicionarItemFaturaComando(Guid FaturaId, Guid? PrecoPlanoId, string Descricao,
    decimal Quantidade, decimal ValorUnitario, decimal Desconto, DateTimeOffset? PeriodoInicio,
    DateTimeOffset? PeriodoFim) : IRequest<ItemFatura>;
