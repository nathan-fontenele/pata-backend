using System.ComponentModel.DataAnnotations;

namespace Pata.Api.DTOs.Tutores;

public sealed record AtualizarTutorDto(
    [property: Required] string Nome,
    [property: Required, EmailAddress] string Email,
    [property: Required] string Telefone);
