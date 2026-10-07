using Microsoft.EntityFrameworkCore;
using Pata.Application;
using Pata.Domain.Excecoes;
using Pata.Infrastructure;
using Pata.Infrastructure.Persistencia;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(options =>
    options.SwaggerDoc("v1", new()
    {
        Title = "Pata API",
        Version = "v1",
        Description = "API de gerenciamento de tutores e veterinarios."
    }));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Pata API v1"));

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
