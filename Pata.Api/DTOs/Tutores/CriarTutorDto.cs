using System.ComponentModel.DataAnnotations;

namespace Pata.Api.DTOs.Tutores;

public sealed record CriarTutorDto(
    [param: Required] string Nome,
    [param: Required] string Cpf,
    [param: Required, EmailAddress] string Email,
    [param: Required] string Telefone);
