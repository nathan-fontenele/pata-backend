using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Pata.Application.Comum.Abstracoes;
using Pata.Domain.Comum;
using Pata.Domain.Entidades.Animal;
using Pata.Domain.Entidades.Tutor;
using Pata.Infrastructure.Persistencia;

namespace Pata.Infrastructure.Tests.Persistencia;

public sealed class ModeloTenantTests
{
    private static readonly Guid TenantId = Guid.NewGuid();

    [Fact]
    public void DeveAplicarFiltroGlobalEmTodasAsEntidadesTenant()
    {
        using var contexto = CriarContexto();

        var entidadesTenant = contexto.Model.GetEntityTypes()
            .Where(tipo => typeof(ITenantEntity).IsAssignableFrom(tipo.ClrType));

        Assert.NotEmpty(entidadesTenant);
        Assert.All(entidadesTenant, tipo => Assert.NotNull(tipo.GetQueryFilter()));
    }

    [Fact]
    public void DeveMapearTenantIdParaColunaPadrao()
    {
        using var contexto = CriarContexto();

        var entidadeTutor = contexto.Model.FindEntityType(typeof(Tutor))!;
        var tabelaTutor = StoreObjectIdentifier.Table(entidadeTutor.GetTableName()!, entidadeTutor.GetSchema());
        var entidadeAnimal = contexto.Model.FindEntityType(typeof(Animal))!;
        var tabelaAnimal = StoreObjectIdentifier.Table(entidadeAnimal.GetTableName()!, entidadeAnimal.GetSchema());

        Assert.Equal("tenant_id", entidadeTutor.FindProperty(nameof(ITenantEntity.TenantId))!.GetColumnName(tabelaTutor));
        Assert.Equal("tenant_id", entidadeAnimal.FindProperty(nameof(ITenantEntity.TenantId))!.GetColumnName(tabelaAnimal));
    }

    [Theory]
    [InlineData(typeof(Animal), typeof(Tutor))]
    [InlineData(typeof(AcessoPortalTutor), typeof(Tutor))]
    public void DeveIncluirTenantNasChavesEstrangeirasEntreEntidadesClinicas(Type dependente, Type principal)
    {
        using var contexto = CriarContexto();
        var entidade = contexto.Model.FindEntityType(dependente)!;
        var chave = Assert.Single(entidade.GetForeignKeys()
            .Where(foreignKey => foreignKey.PrincipalEntityType.ClrType == principal));

        Assert.Contains(chave.Properties, property => property.Name == nameof(ITenantEntity.TenantId));
        Assert.Contains(chave.PrincipalKey.Properties, property => property.Name == nameof(ITenantEntity.TenantId));
    }

    [Fact]
    public void PlanoGlobalNaoDeveTerTenantId()
    {
        using var contexto = CriarContexto();

        var plano = contexto.Model.FindEntityType(typeof(Pata.Domain.Entidades.Cobranca.Plano))!;

        Assert.Null(plano.FindProperty(nameof(ITenantEntity.TenantId)));
        Assert.Null(plano.GetQueryFilter());
    }

    private static PataDbContext CriarContexto()
    {
        var options = new DbContextOptionsBuilder<PataDbContext>()
            .UseNpgsql("Host=localhost;Database=pata_model_tests;Username=pata;Password=pata")
            .Options;
        return new PataDbContext(options, TimeProvider.System, new TenantFalso(TenantId), new UsuarioFalso());
    }

    private sealed class TenantFalso(Guid id) : IContextoTenant
    {
        public Guid TenantId { get; } = id;
    }

    private sealed class UsuarioFalso : IUsuarioAtual
    {
        public string Identificador => "infra-tests";
        public string Perfil => "Teste";
        public IReadOnlyCollection<string> Papeis => ["admin"];
    }
}
