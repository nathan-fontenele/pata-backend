using System.ComponentModel.DataAnnotations;
using Pata.Domain.Entidades.Animal.Enum;

namespace Pata.Api.DTOs.Animais;

public sealed record CadastrarAnimalDto(
    [param: Required] string TutorNome,
    [param: Required] string TutorCpf,
    [param: Required, EmailAddress] string TutorEmail,
    [param: Required] string TutorTelefone,
    [param: Required] string Nome,
    Especie Especie,
    [param: Required] string Raca,
    DateOnly DataNascimento);

public sealed record AnimalDto(Guid Id, Guid TutorId, string Nome, string Especie, string Raca, DateOnly DataNascimento);

public sealed record AnimalCadastradoDto(Guid Id, Guid TutorId, string Nome, bool TutorCriado, string? LinkAcompanhamento);
