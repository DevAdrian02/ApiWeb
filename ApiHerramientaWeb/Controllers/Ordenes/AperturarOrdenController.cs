using ApiHerramientaWeb.Controllers.Ordenes.Emails;
using ApiHerramientaWeb.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModeloPrincipal.Entity;
using System.Net;
using System.Net.Mail;
using ApiHerramientaWeb.Modelos.Ordenes;
using static ApiHerramientaWeb.Modelos.Operaciones.Estructuras.DatosOpe.cablemodems;

namespace ApiHerramientaWeb.Controllers.Ordenes
{
    [Route("api/[controller]")]
    [ApiController]
    public class AperturarOrdenController : ControllerBase
    {
        private readonly CVGEntities _context;
        private readonly ConfiguracionEmail _configEmail;
        private readonly OrdenService _repository;
        private readonly IDesactivarDispositivoService _desactivarDispositivoService;




        public AperturarOrdenController(CVGEntities context, ConfiguracionEmail configEmail, OrdenService repository, IDesactivarDispositivoService desactivarDispositivoService)
        {
            _context = context;
            _configEmail = configEmail;
            _repository = repository;
            _desactivarDispositivoService = desactivarDispositivoService;

        }

        #region Enviar correo de aviso de visita

        public class CorreoAvisoVisitaRequest
        {
            public string Destinatario { get; set; }
            public string Cliente { get; set; }
            public string NombreColector { get; set; }
        }

