namespace Pata.Application.Funcionalidades.Animal.Portal;

public sealed record PetAcompanhamentoDto(Guid Id, string Nome, string Especie, string Raca,
    string Status, DateTimeOffset? DataStatus);

public sealed record PortalTutorDto(string Nome, IReadOnlyList<PetAcompanhamentoDto> Pets);

public interface IConsultaPortalTutor
{
    Task<PortalTutorDto?> ConsultarAsync(string tokenHash, CancellationToken cancellationToken);
}
