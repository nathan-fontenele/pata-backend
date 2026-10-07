using Pata.Application.Funcionalidades.Veterinario.Consultas;

namespace Pata.Api.DTOs.Veterinarios;

public sealed record VeterinarioDto(
    Guid Id,
    string Nome,
    string Email,
    string Telefone,
    string Crmv,
    string Especialidade)
{
    public static VeterinarioDto De(VeterinarioResumoDto veterinario) =>
        new(veterinario.Id, veterinario.Nome, veterinario.Email, veterinario.Telefone,
            veterinario.Crmv, veterinario.Especialidade);
}
