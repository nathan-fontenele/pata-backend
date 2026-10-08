using System.Security.Cryptography;
using System.Text;
using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pata.Api.Servicos;
using Pata.Application.Funcionalidades.Organizacao.Comandos.AceitarConviteUsuario;

namespace Pata.Api.Controllers;

[ApiController]
[Route("api/convites")]
[Produces("application/json")]
public sealed class ConvitesController(
    ISender sender,
    ServicoSessaoPata sessao,
    IWebHostEnvironment ambiente,
    TimeProvider relogio) : ControllerBase
{
    [HttpPost("aceitar")]
    [Authorize(AuthenticationSchemes = "Auth0AccessToken")]
    public async Task<ActionResult<SessaoCriadaDto>> Aceitar(
        AceitarConviteDto dto,
        CancellationToken cancellationToken)
    {
        var sub = User.FindFirst("sub")?.Value;
        var email = User.FindFirst("email")?.Value;
        var emailVerificado = string.Equals(User.FindFirst("email_verified")?.Value, "true",
            StringComparison.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(sub) || string.IsNullOrWhiteSpace(email) || !emailVerificado)
            return Unauthorized("O access token deve identificar o usuario e trazer um e-mail verificado.");

        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(dto.Token)));
        var acesso = await sender.Send(new AceitarConviteUsuarioComando(tokenHash, sub, email,
            relogio.GetUtcNow()), cancellationToken);
        var tokenSessao = sessao.CriarToken(sub, acesso);
        Response.Cookies.Append("pata_session", tokenSessao, new CookieOptions
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
}

public sealed record AceitarConviteDto([param: Required] string Token);
