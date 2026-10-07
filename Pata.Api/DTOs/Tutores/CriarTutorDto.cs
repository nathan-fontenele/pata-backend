using System.ComponentModel.DataAnnotations;

namespace Pata.Api.DTOs.Tutores;

public sealed record CriarTutorDto(
    [property: Required] string Nome,
    [property: Required] string Cpf,
    [property: Required, EmailAddress] string Email,
    [property: Required] string Telefone);
