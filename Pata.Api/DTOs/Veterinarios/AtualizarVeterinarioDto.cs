using System.ComponentModel.DataAnnotations;

namespace Pata.Api.DTOs.Veterinarios;

public sealed record AtualizarVeterinarioDto(
    [param: Required] string Nome,
    [param: Required, EmailAddress] string Email,
    [param: Required] string Telefone,
    [param: Required] string Especialidade);
