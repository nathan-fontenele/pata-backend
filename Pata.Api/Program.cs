using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using Pata.Application;
using Pata.Application.Comum.Abstracoes;
using Pata.Api.Servicos;
using Pata.Domain.Excecoes;
using Pata.Infrastructure;
using Pata.Infrastructure.Persistencia;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplication(typeof(PataDbContext).Assembly);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ContextoTenantHttp>();
builder.Services.AddScoped<IContextoTenant>(services => services.GetRequiredService<ContextoTenantHttp>());
builder.Services.AddScoped<IUsuarioAtual>(services => services.GetRequiredService<ContextoTenantHttp>());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "pata-auth";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "pata-api";
var jwtSigningKey = builder.Configuration["Jwt:SigningKey"];
if (string.IsNullOrWhiteSpace(jwtSigningKey) || Encoding.UTF8.GetByteCount(jwtSigningKey) < 32)
    throw new InvalidOperationException("Configure Jwt:SigningKey com pelo menos 32 bytes.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey)),
            ValidateLifetime = true,
            NameClaimType = "sub",
            RoleClaimType = "role",
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Pata API",
        Version = "v1",
        Description = "API de gerenciamento de tutores e veterinarios."
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe o token JWT emitido pelo provedor de identidade."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Pata API v1"));
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (ErroDeValidacao exception)
    {
        await Results.ValidationProblem(new Dictionary<string, string[]> { [exception.ParamName ?? "request"] = [exception.Message] })
            .ExecuteAsync(context);
    }
    catch (UnauthorizedAccessException)
    {
        await Results.Unauthorized().ExecuteAsync(context);
    }
    catch (ConflitoException exception)
    {
        await Results.Conflict(new { erro = exception.Message }).ExecuteAsync(context);
    }
    catch (RecursoNaoEncontradoException exception)
    {
        await Results.NotFound(new { erro = exception.Message }).ExecuteAsync(context);
    }
    catch (RegraDeNegocioException exception)
    {
        await Results.BadRequest(new { erro = exception.Message }).ExecuteAsync(context);
    }
});

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapControllers();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PataDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.Run();
