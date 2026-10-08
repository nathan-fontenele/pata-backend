using Pata.Application.Comum.Mensagens;

namespace Pata.Application.Funcionalidades.Animal.Comandos.RevogarLinkTutor;

public sealed record RevogarLinkTutorComando(Guid TutorId) : IComando;
