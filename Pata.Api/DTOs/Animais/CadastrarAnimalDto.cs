using System.ComponentModel.DataAnnotations;
using Pata.Domain.Entidades.Animal.Enum;

namespace Pata.Api.DTOs.Animais;

public sealed record CadastrarAnimalDto(
    [property: Required] string TutorNome,
    [property: Required] string TutorCpf,
    [property: Required, EmailAddress] string TutorEmail,
    [property: Required] string TutorTelefone,
    [property: Required] string Nome,
    Especie Especie,
    [property: Required] string Raca,
    DateOnly DataNascimento);

public sealed record AnimalDto(Guid Id, Guid TutorId, string Nome, string Especie, string Raca, DateOnly DataNascimento);

public sealed record AnimalCadastradoDto(Guid Id, Guid TutorId, string Nome, bool TutorCriado, string? LinkAcompanhamento);
