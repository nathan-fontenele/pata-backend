using System.ComponentModel.DataAnnotations;

namespace Pata.Api.DTOs.Veterinarios;

public sealed record CriarVeterinarioDto(
    [param: Required] string Nome,
    [param: Required, EmailAddress] string Email,
    [param: Required] string Telefone,
    [param: Required] string NumeroCrmv,
    [param: Required] string UfCrmv,
    [param: Required] string Especialidade);
