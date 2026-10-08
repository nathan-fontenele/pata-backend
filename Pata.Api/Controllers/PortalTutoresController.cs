using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pata.Application.Funcionalidades.Animal.Portal;

namespace Pata.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/portal/tutores")]
[Produces("application/json")]
public sealed class PortalTutoresController(ISender sender) : ControllerBase
{
    [HttpGet("{token}", Name = nameof(Obter))]
    public async Task<ActionResult<PortalTutorDto>> Obter(string token, CancellationToken cancellationToken)
    {
        var portal = await sender.Send(new ConsultarPortalTutorConsulta(token), cancellationToken);
        return portal is null ? NotFound() : Ok(portal);
    }
}
