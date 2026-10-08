using System.ComponentModel.DataAnnotations;

namespace Pata.Api.DTOs.Tutores;

public sealed record AtualizarTutorDto(
    [param: Required] string Nome,
    [param: Required, EmailAddress] string Email,
    [param: Required] string Telefone);
