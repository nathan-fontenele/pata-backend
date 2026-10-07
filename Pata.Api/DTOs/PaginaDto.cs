using Pata.Application.Comum.Modelos;

namespace Pata.Api.DTOs;

public sealed record PaginaDto<T>(
    IReadOnlyCollection<T> Itens,
    int Pagina,
    int TamanhoPagina,
    int TotalItens,
    int TotalPaginas);

public static class PaginaDto
{
    public static PaginaDto<TDestino> De<TOrigem, TDestino>(
        RespostaPaginada<TOrigem> resposta,
        Func<TOrigem, TDestino> mapear) =>
        new(resposta.Itens.Select(mapear).ToArray(), resposta.Pagina, resposta.TamanhoPagina,
            resposta.TotalItens, resposta.TotalPaginas);
}
