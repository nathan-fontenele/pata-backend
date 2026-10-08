using Pata.Application.Comum.Mensagens;
using Pata.Application.Funcionalidades.Animal.Comandos.CadastrarPet;

namespace Pata.Application.Funcionalidades.Animal.Portal;

public sealed class ConsultarPortalTutorManipulador(IConsultaPortalTutor consulta)
    : IManipuladorConsulta<ConsultarPortalTutorConsulta, PortalTutorDto?>
{
    public Task<PortalTutorDto?> Handle(ConsultarPortalTutorConsulta request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            return Task.FromResult<PortalTutorDto?>(null);
        return consulta.ConsultarAsync(CadastrarPetManipulador.CalcularHash(request.Token), cancellationToken);
    }
}
