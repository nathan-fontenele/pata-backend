using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pata.Api.DTOs;
using Pata.Api.DTOs.Veterinarios;
using Pata.Application.Funcionalidades.Veterinario.Comandos.AtualizarVeterinario;
using Pata.Application.Funcionalidades.Veterinario.Comandos.CriarVeterinario;
using Pata.Application.Funcionalidades.Veterinario.Comandos.ExcluirVeterinario;
using Pata.Application.Funcionalidades.Veterinario.Comandos.RecuperarVeterinario;
using Pata.Application.Funcionalidades.Veterinario.Consultas.ListarVeterinarios;
using Pata.Application.Funcionalidades.Veterinario.Consultas.ListarVeterinariosExcluidos;
using Pata.Application.Funcionalidades.Veterinario.Consultas.ObterVeterinarioPorCrmv;

namespace Pata.Api.Controllers;

[ApiController]
[Route("api/veterinarios")]
[Produces("application/json")]
public sealed class VeterinariosController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginaDto<VeterinarioDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginaDto<VeterinarioDto>>> Listar(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20,
        CancellationToken cancellationToken = default)
    {
        var resposta = await sender.Send(new ListarVeterinariosConsulta(pagina, tamanhoPagina), cancellationToken);
        return Ok(PaginaDto.De(resposta, VeterinarioDto.De));
    }

    [HttpGet("excluidos")]
    [ProducesResponseType(typeof(PaginaDto<VeterinarioDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginaDto<VeterinarioDto>>> ListarExcluidos(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20,
        CancellationToken cancellationToken = default)
    {
        var resposta = await sender.Send(new ListarVeterinariosExcluidosConsulta(pagina, tamanhoPagina), cancellationToken);
        return Ok(PaginaDto.De(resposta, VeterinarioDto.De));
    }

    [HttpGet("crmv/{uf}/{numero}", Name = nameof(ObterPorCrmv))]
    [ProducesResponseType(typeof(VeterinarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VeterinarioDto>> ObterPorCrmv(
        string uf,
        string numero,
        CancellationToken cancellationToken)
    {
        var veterinario = await sender.Send(new ObterVeterinarioPorCrmvConsulta(numero, uf), cancellationToken);
        return veterinario is null ? NotFound() : Ok(VeterinarioDto.De(veterinario));
    }

    [HttpPost]
    [ProducesResponseType(typeof(RecursoCriadoDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<RecursoCriadoDto>> Criar(
        CriarVeterinarioDto dto,
        CancellationToken cancellationToken)
    {
        var id = await sender.Send(new CriarVeterinarioComando(dto.Nome, dto.Email, dto.Telefone,
            dto.NumeroCrmv, dto.UfCrmv, dto.Especialidade), cancellationToken);
        var resposta = new RecursoCriadoDto(id);
        return CreatedAtRoute(nameof(ObterPorCrmv), new { uf = dto.UfCrmv, numero = dto.NumeroCrmv }, resposta);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Atualizar(
        Guid id,
        AtualizarVeterinarioDto dto,
        CancellationToken cancellationToken)
    {
        await sender.Send(new AtualizarVeterinarioComando(id, dto.Nome, dto.Email, dto.Telefone, dto.Especialidade), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new ExcluirVeterinarioComando(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/recuperacao")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Recuperar(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new RecuperarVeterinarioComando(id), cancellationToken);
        return NoContent();
    }
}
