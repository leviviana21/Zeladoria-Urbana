using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using ZeladoriaUrbana.API.Data;
using ZeladoriaUrbana.API.Models;
namespace ZeladoriaUrbana.API.Controllers;
[Route("api/[controller]")][ApiController]
public class OcorrenciasController : ControllerBase
{
  private static readonly string[] StatusValidos=["Pendente","Em Andamento","Concluído"];
  private readonly AppDbContext _db;
  public OcorrenciasController(AppDbContext db)=>_db=db;
  public record NovaOcorrencia([Required] int CategoriaId,[Required,MinLength(10),MaxLength(1500)] string Descricao,[Required,MaxLength(255)] string Endereco,[Required,MaxLength(150)] string ContatoNome,[EmailAddress,MaxLength(150)] string? ContatoEmail,[MaxLength(30)] string? ContatoTelefone);
  [Authorize(Roles="Administrador,Operador")][HttpGet]
  public async Task<IActionResult> Get()=>Ok(await _db.Ocorrencias.AsNoTracking().Include(x=>x.Categoria).OrderByDescending(x=>x.DataCriacao).ToListAsync());
  [HttpPost]
  public async Task<IActionResult> Post(NovaOcorrencia dto)
  {
    if (!await _db.Categorias.AnyAsync(x=>x.Id==dto.CategoriaId)) return BadRequest(new {mensagem="Categoria inválida."});
    var o=new Ocorrencia{CategoriaId=dto.CategoriaId,Descricao=dto.Descricao.Trim(),Endereco=dto.Endereco.Trim(),ContatoNome=dto.ContatoNome.Trim(),ContatoEmail=dto.ContatoEmail?.Trim(),ContatoTelefone=dto.ContatoTelefone?.Trim(),Situacao="Pendente",DataCriacao=DateTime.UtcNow};
    if (string.IsNullOrWhiteSpace(o.Descricao)||string.IsNullOrWhiteSpace(o.Endereco)||string.IsNullOrWhiteSpace(o.ContatoNome)) return BadRequest(new {mensagem="Preencha os campos obrigatórios."});
    _db.Ocorrencias.Add(o);await _db.SaveChangesAsync();
    return Created($"/api/ocorrencias/{o.Id}",new {o.Id,o.Situacao});
  }
  public record AtualizarStatusRequest(string Situacao);
  [Authorize(Roles="Administrador,Operador")][HttpPatch("{id:int}/status")]
  public async Task<IActionResult> Status(int id,AtualizarStatusRequest request)
  {
    if(request is null||!StatusValidos.Contains(request.Situacao))return BadRequest(new {mensagem="Status inválido."});
    var o=await _db.Ocorrencias.FindAsync(id);if(o is null)return NotFound(new {mensagem="Ocorrência não encontrada."});
    o.Situacao=request.Situacao;await _db.SaveChangesAsync();return Ok(new {o.Id,o.Situacao});
  }
  [Authorize(Roles="Administrador")][HttpDelete("{id:int}")]
  public async Task<IActionResult> Delete(int id)
  {
    var o=await _db.Ocorrencias.FindAsync(id);if(o is null)return NotFound(new {mensagem="Ocorrência não encontrada."});
    _db.Ocorrencias.Remove(o);await _db.SaveChangesAsync();return NoContent();
  }
}
