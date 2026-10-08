using Pata.Application.Comum.Mensagens;

namespace Pata.Application.Funcionalidades.Animal.Portal;

public sealed record ConsultarPortalTutorConsulta(string Token) : IConsulta<PortalTutorDto?>;
