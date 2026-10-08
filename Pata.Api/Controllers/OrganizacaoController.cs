using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pata.Api.DTOs;
using Pata.Api.DTOs.Gestao;
using Pata.Application.Comum.Abstracoes;
using Pata.Application.Comum.Recursos;
using Pata.Domain.Entidades.Organizacao;

namespace Pata.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/organizacao")]
[Produces("application/json")]
public sealed class OrganizacaoController(ISender sender, IContextoTenant tenant, TimeProvider relogio) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<OrganizacaoDto>> Obter(CancellationToken cancellationToken)
    {
        var item = await sender.Send(new ObterRegistroConsulta<Organizacao>(tenant.TenantId), cancellationToken);
        return item is null ? NotFound() : Ok(OrganizacaoDto.De(item));
    }

    [HttpGet("equipes")]
    public async Task<ActionResult<IReadOnlyList<EquipeDto>>> ListarEquipes(CancellationToken cancellationToken) =>
        Ok((await sender.Send(new ListarRegistrosConsulta<Equipe>(), cancellationToken)).Select(EquipeDto.De).ToArray());

    [HttpGet("equipes/{id:guid}")]
    public async Task<ActionResult<EquipeDto>> ObterEquipe(Guid id, CancellationToken cancellationToken)
    {
        var item = await sender.Send(new ObterRegistroConsulta<Equipe>(id), cancellationToken);
        return item is null ? NotFound() : Ok(EquipeDto.De(item));
    }

    [HttpPost("equipes")]
    public async Task<ActionResult<RecursoCriadoDto>> CriarEquipe(EquipeRequest dto, CancellationToken cancellationToken)
    {
        var id = await sender.Send(new RegistrarRegistroComando<Equipe>(new Equipe(tenant.TenantId, dto.Nome, dto.Descricao)), cancellationToken);
        return CreatedAtAction(nameof(ObterEquipe), new { id }, new RecursoCriadoDto(id));
    }

    [HttpGet("usuarios")]
    public async Task<ActionResult<IReadOnlyList<UsuarioDto>>> ListarUsuarios(CancellationToken cancellationToken) =>
        Ok((await sender.Send(new ListarRegistrosConsulta<Usuario>(), cancellationToken)).Select(UsuarioDto.De).ToArray());

    [HttpGet("usuarios/{id:guid}")]
    public async Task<ActionResult<UsuarioDto>> ObterUsuario(Guid id, CancellationToken cancellationToken)
    {
        var item = await sender.Send(new ObterRegistroConsulta<Usuario>(id), cancellationToken);
        return item is null ? NotFound() : Ok(UsuarioDto.De(item));
    }

    [HttpPost("usuarios")]
    public async Task<ActionResult<RecursoCriadoDto>> CriarUsuario(UsuarioRequest dto, CancellationToken cancellationToken)
    {
        var usuario = new Usuario(tenant.TenantId, dto.EquipeId, dto.Nome, dto.Sobrenome, dto.Email, dto.Telefone, dto.Cargo);
        var id = await sender.Send(new RegistrarRegistroComando<Usuario>(usuario), cancellationToken);
        return CreatedAtAction(nameof(ObterUsuario), new { id }, new RecursoCriadoDto(id));
    }

    [HttpGet("convites")]
    public async Task<ActionResult<IReadOnlyList<ConviteDto>>> ListarConvites(CancellationToken cancellationToken) =>
        Ok((await sender.Send(new ListarRegistrosConsulta<ConviteUsuario>(), cancellationToken)).Select(ConviteDto.De).ToArray());

    [HttpPost("convites")]
    public async Task<ActionResult<ConviteCriadoDto>> CriarConvite(ConviteRequest dto, CancellationToken cancellationToken)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        var criadoEm = relogio.GetUtcNow();
        var autor = Guid.TryParse(User.FindFirst("sub")?.Value, out var usuarioId) ? usuarioId : (Guid?)null;
        var convite = new ConviteUsuario(tenant.TenantId, dto.UsuarioId, hash, criadoEm, dto.ExpiraEm, autor);
        var id = await sender.Send(new RegistrarRegistroComando<ConviteUsuario>(convite), cancellationToken);
        return CreatedAtAction(nameof(ListarConvites), null, new ConviteCriadoDto(id, token, convite.ExpiraEm));
    }
}

public sealed record ConviteCriadoDto(Guid Id, string Token, DateTimeOffset ExpiraEm);
