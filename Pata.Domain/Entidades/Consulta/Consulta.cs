using Pata.Domain.Comum;
using Pata.Domain.Entidades.Prontuario;
using Pata.Domain.Excecoes;
using ProntuarioClinico = Pata.Domain.Entidades.Prontuario.Prontuario;

namespace Pata.Domain.Entidades.Consulta;

public sealed class Consulta : RaizAgregadaAuditavel<Guid>
{
    private Consulta()
    {
    }

    public Consulta(
        Guid animalId,
        Guid veterinarioId,
        Guid tutorId,
        DateTimeOffset dataHora,
        DateTimeOffset dataAtual) : base(Guid.NewGuid()) =>
        Inicializar(animalId, veterinarioId, tutorId, dataHora, dataAtual);

    public Consulta(
        Guid tenantId,
        Guid animalId,
        Guid veterinarioId,
        Guid tutorId,
        DateTimeOffset dataHora,
        DateTimeOffset dataAtual) : base(Guid.NewGuid(), tenantId) =>
        Inicializar(animalId, veterinarioId, tutorId, dataHora, dataAtual);

    private void Inicializar(
        Guid animalId,
        Guid veterinarioId,
        Guid tutorId,
        DateTimeOffset dataHora,
        DateTimeOffset dataAtual)
    {
        ValidarIdentificador(animalId, nameof(animalId), "Animal");
        ValidarIdentificador(veterinarioId, nameof(veterinarioId), "Veterinario");
        ValidarIdentificador(tutorId, nameof(tutorId), "Tutor");
        ValidarDataFutura(dataHora, dataAtual, nameof(dataHora));

        AnimalId = animalId;
        VeterinarioId = veterinarioId;
        TutorId = tutorId;
        DataHora = dataHora;
        Status = StatusConsulta.Agendada;
    }

    public Guid AnimalId { get; private set; }
    public Guid VeterinarioId { get; private set; }
    public Guid TutorId { get; private set; }
    public DateTimeOffset DataHora { get; private set; }
    public DateTimeOffset? ConfirmadaEm { get; private set; }
    public StatusConsulta Status { get; private set; }
    public ProntuarioClinico? Prontuario { get; private set; }

    public void Confirmar(DateTimeOffset confirmadaEm)
    {
        GarantirStatus(StatusConsulta.Agendada, "confirmada");

        if (confirmadaEm == default)
            throw new ErroDeValidacao("A data de confirmacao e obrigatoria.", nameof(confirmadaEm));

        Status = StatusConsulta.Confirmada;
        ConfirmadaEm = confirmadaEm;
    }

    public void Cancelar()
    {
        GarantirConsultaEmAberto("cancelada");
        Status = StatusConsulta.Cancelada;
    }

    public void Finalizar(
        IEnumerable<Sintoma> sintomas,
        string? diagnostico,
        string? prescricao,
        DateTimeOffset dataRegistro)
    {
        GarantirPodeFinalizar();

        if (Prontuario is not null)
            throw new RegraDeNegocioException("A consulta ja possui um prontuario.");

        var prontuario = new ProntuarioClinico(
            Guid.NewGuid(),
            Id,
            sintomas,
            diagnostico,
            prescricao,
            dataRegistro);

        Prontuario = prontuario;
        Status = StatusConsulta.Finalizada;
    }

    public void Realizar(
        IEnumerable<Sintoma> sintomas,
        string? diagnostico,
        string? prescricao,
        DateTimeOffset dataRegistro) =>
        Finalizar(sintomas, diagnostico, prescricao, dataRegistro);

    public void Remarcar(DateTimeOffset novaDataHora, DateTimeOffset dataAtual)
    {
        GarantirConsultaEmAberto("remarcada");
        ValidarDataFutura(novaDataHora, dataAtual, nameof(novaDataHora));

        DataHora = novaDataHora;
        Status = StatusConsulta.Agendada;
        ConfirmadaEm = null;
    }

    public void SubstituirVeterinario(Guid veterinarioId)
    {
        GarantirConsultaEmAberto("alterada");
        ValidarIdentificador(veterinarioId, nameof(veterinarioId), "Veterinario");

        VeterinarioId = veterinarioId;
    }

    private void GarantirConsultaEmAberto(string operacao)
    {
        if (Status is StatusConsulta.Cancelada or StatusConsulta.Finalizada)
            throw new RegraDeNegocioException(
                $"Uma consulta {Status.ToString().ToLowerInvariant()} nao pode ser {operacao}.");
    }

    private void GarantirStatus(StatusConsulta statusEsperado, string operacao)
    {
        if (Status != statusEsperado)
            throw new RegraDeNegocioException(
                $"A consulta deve estar {statusEsperado.ToString().ToLowerInvariant()} para ser {operacao}.");
    }

    private void GarantirPodeFinalizar()
    {
        if (Status is not (StatusConsulta.Agendada or StatusConsulta.Confirmada))
            throw new RegraDeNegocioException(
                "Apenas consultas agendadas ou confirmadas podem ser finalizadas.");
    }

    private static void ValidarIdentificador(Guid id, string nomeParametro, string entidade)
    {
        if (id == Guid.Empty)
            throw new ErroDeValidacao($"{entidade} e obrigatorio.", nomeParametro);
    }

    private static void ValidarDataFutura(
        DateTimeOffset dataHora,
        DateTimeOffset dataAtual,
        string nomeParametro)
    {
        if (dataHora <= dataAtual)
            throw new ErroDeValidacao(
                "A data e hora da consulta devem ser posteriores a data atual.",
                nomeParametro);
    }
}
