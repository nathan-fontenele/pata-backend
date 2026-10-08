using MediatR;
using Microsoft.EntityFrameworkCore;
using Pata.Application.Funcionalidades.Cobranca;
using Pata.Domain.Entidades.Cobranca;
using Pata.Domain.Excecoes;

namespace Pata.Infrastructure.Persistencia.Recursos;

internal sealed class AdicionarItemFaturaManipulador(PataDbContext contexto)
    : IRequestHandler<AdicionarItemFaturaComando, ItemFatura>
{
    public async Task<ItemFatura> Handle(AdicionarItemFaturaComando request, CancellationToken cancellationToken)
    {
        var fatura = await contexto.Faturas.Include(item => item.Itens)
            .SingleOrDefaultAsync(item => item.Id == request.FaturaId, cancellationToken)
            ?? throw new RecursoNaoEncontradoException("Fatura nao encontrada.");
        var item = fatura.AdicionarItem(request.PrecoPlanoId, request.Descricao, request.Quantidade,
            request.ValorUnitario, request.Desconto, request.PeriodoInicio, request.PeriodoFim);
        await contexto.SaveChangesAsync(cancellationToken);
        return item;
    }
}
