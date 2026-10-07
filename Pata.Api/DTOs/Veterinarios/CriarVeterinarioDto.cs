using System.ComponentModel.DataAnnotations;

namespace Pata.Api.DTOs.Veterinarios;

public sealed record CriarVeterinarioDto(
    [property: Required] string Nome,
    [property: Required, EmailAddress] string Email,
    [property: Required] string Telefone,
    [property: Required] string NumeroCrmv,
    [property: Required] string UfCrmv,
    [property: Required] string Especialidade);
