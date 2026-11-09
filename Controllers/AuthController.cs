using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ZeladoriaUrbana.API.Data;
using ZeladoriaUrbana.API.Services;

namespace ZeladoriaUrbana.API.Controllers;
[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly AppDbContext _db;
    public AuthController(IConfiguration config, AppDbContext db) { _config=config; _db=db; }
    public record LoginModel(string Usuario, string Senha);
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Usuario) || string.IsNullOrWhiteSpace(model.Senha)) return BadRequest(new { mensagem="Informe usuário e senha." });
        var login = model.Usuario.Trim();
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u=>u.Email==login || u.Nome==login);
        if (usuario is null || !SenhaService.Verificar(model.Senha, usuario.SenhaHash)) return Unauthorized(new { mensagem="Usuário ou senha inválidos." });
        if (SenhaService.Legado(usuario.SenhaHash)) { usuario.SenhaHash=SenhaService.GerarHash(model.Senha); await _db.SaveChangesAsync(); }
        var key=Encoding.UTF8.GetBytes(_config["Jwt:Key"]!);
        var identity=new ClaimsIdentity(new[] {new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),new Claim(ClaimTypes.Name,usuario.Nome),new Claim(ClaimTypes.Role,usuario.Perfil)});
        var descriptor=new SecurityTokenDescriptor {Subject=identity, Expires=DateTime.UtcNow.AddHours(2), Issuer=_config["Jwt:Issuer"], Audience=_config["Jwt:Audience"], SigningCredentials=new SigningCredentials(new SymmetricSecurityKey(key),SecurityAlgorithms.HmacSha256)};
        var handler=new JwtSecurityTokenHandler();
        return Ok(new { token=handler.WriteToken(handler.CreateToken(descriptor)),nome=usuario.Nome,perfil=usuario.Perfil });
    }
}
