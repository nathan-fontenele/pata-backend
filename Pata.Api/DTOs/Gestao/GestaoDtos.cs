using Pata.Domain.Entidades.Auditoria;
using Pata.Domain.Entidades.Cobranca;
using Pata.Domain.Entidades.Organizacao;
using System.ComponentModel.DataAnnotations;

namespace Pata.Api.DTOs.Gestao;

public sealed record OrganizacaoDto(Guid Id, string Nome, string Slug, string Status, string Cnpj, string Email, string? LogoUrl)
{
    public static OrganizacaoDto De(Organizacao item) => new(item.Id, item.Nome, item.Slug, item.Status.ToString(), item.Cnpj, item.Email, item.LogoUrl);
}
public sealed record CriarOrganizacaoDto(
    [param: Required] string Nome,
    [param: Required] string Slug,
    [param: Required] string Cnpj,
    [param: Required, EmailAddress] string Email,
    string? LogoUrl);
public sealed record EquipeRequest(string Nome, string? Descricao);
public sealed record EquipeDto(Guid Id, string Nome, string? Descricao, string Status)
{
    public static EquipeDto De(Equipe item) => new(item.Id, item.Nome, item.Descricao, item.Status.ToString());
}
public sealed record UsuarioRequest(Guid EquipeId, string Nome, string Sobrenome, string Email, string? Telefone,
    string? Cargo, PerfilUsuario Perfil = PerfilUsuario.Usuario);
public sealed record UsuarioDto(Guid Id, Guid EquipeId, string Nome, string Sobrenome, string Email, string? Telefone,
    string? Cargo, DateTimeOffset? UltimoAcessoEm, string Status, string Perfil)
{
    public static UsuarioDto De(Usuario item) => new(item.Id, item.EquipeId, item.Nome, item.Sobrenome, item.Email,
        item.Telefone, item.Cargo, item.UltimoAcessoEm, item.Status.ToString(), item.Perfil.ToString());
}
public sealed record ConviteRequest(Guid UsuarioId, DateTimeOffset ExpiraEm);
public sealed record ConviteDto(Guid Id, Guid UsuarioId, DateTimeOffset CriadoEm, DateTimeOffset ExpiraEm,
    DateTimeOffset? AceitoEm, DateTimeOffset? RevogadoEm, Guid? ConvidadoPorUsuarioId)
{
    public static ConviteDto De(ConviteUsuario item) => new(item.Id, item.UsuarioId, item.CriadoEm, item.ExpiraEm,
        item.AceitoEm, item.RevogadoEm, item.ConvidadoPorUsuarioId);
}
public sealed record PlanoRequest(string Codigo, string Nome, string? Descricao);
public sealed record PlanoDto(Guid Id, string Codigo, string Nome, string? Descricao, string Status)
{
    public static PlanoDto De(Plano item) => new(item.Id, item.Codigo, item.Nome, item.Descricao, item.Status.ToString());
}
public sealed record PrecoPlanoRequest(string Codigo, decimal Valor, string Moeda, int DiasTeste,
    IntervaloUnidade IntervaloUnidade, int IntervaloQuantidade);
public sealed record PrecoPlanoDto(Guid Id, Guid PlanoId, string Codigo, decimal Valor, string Moeda, int DiasTeste,
    string IntervaloUnidade, int IntervaloQuantidade, string Status)
{
    public static PrecoPlanoDto De(PrecoPlano item) => new(item.Id, item.PlanoId, item.Codigo, item.Valor, item.Moeda,
        item.DiasTeste, item.IntervaloUnidade.ToString(), item.IntervaloQuantidade, item.Status.ToString());
}
public sealed record AssinaturaRequest(Guid PrecoPlanoId, DateTimeOffset InicioEm,
    DateTimeOffset? TesteInicio, DateTimeOffset? TesteFim);
