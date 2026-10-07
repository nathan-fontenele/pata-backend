using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pata.Api.DTOs;
using Pata.Api.DTOs.Tutores;
using Pata.Application.Funcionalidades.Tutor.Comandos.AtualizarTutor;
using Pata.Application.Funcionalidades.Tutor.Comandos.CriarTutor;
using Pata.Application.Funcionalidades.Tutor.Comandos.ExcluirTutor;
using Pata.Application.Funcionalidades.Tutor.Comandos.RecuperarTutor;
using Pata.Application.Funcionalidades.Tutor.Consultas.ListarTutores;
using Pata.Application.Funcionalidades.Tutor.Consultas.ListarTutoresExcluidos;
using Pata.Application.Funcionalidades.Tutor.Consultas.ObterTutorPorCpf;

namespace Pata.Api.Controllers;

[ApiController]
[Route("api/tutores")]
[Produces("application/json")]
public sealed class TutoresController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginaDto<TutorDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginaDto<TutorDto>>> Listar(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20,
        CancellationToken cancellationToken = default)
    {
        var resposta = await sender.Send(new ListarTutoresConsulta(pagina, tamanhoPagina), cancellationToken);
        return Ok(PaginaDto.De(resposta, TutorDto.De));
    }

    [HttpGet("excluidos")]
    [ProducesResponseType(typeof(PaginaDto<TutorDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginaDto<TutorDto>>> ListarExcluidos(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20,
        CancellationToken cancellationToken = default)
    {
        var resposta = await sender.Send(new ListarTutoresExcluidosConsulta(pagina, tamanhoPagina), cancellationToken);
        return Ok(PaginaDto.De(resposta, TutorDto.De));
    }

    [HttpGet("cpf/{cpf}", Name = nameof(ObterPorCpf))]
    [ProducesResponseType(typeof(TutorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TutorDto>> ObterPorCpf(string cpf, CancellationToken cancellationToken)
    {
        var tutor = await sender.Send(new ObterTutorPorCpfConsulta(cpf), cancellationToken);
        return tutor is null ? NotFound() : Ok(TutorDto.De(tutor));
    }

    [HttpPost]
    [ProducesResponseType(typeof(RecursoCriadoDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<RecursoCriadoDto>> Criar(
        CriarTutorDto dto,
        CancellationToken cancellationToken)
    {
        var id = await sender.Send(new CriarTutorComando(dto.Nome, dto.Cpf, dto.Email, dto.Telefone), cancellationToken);
        var resposta = new RecursoCriadoDto(id);
        return CreatedAtRoute(nameof(ObterPorCpf), new { cpf = dto.Cpf }, resposta);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Atualizar(
        Guid id,
        AtualizarTutorDto dto,
        CancellationToken cancellationToken)
    {
        await sender.Send(new AtualizarTutorComando(id, dto.Nome, dto.Email, dto.Telefone), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new ExcluirTutorComando(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/recuperacao")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Recuperar(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new RecuperarTutorComando(id), cancellationToken);
        return NoContent();
    }
}
