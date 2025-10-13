using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModeloPrincipal.Entity;

namespace ApiHerramientaWeb.Controllers.Zona
{
    [ApiController]
    [Route("api/[controller]")]
    public class ZonaController : Controller
    {
        private readonly CVGEntities _context;

        public ZonaController(CVGEntities context)
        {
            _context = context;
        }

        [HttpGet("GetZonas")]
        public async Task<IActionResult> GetZonas()
        {
            try
            {
                var zonas = await _context.Mstubigeos
                    .Select(m => new
                    {
                        m.Ideubigeo,
                        m.Dscubigeo,
                        m.Tipubigeo
                    })
                    .Where(m=> m.Tipubigeo == "ZN")
                    .ToListAsync();
                return Ok(zonas);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al obtener las zonas", error = ex.Message });
            }
        }
    }
}
