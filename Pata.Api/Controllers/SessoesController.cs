using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pata.Api.Servicos;
using Pata.Domain.Entidades.Organizacao;
using Pata.Domain.Repositorios;

namespace Pata.Api.Controllers;

[ApiController]
[Route("api/sessoes")]
[Produces("application/json")]
public sealed class SessoesController(
    IRepositorioOrganizacao repositorio,
    ServicoSessaoPata sessao,
    IWebHostEnvironment ambiente,
    TimeProvider relogio) : ControllerBase
{
    [HttpGet("organizacoes")]
    [Authorize(AuthenticationSchemes = "Auth0AccessToken")]
    public async Task<ActionResult<IReadOnlyList<OrganizacaoDisponivelDto>>> ListarOrganizacoes(
        CancellationToken cancellationToken)
    {
        var sub = User.FindFirst("sub")?.Value;
        if (string.IsNullOrWhiteSpace(sub))
            return Unauthorized();

        var acessos = await repositorio.ListarAcessosAuth0Async(sub, cancellationToken);
        return Ok(acessos.Select(acesso => new OrganizacaoDisponivelDto(
            acesso.TenantId, acesso.Nome, acesso.Slug, acesso.Perfil.ToString().ToLowerInvariant())).ToArray());
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = "Auth0AccessToken")]
    public async Task<ActionResult<SessaoCriadaDto>> SelecionarOrganizacao(
        SelecionarOrganizacaoDto dto,
        CancellationToken cancellationToken)
    {
        var sub = User.FindFirst("sub")?.Value;
        if (string.IsNullOrWhiteSpace(sub))
            return Unauthorized();

        var acesso = await repositorio.ObterAcessoAuth0Async(sub, dto.TenantId, cancellationToken);
        if (acesso is null)
            return Forbid("Auth0AccessToken");

        var token = sessao.CriarToken(sub, acesso);
        Response.Cookies.Append("pata_session", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = !ambiente.IsDevelopment(),
            SameSite = SameSiteMode.Lax,
            IsEssential = true,
            Path = "/",
            MaxAge = ServicoSessaoPata.DuracaoSessao,
            Expires = relogio.GetUtcNow().Add(ServicoSessaoPata.DuracaoSessao)
        });

        return Ok(new SessaoCriadaDto(acesso.TenantId, acesso.Perfil.ToString().ToLowerInvariant()));
    }

    [HttpDelete]
    [AllowAnonymous]
    public IActionResult Encerrar()
    {
        Response.Cookies.Delete("pata_session", new CookieOptions
        {
            Secure = !ambiente.IsDevelopment(),
            SameSite = SameSiteMode.Lax,
            HttpOnly = true,
            Path = "/"
        });
        return NoContent();
    }
}

public sealed record SelecionarOrganizacaoDto(Guid TenantId);
public sealed record OrganizacaoDisponivelDto(Guid TenantId, string Nome, string Slug, string Papel);
public sealed record SessaoCriadaDto(Guid TenantId, string Papel);
