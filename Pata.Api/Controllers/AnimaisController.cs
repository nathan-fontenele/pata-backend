using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pata.Api.DTOs.Animais;
using Pata.Application.Comum.Recursos;
using Pata.Application.Funcionalidades.Animal.Comandos.CadastrarPet;
using Pata.Domain.Entidades.Animal;

namespace Pata.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/animais")]
[Produces("application/json")]
public sealed class AnimaisController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AnimalDto>>> Listar(CancellationToken cancellationToken)
    {
        var animais = await sender.Send(new ListarRegistrosConsulta<Animal>(), cancellationToken);
        return Ok(animais.Select(De).ToArray());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AnimalDto>> Obter(Guid id, CancellationToken cancellationToken)
    {
        var animal = await sender.Send(new ObterRegistroConsulta<Animal>(id), cancellationToken);
        return animal is null ? NotFound() : Ok(De(animal));
    }

    [HttpPost]
    public async Task<ActionResult<AnimalCadastradoDto>> Cadastrar(CadastrarAnimalDto dto,
        CancellationToken cancellationToken)
    {
        var resultado = await sender.Send(new CadastrarPetComando(dto.TutorNome, dto.TutorCpf, dto.TutorEmail,
            dto.TutorTelefone, dto.Nome, dto.Especie, dto.Raca, dto.DataNascimento), cancellationToken);
        var link = resultado.TokenAcesso is null
            ? null
            : Url.Link(nameof(PortalTutoresController.Obter), new { token = resultado.TokenAcesso });
        var resposta = new AnimalCadastradoDto(resultado.AnimalId, resultado.TutorId, resultado.Nome,
            resultado.NovoTutor, link);
        return CreatedAtAction(nameof(Obter), new { id = resultado.AnimalId }, resposta);
    }

    private static AnimalDto De(Animal animal) =>
        new(animal.Id, animal.TutorId, animal.Nome, animal.Especie.ToString(), animal.Raca, animal.DataNascimento);
}
