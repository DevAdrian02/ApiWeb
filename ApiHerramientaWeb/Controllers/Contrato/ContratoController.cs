using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModeloPrincipal.Entity;
using static ApiHerramientaWeb.Controllers.Cobranza.Arreglo_de_Pago.DiaPagoController;

namespace ApiHerramientaWeb.Controllers.Contrato
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContratoController : Controller
    {
        private readonly CVGEntities _context;

        public ContratoController(CVGEntities context)
        {
            _context = context;
        }

        public class UpdateGeoContratoRequest
        {
            public int Cnt { get; set; }
            public decimal latitud { get; set; }
            public decimal longitud { get; set; }
        }
        [HttpPatch("UpdateGeoContrato")]
        public async Task<IActionResult> UpdateGeoContrato([FromBody] UpdateGeoContratoRequest request)
        {
            try
            {
                var contrato = await _context.Mstcnts
                    .Where(c => c.Ideftocnt == request.Cnt)
                    .FirstOrDefaultAsync();
                if (contrato == null)
                {
                    return NotFound(new { code = 0, message = "Contrato no encontrado." });
                }
                contrato.Latitud = request.latitud;
                contrato.Longitud = request.longitud;
                _context.Mstcnts.Update(contrato);
                await _context.SaveChangesAsync();
                return Ok(new { code = 1, message = "Geolocalización actualizada correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { code = 0, message = "Error al actualizar la geolocalización: " + ex.Message });
            }
        }

    }
}