public sealed record AssinaturaDto(Guid Id, Guid OrganizacaoId, Guid PrecoPlanoId, string Status,
    DateTimeOffset InicioEm, DateTimeOffset? PeriodoAtualInicio, DateTimeOffset? PeriodoAtualFim,
    DateTimeOffset? TesteInicio, DateTimeOffset? TesteFim, DateTimeOffset? CancelarEm,
    DateTimeOffset? CancelamentoSolicitadoEm, DateTimeOffset? EncerradoEm)
{
    public static AssinaturaDto De(Assinatura item) => new(item.Id, item.OrganizacaoId, item.PrecoPlanoId,
        item.Status.ToString(), item.InicioEm, item.PeriodoAtualInicio, item.PeriodoAtualFim, item.TesteInicio,
        item.TesteFim, item.CancelarEm, item.CancelamentoSolicitadoEm, item.EncerradoEm);
}
public sealed record FaturaRequest(Guid AssinaturaId, string Numero, string Moeda,
    DateTimeOffset EmitidaEm, DateTimeOffset VencimentoEm);
public sealed record ItemFaturaRequest(Guid? PrecoPlanoId, string Descricao, decimal Quantidade,
    decimal ValorUnitario, decimal Desconto, DateTimeOffset? PeriodoInicio, DateTimeOffset? PeriodoFim);
public sealed record ItemFaturaDto(Guid Id, Guid? PrecoPlanoId, string Descricao, decimal Quantidade,
    decimal ValorUnitario, decimal Desconto, decimal Total, DateTimeOffset? PeriodoInicio, DateTimeOffset? PeriodoFim)
{
    public static ItemFaturaDto De(ItemFatura item) => new(item.Id, item.PrecoPlanoId, item.Descricao,
        item.Quantidade, item.ValorUnitario, item.Desconto, item.Total, item.PeriodoInicio, item.PeriodoFim);
}
public sealed record FaturaDto(Guid Id, Guid AssinaturaId, string Numero, string Status, string Moeda,
    decimal Subtotal, decimal DescontoTotal, decimal Total, DateTimeOffset EmitidaEm,
    DateTimeOffset VencimentoEm, DateTimeOffset? QuitadaEm, IReadOnlyList<ItemFaturaDto> Itens)
{
    public static FaturaDto De(Fatura item) => new(item.Id, item.AssinaturaId, item.Numero, item.Status.ToString(),
        item.Moeda, item.Subtotal, item.DescontoTotal, item.Total, item.EmitidaEm, item.VencimentoEm,
        item.QuitadaEm, item.Itens.Select(ItemFaturaDto.De).ToArray());
}
public sealed record PagamentoRequest(decimal Valor, string Moeda, string Metodo, string Provedor, string ChaveIdempotencia);
public sealed record PagamentoDto(Guid Id, Guid FaturaId, string Status, decimal Valor, string Moeda,
    string Metodo, string Provedor, string? ReferenciaExterna, DateTimeOffset? ConfirmadaEm,
    DateTimeOffset? FalhouEm, string? CodigoFalha, string? MensagemFalha)
{
    public static PagamentoDto De(Pagamento item) => new(item.Id, item.FaturaId, item.Status.ToString(), item.Valor,
        item.Moeda, item.Metodo, item.Provedor, item.ReferenciaExterna, item.ConfirmadaEm, item.FalhouEm,
        item.CodigoFalha, item.MensagemFalha);
}
public sealed record AuditoriaDto(Guid Id, Guid EntidadeId, string EntidadeTipo, string Acao,
    DateTimeOffset CriadoEm, string CriadoPor, DateTimeOffset? EditadoEm, string? EditadoPor,
    DateTimeOffset? ExcluidoEm, string? ExcluidoPor, string? EntidadeAntes, string? EntidadeDepois)
{
    public static AuditoriaDto De(Auditoria item) => new(item.Id, item.EntidadeId, item.EntidadeTipo,
        item.Acao.ToString(), item.CriadoEm, item.CriadoPor, item.EditadoEm, item.EditadoPor,
        item.ExcluidoEm, item.ExcluidoPor, item.EntidadeAntes, item.EntidadeDepois);
}
