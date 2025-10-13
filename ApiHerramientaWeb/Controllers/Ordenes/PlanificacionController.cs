using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModeloPrincipal.Entity;

namespace ApiHerramientaWeb.Controllers.Ordenes
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlanificacionController : Controller
    {
        private readonly CVGEntities _context;
        public PlanificacionController(
            CVGEntities context)
        {
            _context = context;
        }

        [HttpGet("GetPlanificacion")]
        public async Task<IActionResult> cargarPlanificación(int idSuc)
        {
            try
            {
                var planificacion = await _context.VwPlanificaciones
                    .Where(p => p.Idsucursal == idSuc)
                    .ToListAsync();

                if (planificacion == null || !planificacion.Any())
                {
                    return NotFound(new { Message = "No se encontraron registros de planificación." });
                }

                // Agrupar por Fecha y Técnico
                var agrupado = planificacion
                    .GroupBy(p => new { p.Fecha, p.Tecnico })
                    .Select(g => new
                    {
                        Fecha = g.Key.Fecha,
                        Tecnico = g.Key.Tecnico,
                        Cuadrillas = g.Select(x => x.Cuadrilla).Distinct().ToList(),
                        Ordenes = g.ToList(),
                        TieneVariasCuadrillas = g.Select(x => x.Cuadrilla).Distinct().Count() > 1
                    })
                    .ToList();

                // Buscar técnicos con más de una cuadrilla en la misma fecha
                var tecnicosConConflicto = agrupado
                    .Where(x => x.TieneVariasCuadrillas)
                    .Select(x => new { x.Fecha, x.Tecnico, x.Cuadrillas })
                    .ToList();

                if (tecnicosConConflicto.Any())
                {
                    return StatusCode(409, new
                    {
                        Message = "Hay técnicos asignados a más de una cuadrilla en la misma fecha.",
                        Conflictos = tecnicosConConflicto
                    });
                }

                return Ok(agrupado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error al cargar la planificación.", Error = ex.Message });
            }
        }


    }
}
