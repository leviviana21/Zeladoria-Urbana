using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZeladoriaUrbana.API.Data;

namespace ZeladoriaUrbana.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoriasController : ControllerBase
{
    private readonly AppDbContext _context;
    public CategoriasController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await _context.Categorias.AsNoTracking().OrderBy(c => c.Nome).ToListAsync());
}
