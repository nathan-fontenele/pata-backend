using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pata.Api.DTOs.Gestao;
using Pata.Application.Comum.Recursos;
using Pata.Domain.Entidades.Auditoria;

namespace Pata.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/auditorias")]
[Produces("application/json")]
public sealed class AuditoriasController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AuditoriaDto>>> Listar(CancellationToken cancellationToken) =>
        Ok((await sender.Send(new ListarRegistrosConsulta<Auditoria>(), cancellationToken)).Select(AuditoriaDto.De).ToArray());

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AuditoriaDto>> Obter(Guid id, CancellationToken cancellationToken)
    {
        var item = await sender.Send(new ObterRegistroConsulta<Auditoria>(id), cancellationToken);
        return item is null ? NotFound() : Ok(AuditoriaDto.De(item));
    }
}
