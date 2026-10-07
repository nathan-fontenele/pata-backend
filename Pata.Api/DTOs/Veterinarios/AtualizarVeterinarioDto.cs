using System.ComponentModel.DataAnnotations;

namespace Pata.Api.DTOs.Veterinarios;

public sealed record AtualizarVeterinarioDto(
    [property: Required] string Nome,
    [property: Required, EmailAddress] string Email,
    [property: Required] string Telefone,
    [property: Required] string Especialidade);
