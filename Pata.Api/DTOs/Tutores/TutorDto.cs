using Pata.Application.Funcionalidades.Tutor.Consultas;

namespace Pata.Api.DTOs.Tutores;

public sealed record TutorDto(
    Guid Id,
    string Nome,
    string Cpf,
    string Email,
    string Telefone)
{
    public static TutorDto De(TutorResumoDto tutor) =>
        new(tutor.Id, tutor.Nome, tutor.Cpf, tutor.Email, tutor.Telefone);
}
