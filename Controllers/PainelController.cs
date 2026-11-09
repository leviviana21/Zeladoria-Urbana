using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ZeladoriaUrbana.API.Data;
using ZeladoriaUrbana.API.Services;

namespace ZeladoriaUrbana.API.Controllers;

[Route("painel-mvc")]
public class PainelController : Controller
{
    private readonly AppDbContext db;
    public PainelController(AppDbContext db) => this.db = db;

    [HttpGet("login")]
    public IActionResult Login() => View();

    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string usuario, string senha)
    {
        if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(senha)) { ModelState.AddModelError("", "Informe usuário e senha."); return View(); }
        var nome = usuario.Trim();
        var u = await db.Usuarios.FirstOrDefaultAsync(x => x.Email == nome || x.Nome == nome);
        if (u == null || !SenhaService.Verificar(senha, u.SenhaHash) || (u.Perfil != "Administrador" && u.Perfil != "Operador"))
        { ModelState.AddModelError("", "Usuário ou senha inválidos."); return View(); }
        if (SenhaService.Legado(u.SenhaHash)) { u.SenhaHash = SenhaService.GerarHash(senha); await db.SaveChangesAsync(); }
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier,u.Id.ToString()), new Claim(ClaimTypes.Name,u.Nome), new Claim(ClaimTypes.Role,u.Perfil) },"PainelCookie");
        await HttpContext.SignInAsync("PainelCookie",new ClaimsPrincipal(identity));
        return RedirectToAction(nameof(Index));
    }

    [Authorize(AuthenticationSchemes="PainelCookie",Roles="Administrador,Operador")]
    [HttpGet("")]
    public async Task<IActionResult> Index(string? situacao)
    {
        var query = db.Ocorrencias.AsNoTracking().Include(x=>x.Categoria).AsQueryable();
        if (!string.IsNullOrWhiteSpace(situacao) && new[]{"Pendente","Em Andamento","Concluído"}.Contains(situacao)) query=query.Where(x=>x.Situacao==situacao);
        ViewBag.Filtro = situacao;
        return View(await query.OrderByDescending(x=>x.DataCriacao).Take(500).ToListAsync());
    }

    [Authorize(AuthenticationSchemes="PainelCookie",Roles="Administrador,Operador")]
    [HttpPost("status/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Status(int id,string situacao)
    {
        if (!new[]{"Pendente","Em Andamento","Concluído"}.Contains(situacao)) return BadRequest("Situação inválida.");
        var ocorrencia=await db.Ocorrencias.FindAsync(id);
        if (ocorrencia == null) return NotFound();
        ocorrencia.Situacao=situacao;
        await db.SaveChangesAsync();
        TempData["Mensagem"]="Situação atualizada.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(AuthenticationSchemes="PainelCookie",Roles="Administrador")]
    [HttpPost("excluir/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Excluir(int id)
    {
        var ocorrencia=await db.Ocorrencias.FindAsync(id);
        if(ocorrencia == null) return NotFound();
        db.Ocorrencias.Remove(ocorrencia);
        await db.SaveChangesAsync();
        TempData["Mensagem"]="Ocorrência excluída.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(AuthenticationSchemes="PainelCookie",Roles="Administrador,Operador")]
    [HttpPost("sair")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sair(){await HttpContext.SignOutAsync("PainelCookie");return RedirectToAction(nameof(Login));}

    [Authorize(AuthenticationSchemes="PainelCookie",Roles="Administrador,Operador")]
    [HttpGet("diversidade")]
    public IActionResult Diversidade() => View();
}
