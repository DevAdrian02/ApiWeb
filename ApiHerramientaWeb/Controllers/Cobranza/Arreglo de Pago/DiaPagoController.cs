using ApiHerramientaWeb.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModeloPrincipal.Entity;

namespace ApiHerramientaWeb.Controllers.Cobranza.Arreglo_de_Pago
{
    [ApiController]
    [Route("api/[controller]")]
    public class DiaPagoController : Controller
    {
        private readonly CVGEntities _context;
        private readonly Utils _utils;
        private readonly IConfiguration _configuration;

        public DiaPagoController(CVGEntities context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _utils = new Utils(_context, _configuration);
        }

        public class UpdateDiaPagoRequest
        {
            public int Cnt { get; set; }
            public int DiaPago { get; set; }
        }

        [HttpPatch("UpdateDiaPago")]
        public async Task<IActionResult> UpdateDiaPago([FromBody] UpdateDiaPagoRequest request)
        {
            if (request.Cnt <= 0)
            {
                return Json(new { success = false, message = "Número de contrato no proporcionado." });
            }
            if (request.DiaPago < 1 || request.DiaPago > 30)
            {
                return Json(new { success = false, message = "Día de pago no válido. Debe estar entre 1 y 30." });
            }
            try
            {
                var contrato = await _context.Mstcnts.FirstOrDefaultAsync(c => c.Ideftocnt == request.Cnt); 
                if (contrato == null)
                {
                    return Json(new { success = false, message = "Contrato no encontrado." });
                }
                contrato.Diapag = (byte?) request.DiaPago;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Día de pago actualizado correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al actualizar el día de pago: {ex.Message}" });
            }
        }

    }
}
