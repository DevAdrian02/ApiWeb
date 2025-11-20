using ApiHerramientaWeb.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace ApiHerramientaWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UbicacionController : ControllerBase
    {
        private readonly IHubContext<UbicacionHub> _hubContext;

        public UbicacionController(IHubContext<UbicacionHub> hubContext)
        {
            _hubContext = hubContext;
        }

        [HttpPost("enviar")]
        public async Task<IActionResult> EnviarUbicacion([FromBody] UbicacionRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.IdUsuario))
                return BadRequest("Datos inválidos.");

            var data = new
            {
                IdUsuario = request.IdUsuario,
                Latitud = request.Latitud,
                Longitud = request.Longitud,
                Timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients.All.SendAsync("RecibirUbicacion", data);

            return Ok(new { message = "Ubicación recibida y enviada correctamente.", data });
        }
    }

    public class UbicacionRequest
    {
        public string IdUsuario { get; set; } = string.Empty;
        public double Latitud { get; set; }
        public double Longitud { get; set; }
    }
}