using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using System.Threading.RateLimiting;
using Pata.Application;
using Pata.Api.Autorizacao;
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
builder.Services.AddScoped<IContextoTenantDefinivel>(services => services.GetRequiredService<ContextoTenantHttp>());
builder.Services.AddScoped<IUsuarioAtual>(services => services.GetRequiredService<ContextoTenantHttp>());
builder.Services.AddScoped<ServicoSessaoPata>();
builder.Services.AddScoped<IAuthorizationHandler, AdminClinicaHandler>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "pata-auth";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "pata-api";
var jwtSigningKey = builder.Configuration["Jwt:SigningKey"];
var auth0Authority = builder.Configuration["Auth0:Authority"];
var auth0Audience = builder.Configuration["Auth0:Audience"];
if (string.IsNullOrWhiteSpace(jwtSigningKey) || Encoding.UTF8.GetByteCount(jwtSigningKey) < 32)
    throw new InvalidOperationException("Configure Jwt:SigningKey com pelo menos 32 bytes.");
if (string.IsNullOrWhiteSpace(auth0Authority) || string.IsNullOrWhiteSpace(auth0Audience))
    throw new InvalidOperationException("Configure Auth0:Authority e Auth0:Audience.");

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = "PataSession";
        options.DefaultChallengeScheme = "PataSession";
    })
    .AddJwtBearer("PataSession", options =>
    {
        options.MapInboundClaims = false;
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (string.IsNullOrEmpty(context.Token)
                    && context.Request.Cookies.TryGetValue("pata_session", out var cookieToken))
                    context.Token = cookieToken;
                return Task.CompletedTask;
            }
        };
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
    })
    .AddJwtBearer("Auth0AccessToken", options =>
    {
        options.Authority = auth0Authority;
        options.Audience = auth0Audience;
        options.RequireHttpsMetadata = true;
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = auth0Authority,
            ValidateAudience = true,
            ValidAudience = auth0Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            NameClaimType = "sub",
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminClinica", policy =>
        policy.RequireAuthenticatedUser().AddRequirements(new AdminClinicaRequisito()));
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("cadastro-organizacao", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "ip-desconhecido",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(15),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});
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
        Description = "Use o access token Auth0 no cadastro/seleção de clínica e a sessão Pata nas demais rotas."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});

var app = builder.Build();

app.UseRouting();
app.UseRateLimiter();
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
    await db.Database.MigrateAsync();
}

app.Run();
