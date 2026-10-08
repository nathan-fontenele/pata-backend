using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pata.Api.DTOs;
using Pata.Api.DTOs.Gestao;
using Pata.Application.Comum.Abstracoes;
using Pata.Application.Comum.Recursos;
using Pata.Application.Funcionalidades.Cobranca;
using Pata.Domain.Entidades.Cobranca;

namespace Pata.Api.Controllers;

[ApiController]
[Authorize(Policy = "AdminClinica")]
[Route("api")]
[Produces("application/json")]
public sealed class CobrancasController(ISender sender, IContextoTenant tenant) : ControllerBase
{
    [HttpGet("assinaturas")]
    public async Task<ActionResult<IReadOnlyList<AssinaturaDto>>> ListarAssinaturas(CancellationToken cancellationToken) =>
        Ok((await sender.Send(new ListarRegistrosConsulta<Assinatura>(), cancellationToken)).Select(AssinaturaDto.De).ToArray());

    [HttpGet("assinaturas/{id:guid}")]
    public async Task<ActionResult<AssinaturaDto>> ObterAssinatura(Guid id, CancellationToken cancellationToken)
    {
        var item = await sender.Send(new ObterRegistroConsulta<Assinatura>(id), cancellationToken);
        return item is null ? NotFound() : Ok(AssinaturaDto.De(item));
    }

    [HttpPost("assinaturas")]
    public async Task<ActionResult<RecursoCriadoDto>> CriarAssinatura(AssinaturaRequest dto, CancellationToken cancellationToken)
    {
        var assinatura = new Assinatura(tenant.TenantId, dto.PrecoPlanoId, dto.InicioEm, dto.TesteInicio, dto.TesteFim);
        var id = await sender.Send(new RegistrarRegistroComando<Assinatura>(assinatura), cancellationToken);
        return CreatedAtAction(nameof(ObterAssinatura), new { id }, new RecursoCriadoDto(id));
    }

    [HttpGet("faturas")]
    public async Task<ActionResult<IReadOnlyList<FaturaDto>>> ListarFaturas(CancellationToken cancellationToken) =>
        Ok((await sender.Send(new ListarFaturasConsulta(), cancellationToken)).Select(FaturaDto.De).ToArray());

    [HttpGet("faturas/{id:guid}")]
    public async Task<ActionResult<FaturaDto>> ObterFatura(Guid id, CancellationToken cancellationToken)
    {
        var item = await sender.Send(new ObterFaturaConsulta(id), cancellationToken);
        return item is null ? NotFound() : Ok(FaturaDto.De(item));
    }

    [HttpPost("faturas")]
    public async Task<ActionResult<RecursoCriadoDto>> CriarFatura(FaturaRequest dto, CancellationToken cancellationToken)
    {
        var fatura = new Fatura(tenant.TenantId, dto.AssinaturaId, dto.Numero, dto.Moeda, dto.EmitidaEm, dto.VencimentoEm);
        var id = await sender.Send(new RegistrarRegistroComando<Fatura>(fatura), cancellationToken);
        return CreatedAtAction(nameof(ObterFatura), new { id }, new RecursoCriadoDto(id));
    }

    [HttpPost("faturas/{id:guid}/itens")]
    public async Task<ActionResult<ItemFaturaDto>> AdicionarItem(Guid id, ItemFaturaRequest dto, CancellationToken cancellationToken)
    {
        var item = await sender.Send(new AdicionarItemFaturaComando(id, dto.PrecoPlanoId, dto.Descricao,
            dto.Quantidade, dto.ValorUnitario, dto.Desconto, dto.PeriodoInicio, dto.PeriodoFim), cancellationToken);
        return CreatedAtAction(nameof(ObterFatura), new { id }, ItemFaturaDto.De(item));
    }

    [HttpGet("pagamentos")]
    public async Task<ActionResult<IReadOnlyList<PagamentoDto>>> ListarPagamentos(CancellationToken cancellationToken) =>
        Ok((await sender.Send(new ListarRegistrosConsulta<Pagamento>(), cancellationToken)).Select(PagamentoDto.De).ToArray());

    [HttpGet("pagamentos/{id:guid}")]
    public async Task<ActionResult<PagamentoDto>> ObterPagamento(Guid id, CancellationToken cancellationToken)
    {
        var item = await sender.Send(new ObterRegistroConsulta<Pagamento>(id), cancellationToken);
        return item is null ? NotFound() : Ok(PagamentoDto.De(item));
    }

    [HttpPost("faturas/{faturaId:guid}/pagamentos")]
    public async Task<ActionResult<RecursoCriadoDto>> CriarPagamento(Guid faturaId, PagamentoRequest dto, CancellationToken cancellationToken)
    {
        var pagamento = new Pagamento(tenant.TenantId, faturaId, dto.Valor, dto.Moeda, dto.Metodo, dto.Provedor,
            dto.ChaveIdempotencia);
        var id = await sender.Send(new RegistrarRegistroComando<Pagamento>(pagamento), cancellationToken);
        return CreatedAtAction(nameof(ObterPagamento), new { id }, new RecursoCriadoDto(id));
    }
}
