using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Pata.Api.DTOs;
using Pata.Api.DTOs.Gestao;
using Pata.Api.Servicos;
using Pata.Application.Comum.Abstracoes;
using Pata.Application.Comum.Recursos;
using Pata.Application.Funcionalidades.Organizacao.Comandos.CriarOrganizacao;
using Pata.Domain.Entidades.Organizacao;
using Pata.Domain.Repositorios;

namespace Pata.Api.Controllers;

[ApiController]
[Route("api/organizacao")]
[Produces("application/json")]
public sealed class OrganizacaoController(
    ISender sender,
    IContextoTenant tenant,
    IRepositorioOrganizacao repositorioOrganizacao,
    ServicoSessaoPata sessao,
    IWebHostEnvironment ambiente,
    TimeProvider relogio) : ControllerBase
{
    [HttpPost]
    [Authorize(AuthenticationSchemes = "Auth0AccessToken")]
    [EnableRateLimiting("cadastro-organizacao")]
    [ProducesResponseType(typeof(CriarOrganizacaoResposta), StatusCodes.Status201Created)]
    public async Task<ActionResult<CriarOrganizacaoResposta>> Criar(
        CriarOrganizacaoDto dto,
        CancellationToken cancellationToken)
    {
        var auth0Sub = User.FindFirst("sub")?.Value;
        if (string.IsNullOrWhiteSpace(auth0Sub))
            return Unauthorized();

        var emailAdmin = User.FindFirst("email")?.Value ?? dto.Email;
        var nomeCompleto = User.FindFirst("name")?.Value?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? [];
        var nomeAdmin = User.FindFirst("given_name")?.Value ?? nomeCompleto.FirstOrDefault() ?? dto.Nome;
        var sobrenomeAdmin = User.FindFirst("family_name")?.Value
            ?? (nomeCompleto.Length > 1 ? string.Join(" ", nomeCompleto.Skip(1)) : "Administrador");
        var id = await sender.Send(new CriarOrganizacaoComando(dto.Nome, dto.Slug, dto.Cnpj, dto.Email,
            dto.LogoUrl, auth0Sub, nomeAdmin, sobrenomeAdmin, emailAdmin), cancellationToken);
        var acesso = await repositorioOrganizacao.ObterAcessoAuth0Async(auth0Sub, id, cancellationToken);
        if (acesso is null)
            throw new InvalidOperationException("O administrador inicial nao foi associado a organizacao.");
        DefinirCookieSessao(sessao.CriarToken(auth0Sub, acesso));
        var resposta = new OrganizacaoDto(id, dto.Nome.Trim(), dto.Slug.Trim().ToLowerInvariant(),
            StatusOrganizacao.Ativa.ToString(), string.Concat(dto.Cnpj.Where(char.IsDigit)), dto.Email.Trim(),
            string.IsNullOrWhiteSpace(dto.LogoUrl) ? null : dto.LogoUrl.Trim());
        return CreatedAtAction(nameof(Obter), null,
            new CriarOrganizacaoResposta(resposta, acesso.TenantId, "admin"));
    }

    [HttpGet]
    [Authorize(Policy = "AdminClinica")]
    public async Task<ActionResult<OrganizacaoDto>> Obter(CancellationToken cancellationToken)
    {
        var item = await sender.Send(new ObterRegistroConsulta<Organizacao>(tenant.TenantId), cancellationToken);
        return item is null ? NotFound() : Ok(OrganizacaoDto.De(item));
    }

    [HttpGet("equipes")]
    [Authorize(Policy = "AdminClinica")]
    public async Task<ActionResult<IReadOnlyList<EquipeDto>>> ListarEquipes(CancellationToken cancellationToken) =>
        Ok((await sender.Send(new ListarRegistrosConsulta<Equipe>(), cancellationToken)).Select(EquipeDto.De).ToArray());

    [HttpGet("equipes/{id:guid}")]
    [Authorize(Policy = "AdminClinica")]
    public async Task<ActionResult<EquipeDto>> ObterEquipe(Guid id, CancellationToken cancellationToken)
    {
        var item = await sender.Send(new ObterRegistroConsulta<Equipe>(id), cancellationToken);
        return item is null ? NotFound() : Ok(EquipeDto.De(item));
    }

    [HttpPost("equipes")]
    [Authorize(Policy = "AdminClinica")]
    public async Task<ActionResult<RecursoCriadoDto>> CriarEquipe(EquipeRequest dto, CancellationToken cancellationToken)
    {
        var id = await sender.Send(new RegistrarRegistroComando<Equipe>(new Equipe(tenant.TenantId, dto.Nome, dto.Descricao)), cancellationToken);
        return CreatedAtAction(nameof(ObterEquipe), new { id }, new RecursoCriadoDto(id));
    }

    [HttpGet("usuarios")]
    [Authorize(Policy = "AdminClinica")]
    public async Task<ActionResult<IReadOnlyList<UsuarioDto>>> ListarUsuarios(CancellationToken cancellationToken) =>
        Ok((await sender.Send(new ListarRegistrosConsulta<Usuario>(), cancellationToken)).Select(UsuarioDto.De).ToArray());

    [HttpGet("usuarios/{id:guid}")]
    [Authorize(Policy = "AdminClinica")]
    public async Task<ActionResult<UsuarioDto>> ObterUsuario(Guid id, CancellationToken cancellationToken)
    {
        var item = await sender.Send(new ObterRegistroConsulta<Usuario>(id), cancellationToken);
        return item is null ? NotFound() : Ok(UsuarioDto.De(item));
    }

    [HttpPost("usuarios")]
    [Authorize(Policy = "AdminClinica")]
    public async Task<ActionResult<RecursoCriadoDto>> CriarUsuario(UsuarioRequest dto, CancellationToken cancellationToken)
    {
        var usuario = new Usuario(tenant.TenantId, dto.EquipeId, dto.Nome, dto.Sobrenome, dto.Email,
            dto.Telefone, dto.Cargo, perfil: dto.Perfil);
        var id = await sender.Send(new RegistrarRegistroComando<Usuario>(usuario), cancellationToken);
        return CreatedAtAction(nameof(ObterUsuario), new { id }, new RecursoCriadoDto(id));
    }

    [HttpGet("convites")]
    [Authorize(Policy = "AdminClinica")]
    public async Task<ActionResult<IReadOnlyList<ConviteDto>>> ListarConvites(CancellationToken cancellationToken) =>
        Ok((await sender.Send(new ListarRegistrosConsulta<ConviteUsuario>(), cancellationToken)).Select(ConviteDto.De).ToArray());

    [HttpPost("convites")]
    [Authorize(Policy = "AdminClinica")]
    public async Task<ActionResult<ConviteCriadoDto>> CriarConvite(ConviteRequest dto, CancellationToken cancellationToken)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        var criadoEm = relogio.GetUtcNow();
        var autor = Guid.TryParse(User.FindFirst("uid")?.Value, out var usuarioId) ? usuarioId : (Guid?)null;
        var convite = new ConviteUsuario(tenant.TenantId, dto.UsuarioId, hash, criadoEm, dto.ExpiraEm, autor);
        var id = await sender.Send(new RegistrarRegistroComando<ConviteUsuario>(convite), cancellationToken);
        return CreatedAtAction(nameof(ListarConvites), null, new ConviteCriadoDto(id, token, convite.ExpiraEm));
    }

    private void DefinirCookieSessao(string token) => Response.Cookies.Append("pata_session", token,
        new CookieOptions
        {
            HttpOnly = true,
            Secure = !ambiente.IsDevelopment(),
            SameSite = SameSiteMode.Lax,
            IsEssential = true,
            Path = "/",
            MaxAge = ServicoSessaoPata.DuracaoSessao,
            Expires = relogio.GetUtcNow().Add(ServicoSessaoPata.DuracaoSessao)
        });
}

public sealed record ConviteCriadoDto(Guid Id, string Token, DateTimeOffset ExpiraEm);
public sealed record CriarOrganizacaoResposta(OrganizacaoDto Organizacao, Guid TenantId, string Papel);
