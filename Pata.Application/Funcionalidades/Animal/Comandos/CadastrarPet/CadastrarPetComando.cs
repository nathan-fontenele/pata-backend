using Pata.Application.Comum.Mensagens;

namespace Pata.Application.Funcionalidades.Animal.Comandos.CadastrarPet;

public sealed record CadastrarPetComando(
    string TutorNome,
    string TutorCpf,
    string TutorEmail,
    string TutorTelefone,
    string Nome,
    Pata.Domain.Entidades.Animal.Enum.Especie Especie,
    string Raca,
    DateOnly DataNascimento) : IComando<PetCadastrado>;

public sealed record PetCadastrado(Guid AnimalId, Guid TutorId, string TutorNome, string Nome,
    string? TokenAcesso, bool NovoTutor);
