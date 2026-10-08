using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Pata.Domain.Entidades.Organizacao;

namespace Pata.Api.Servicos;

public sealed class ServicoSessaoPata(IConfiguration configuration, TimeProvider timeProvider)
{
    public static readonly TimeSpan DuracaoSessao = TimeSpan.FromHours(1);

    public string CriarToken(string auth0Sub, AcessoOrganizacao acesso)
    {
        var agora = timeProvider.GetUtcNow();
        var signingKey = configuration["Jwt:SigningKey"]
            ?? throw new InvalidOperationException("Jwt:SigningKey nao configurada.");
        var credenciais = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"] ?? "pata-auth",
            audience: configuration["Jwt:Audience"] ?? "pata-api",
            claims:
            [
                new Claim("sub", auth0Sub),
                new Claim("tenant_id", acesso.TenantId.ToString()),
                new Claim("role", acesso.Perfil == PerfilUsuario.Admin ? "admin" : "usuario"),
                new Claim("uid", acesso.UsuarioId.ToString())
            ],
            notBefore: agora.UtcDateTime,
            expires: agora.Add(DuracaoSessao).UtcDateTime,
            signingCredentials: credenciais);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
