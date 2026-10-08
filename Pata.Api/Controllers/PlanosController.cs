using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pata.Api.DTOs;
using Pata.Api.DTOs.Gestao;
using Pata.Application.Comum.Recursos;
using Pata.Domain.Entidades.Cobranca;

namespace Pata.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/planos")]
[Produces("application/json")]
public sealed class PlanosController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PlanoDto>>> Listar(CancellationToken cancellationToken) =>
        Ok((await sender.Send(new ListarRegistrosConsulta<Plano>(), cancellationToken)).Select(PlanoDto.De).ToArray());

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PlanoDto>> Obter(Guid id, CancellationToken cancellationToken)
    {
        var item = await sender.Send(new ObterRegistroConsulta<Plano>(id), cancellationToken);
        return item is null ? NotFound() : Ok(PlanoDto.De(item));
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<RecursoCriadoDto>> Criar(PlanoRequest dto, CancellationToken cancellationToken)
    {
        var plano = new Plano(dto.Codigo, dto.Nome, dto.Descricao);
        var id = await sender.Send(new RegistrarRegistroComando<Plano>(plano), cancellationToken);
        return CreatedAtAction(nameof(Obter), new { id }, new RecursoCriadoDto(id));
    }

    [HttpGet("{planoId:guid}/precos")]
    public async Task<ActionResult<IReadOnlyList<PrecoPlanoDto>>> ListarPrecos(Guid planoId, CancellationToken cancellationToken)
    {
        var precos = await sender.Send(new ListarRegistrosConsulta<PrecoPlano>(), cancellationToken);
        return Ok(precos.Where(item => item.PlanoId == planoId).Select(PrecoPlanoDto.De).ToArray());
    }

    [HttpPost("{planoId:guid}/precos")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<RecursoCriadoDto>> CriarPreco(Guid planoId, PrecoPlanoRequest dto, CancellationToken cancellationToken)
    {
        var preco = new PrecoPlano(planoId, dto.Codigo, dto.Valor, dto.Moeda, dto.DiasTeste,
            dto.IntervaloUnidade, dto.IntervaloQuantidade);
        var id = await sender.Send(new RegistrarRegistroComando<PrecoPlano>(preco), cancellationToken);
        return CreatedAtAction(nameof(ListarPrecos), new { planoId }, new RecursoCriadoDto(id));
    }
}
