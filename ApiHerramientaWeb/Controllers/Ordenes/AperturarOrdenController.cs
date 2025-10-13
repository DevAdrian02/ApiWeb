using ApiHerramientaWeb.Controllers.Ordenes.Emails;
using ApiHerramientaWeb.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModeloPrincipal.Entity;
using System.Net;
using System.Net.Mail;


namespace ApiHerramientaWeb.Controllers.Ordenes
{
    [Route("api/[controller]")]
    [ApiController]
    public class AperturarOrdenController : ControllerBase
    {
        private readonly CVGEntities _context;
        private readonly ConfiguracionEmail _configEmail;


        public AperturarOrdenController(CVGEntities context, ConfiguracionEmail configEmail)
        {
            _context = context;
            _configEmail = configEmail;

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

    }
}
