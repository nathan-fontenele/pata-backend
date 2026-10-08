using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pata.Api.Controllers;
using Pata.Api.DTOs.Animais;
using Pata.Application.Funcionalidades.Animal.Portal;

namespace Pata.Api.Tests.Contratos;

public sealed class PortalTutorContratoTests
{
    [Fact]
    public void PortalDeveSerPublicoEUsarRotaComToken()
    {
        var tipo = typeof(PortalTutoresController);

        Assert.NotNull(tipo.GetCustomAttribute<AllowAnonymousAttribute>());
        Assert.Equal("api/portal/tutores", tipo.GetCustomAttribute<RouteAttribute>()?.Template);
        Assert.NotNull(tipo.GetMethod(nameof(PortalTutoresController.Obter))?
            .GetCustomAttribute<HttpGetAttribute>());
    }

    [Fact]
    public void CadastroDeAnimalDeveExigirAutenticacaoDaClinica()
    {
        var tipo = typeof(AnimaisController);

        Assert.NotNull(tipo.GetCustomAttribute<AuthorizeAttribute>());
        Assert.Equal("api/animais", tipo.GetCustomAttribute<RouteAttribute>()?.Template);
    }

    [Fact]
    public void RespostaDoPortalNaoDeveExporDadosInternosOuCredenciais()
    {
        var contrato = new PortalTutorDto("Maria", [
            new PetAcompanhamentoDto(Guid.NewGuid(), "Mel", "Cachorro", "Vira-lata", "Agendada", null)
        ]);

        var json = JsonSerializer.Serialize(contrato);

        Assert.Contains("Maria", json);
        Assert.Contains("Mel", json);
        Assert.DoesNotContain("tenant", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("token", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("cpf", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CadastroDePetNaoDeveAceitarTenantInformadoPeloCliente()
    {
        var propriedades = typeof(CadastrarAnimalDto).GetProperties().Select(propriedade => propriedade.Name);

        Assert.DoesNotContain(propriedades, propriedade => propriedade.Contains("Tenant", StringComparison.OrdinalIgnoreCase));
    }
}