        [HttpPost("EnviarAvisoVisita")]
        public async Task<IActionResult> EnviarAvisoVisita([FromBody] CorreoAvisoVisitaRequest request)
        {
            if (string.IsNullOrEmpty(request.Destinatario) ||
                string.IsNullOrEmpty(request.Cliente) ||
                string.IsNullOrEmpty(request.NombreColector))
            {
                return Ok(new
                {
                    status = 0,
                    message = "Destinatario, Cliente y Nombre del Colector son obligatorios."
                });
            }

            try
            {
                string asunto = "Aviso de Visita de Colector";
                string cuerpoCorreo = GenerarCuerpoAviso.GenerarCuerpoAvisoVisita(request.Cliente, request.NombreColector);

                var resultado = await _configEmail.EnviarCorreoSendGridSMTP(request.Destinatario, asunto, cuerpoCorreo);

                if (resultado.Exito)
                {
                    return Ok(new
                    {
                        status = 1,
                        message = "Correo enviado exitosamente."
                    });
                }
                else
                {
                    return Ok(new
                    {
                        status = 0,
                        message = "Hubo un problema al enviar el correo."
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    status = 0,
                    message = $"Error al enviar el correo: {ex.Message}"
                });
            }
        }



        #endregion


        #region Enviar correo de aviso de cobro

        public class CorreoAvisoDeCobro
        {
            public string Destinatario { get; set; }
            public string Cliente { get; set; }
            public string ContratoNo { get; set; }
            public string Sector { get; set; }
            public string MesFactura { get; set; }
            public decimal Monto { get; set; }
            public string? FacturaNo { get; set; } // Nuevo campo opcional para número de factura
        }

        [HttpPost("EnviarAvisoCobro")]
        public async Task<IActionResult> EnviarAvisoCobro([FromBody] CorreoAvisoDeCobro request)
        {
            if (string.IsNullOrEmpty(request.Destinatario) ||
                string.IsNullOrEmpty(request.Cliente) ||
                string.IsNullOrEmpty(request.ContratoNo) ||
                string.IsNullOrEmpty(request.Sector) ||
                string.IsNullOrEmpty(request.MesFactura))
            {
                return Ok(new
                {
                    status = 0,
                    message = "Todos los campos obligatorios deben estar completos."
                });
            }

            try
            {
                string asunto = "Aviso de Cobro - Casavision";
                string cuerpoCorreo = GenerarCuerpoAviso.GenerarCuerpoAvisoCobro(
                    cliente: request.Cliente,
                    noContrato: request.ContratoNo,
                    sector: request.Sector,
                    mesFactura: request.MesFactura,
                    monto: request.Monto,
                    factura: request.FacturaNo // Campo opcional
                );

                var resultado = await _configEmail.EnviarCorreoSendGridSMTP(request.Destinatario, asunto, cuerpoCorreo);

                if (resultado.Exito)
                {
                    return Ok(new
                    {
                        status = 1,
                        message = "Aviso de cobro enviado exitosamente."
                    });
                }
                else
                {
                    return Ok(new
                    {
                        status = 0,
                        message = "Hubo un problema al enviar el aviso de cobro."
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    status = 0,
                    message = $"Error al enviar el aviso de cobro: {ex.Message}"
                });
            }
        }

        #endregion

        #region Obtener información de asignación
        [HttpGet("ObtenerTecnicos")]
        public async Task<IActionResult> ObtenerTecnicos()
        {
            var tecnicos = await _context.Msttecs
                .Where(t => t.Esttec == 1)
                .Select(t => new
                {
                    t.Idetec,
                    t.Apepritec,
                    t.Apesegtec,
                    t.Nomtec,

                })
                .ToListAsync();

            return Ok(tecnicos);
        }

        [HttpGet("ObtenerCuadrillas")]

        public async Task<IActionResult> ObtenerCuadrillas()
        {
            var cuadrillas = await _context.Mstcuadrillas
                .Where(c => c.Activo == true)
                .Select(c => new
                {
                    c.Idcuadrilla,
                    c.Descripcion,
                    c.Idsucursal,
                    c.Placa,
                    c.Bodega
                }).ToListAsync();

            return Ok(cuadrillas);

        }

        #endregion


        #region APERTURA DE ORDEN

        [HttpPost("aperturarOrden")]
        public async Task<IActionResult> CrearOrdenColector([FromBody] CrearOrdenRequestModel request)
        {
            // 1️⃣ Crear orden normalmente
            var ip = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";

            var user = await _context.Mstusrs
               .Where(c => c.Ideusr == request.Usuario)
               .Select(c => new { c.Codusr })
               .FirstOrDefaultAsync();

            var result = await _repository.CrearOrdenColectorAsync(
                request.IDECNT,
                request.IDETECAsg,
                request.IDCUADRILLA,
                user.Codusr
            );

            // 2️⃣ Buscar el IDEFTOCNT (o IDEFCONT) en la tabla MSTCNT según IDECNT
            var contrato = await _context.Mstcnts
                .Where(c => c.Idecnt == request.IDECNT)
                .Select(c => new { c.Ideftocnt }) // o c.Ideftocnt según cómo se llame en tu modelo
                .FirstOrDefaultAsync();

            if (contrato == null)
            {
                return BadRequest(new { message = "No se encontró el contrato en MSTCNT con ese IDECNT." });
            }

            var iduser = await _context.Mstusrs
                .Where(c => c.Ideusr == request.Usuario)
                .Select(c => new { c.Ideusr })
                .FirstOrDefaultAsync();

            if (iduser == null)
            {
                return BadRequest(new { message = "No se encontró el usuario" });
            }


            // 3️⃣ Armar la solicitud para desactivar
            var desactivarRequest = new DesactivarCmRequest
            {
                iduser = iduser.Ideusr,
                Ideftocnt = contrato.Ideftocnt, // el valor que obtuvimos
                Comentario = "Desactivado al crear orden"
            };

            // 4️⃣ Llamar al método interno
            var desactivarResultado = await _desactivarDispositivoService.DesactivarCmInternoAsync(desactivarRequest, ip);

            // 5️⃣ Responder con ambos resultados
            return Ok(new
            {
                message = result,
                desactivacion = desactivarResultado
            });
        }


        [HttpPost("aperturarOrdenReconexion")]
        public async Task<IActionResult> CrearOrdenReconexionTapNap([FromBody] CrearOrdenRequestModel request)
        {
            // 1️⃣ Obtener el CODUSR del usuario logueado (necesario para el SP)
            var user = await _context.Mstusrs
                .Where(c => c.Ideusr == request.Usuario)
                .Select(c => new { c.Codusr })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return BadRequest(new { message = "No se encontró el usuario especificado." });
            }

            // 2️⃣ Llamar al repositorio (Método nuevo que acabamos de crear)
            var result = await _repository.CrearOrdenReconexionTapNapAsync(
                request.IDECNT,
                request.IDETECAsg,
                request.IDCUADRILLA,
                user.Codusr
            );

            // 3️⃣ Responder con los mensajes del SP (PRINTs capturados)
            return Ok(new
            {
                message = result
            });
        }


        [HttpPost("aperturarOrdenDesconexion")]
        public async Task<IActionResult> CrearOrdenDesconexion([FromBody] CrearOrdenDesconexionModel request)
        {
            // Buscar usuario
            var user = await _context.Mstusrs
                .Where(c => c.Ideusr == request.Usuario)
                .Select(c => new { c.Codusr })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return BadRequest(new { message = "Usuario no encontrado." });
            }

            // Crear orden principal
            var result = await _repository.CrearOrdenDesconexionAsync(
                request.IDECNT,
                request.IDETECAsg,
                request.IDCUADRILLA,
                user.Codusr
            );

            // Intentar notificar al servicio externo (sin importar si falla)
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var url = "https://wservices.casavision.com/CvApiDev/api/cv/SendNotificationByTecnico";

                    var payload = new
                    {
                        IdTecnico = request.IDETECAsg,
                        TipoNotificacion = 3
                    };

                    var json = System.Text.Json.JsonSerializer.Serialize(payload);
                    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                    var response = await httpClient.PostAsync(url, content);

                    // Ignorar completamente la respuesta (no importa si da error o no)
                }
            }
            catch
            {
                // Silenciosamente ignorar cualquier error del API externo
            }

            // Responder éxito principal
            return Ok(new { message = result });
        }




        [HttpGet("ConfirmarOrdenTipo")]
        public async Task<IActionResult> ConfirmarOrdenTipo(int idefcnt)
        {
            try
            {
                // Buscar el contrato por idefcnt en la tabla mstcnt
                var idecnt = await _context.Mstcnts
                     .Where(c => c.Ideftocnt == idefcnt)
                            .Select(c => c.Idecnt)
                            .FirstOrDefaultAsync();

                if (idecnt == null)
                {
                    return Ok(false);
                }

                // Verificar si existe una orden tipo 91 en estado 00020
                bool tieneOrden = await _context.Msttickets
                    .AnyAsync(t => t.Idecnt == idecnt && t.Ideord == 91 && t.Codesttkt == "00020");

                return Ok(tieneOrden);
            }
            catch
            {
                // En caso de error, devolver false
                return Ok(false);
            }
        }


        #endregion

        #region Obtener historico visitas


        [HttpGet("visitas/{idUser}")]
        public async Task<IActionResult> ObtenerVisitasColector(int idUser)
        {
            var visitas = await _repository.ObtenerVisitasColectorPorUsuarioAsync(idUser);

            if (visitas == null || visitas.Count == 0)
                return NotFound(new { mensaje = "No se encontraron visitas para este usuario." });

            return Ok(visitas);
        }
        #endregion
    }
}
