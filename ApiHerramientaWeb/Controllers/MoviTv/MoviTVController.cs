using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ModeloPrincipal.Entity;
using ApiHerramientaWeb.Services;
using ApiHerramientaWeb.Modelos.MoviTvCuerpo;

namespace ApiHerramientaWeb.Controllers.MoviTv
{
    [ApiController]
    [Route("api/[controller]")]
    public class MoviTVController : Controller
    {
        private readonly CVGEntities _context;
        private readonly ConfiguracionEmail _configuracionEmail;

        public MoviTVController(CVGEntities context, ConfiguracionEmail configuracionEmail)
        {
            _context = context;
            _configuracionEmail = configuracionEmail;
        }

        [HttpGet("GetMoviTVData")]
        public async Task<IActionResult> GetMoviTVData(
            int contrato,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var contratoMovi = await _context.Mstlogins
                    .Where(c => c.NoContrato == contrato)
                    .Select(c => new
                    {
                        c.NoContrato,
                        c.Login,
                        c.Email,
                        c.Password,
                        c.Activado
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (contratoMovi == null)
                {
                    return NotFound(new { Message = "Contrato no encontrado." });
                }

                return Ok(contratoMovi);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPost("EnviarCorreoBienvenida")]
        public async Task<IActionResult> EnviarCorreoBienvenida(
            [FromBody] EnvioCorreoBienvenidaRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Validar request
                if (string.IsNullOrEmpty(request.Email) ||
                    string.IsNullOrEmpty(request.Usuario) ||
                    string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest(new { Message = "Email, usuario y contraseña son requeridos." });
                }

                // Crear objeto UsuarioMoviTv con los datos
                var usuarioMoviTv = new UsuarioMoviTv
                {
                    Nombre = request.Nombre ?? request.Usuario,
                    Usuario = request.Usuario,
                    Password = request.Password,
                    Url =  "https://movitvapp.com"
                };

                // Generar el HTML usando tu clase existente
                var cuerpoCorreo = new CuerpoCorreoMovitTV();
                string cuerpoHtml = cuerpoCorreo.GenerarCorreoBienvenida(usuarioMoviTv);

                string asunto = request.Asunto ?? "¡Bienvenido a MoviTV! - Tus Credenciales de Acceso";

                // Enviar el correo
                var (exito, error) = await _configuracionEmail.EnviarCorreoSendGridSMTP(
                    request.Email,
                    asunto,
                    cuerpoHtml);

                if (!exito)
                {
                    return StatusCode(500, new
                    {
                        Error = "Error al enviar el correo",
                        Detalles = error
                    });
                }

                return Ok(new
                {
                    Message = "Correo de bienvenida enviado exitosamente",
                    Destinatario = request.Email,
                    Usuario = request.Usuario
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Error = "Error interno del servidor",
                    Detalles = ex.Message
                });
            }
        }


        [HttpGet("GetDataContrato")]
        public async Task<IActionResult> GetDataContrato(
            int contrato,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var contratoCliente = await _context.VwcontratoClientes
                    .Where(c => c.Contrato == contrato)
                    .Select(c => new
                    {
                        c.Contrato,
                        c.Mail,
                        c.Name,
                        c.Direccion,
                        c.Idempresa
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (contratoCliente == null)
                {
                    return NotFound(new { Message = "Contrato no encontrado." });
                }

                return Ok(contratoCliente);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }


    }

    // Modelos para los requests
    public class EnvioCorreoBienvenidaRequest
    {
        public string? Nombre { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Asunto { get; set; }
    }

   
}