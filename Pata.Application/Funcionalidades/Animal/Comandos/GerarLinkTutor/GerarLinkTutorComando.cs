using Pata.Application.Comum.Mensagens;

namespace Pata.Application.Funcionalidades.Animal.Comandos.GerarLinkTutor;

public sealed record GerarLinkTutorComando(Guid TutorId) : IComando<string>;
