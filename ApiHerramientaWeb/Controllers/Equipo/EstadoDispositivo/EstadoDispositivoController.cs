using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApiHerramientaWeb.Controllers.Integraciones.SmartOlt;
using ApiHerramientaWeb.Controllers.Integraciones.Krill;
using ModeloPrincipal.Entity;
using static ApiHerramientaWeb.Modelos.Operaciones.Estructuras.DatosOpe.cablemodems;
using static System.Runtime.InteropServices.JavaScript.JSType;
using ApiHerramientaWeb.Modelos;
using Microsoft.Extensions.Configuration;
using ApiHerramientaWeb.Services;

namespace ApiHerramientaWeb.Controllers.Equipo.EstadoDispositivo
{
    [ApiController]
    [Route("api/[controller]")]
    public class EstadoDispositivoController : Controller
    {
        private readonly CVGEntities _context;
        private readonly KrillService _krillService;
        private readonly SmartOltService _smartOltService;
        private readonly SmartOltCatvService _smartOlCatvService;
        private readonly ModemService _estadoModemService;
        private readonly Utils _utils;

        public EstadoDispositivoController(
            CVGEntities context,
            IConfiguration configuration,
            KrillService krillService,
            SmartOltService smartOltService,
            SmartOltCatvService smartOlCatvService,
            ModemService estadoModemService)
        {
            _context = context;
            _krillService = krillService;
            _smartOltService = smartOltService;
            _utils = new Utils(_context, configuration);
            _smartOlCatvService = smartOlCatvService;
            _estadoModemService = estadoModemService;
        }



        #region BuscarDatos
        [HttpGet("Buscar_datos")]
        public async Task<ActionResult> Buscar_datos(int? numero_contrato)
        {
            try
            {
                var query = await _estadoModemService.ObtenerQueryContrato(numero_contrato);

                if (!query.Any())
                    return Json(new { success = false, datos = new List<activaCmDt>() });

                var tipo_tecnologia = query.FirstOrDefault()?.IDTECNOLOGIA ?? 0;
                var internet = query.Any(o => o.id_servicio == 3 && o.ESTADO_CONTRATO_NUM == 1);
                var estado_contrato = query.FirstOrDefault(o => o.PRIMARIO)?.ESTADO_CONTRATO;
                var estado_contrato_numero = query.FirstOrDefault(o => o.PRIMARIO)?.ESTADO_CONTRATO_NUM ?? 0;
                var suspensionCompleto = query.FirstOrDefault(o => o.suspensionCompleto)?.suspensionCompleto ?? false;

                var activeIntegrations = await _context.Mstintegracions
                    .Where(i => i.Idtecnologia == tipo_tecnologia && i.Activa == true)
                    .Select(i => i.Idintegracion)
                    .ToListAsync();

                List<activaCmDt> srvCnt = new List<activaCmDt>();
                bool disponible_activar = false;
                EstadoModem ESTADO_MODEM = EstadoModem.Inactivo;
                bool UBICACION = true;

                foreach (var integracion_ in activeIntegrations)
                {
                    if (internet)
                    {
                        switch (integracion_)
                        {
                            case 4: // Krill
                                var krillData = await _krillService.GetEstadoModemAsync(
                           
                                    query.FirstOrDefault()?.COD_SUC,
                                             query.FirstOrDefault()?.REALM);

                                ESTADO_MODEM = krillData.estadoModem;
                                disponible_activar = krillData.disponibleActivar;
                                break;

                            case 2: // SmartOlt
                                var smartOltData = await _smartOltService.GetEstadoModemAsync(
                                    query.FirstOrDefault()?.COD_SUC);

                                ESTADO_MODEM = smartOltData.estadoModem;
                                disponible_activar = smartOltData.disponibleActivar;
                                break;
                        }
                    }
                    else
                    {
                        ESTADO_MODEM = EstadoModem.SoloTV;
                    }
                }

                srvCnt = query.GroupBy(q => new { q.Marca, q.Modelo })
                    .Select(g => g.First())
                    .Select(item => new activaCmDt
                    {
                        TENENCIA = item.TENENCIA,
                        CONTRATO = item.CONTRATO.ToString(),
                        ESTTEC = estado_contrato_numero,
                        ESTADO = estado_contrato,
                        ESTADO_MODEM = ESTADO_MODEM,
                        NOMBRE = item.NOMBRE,
                        COD_SUC = item.COD_SUC,
                        LATITUD = (double)item.LATITUD,
                        LONGITUD = (double)item.LONGITUD,
                        DIRECCION = item.DIRECCION,
                        SERVICIO = string.Join(", ", query.Select(o => o.SERVICIO)),
                        DICMACCM = item.DICMACCM,
                        SUCURSAL = item.SUCURSAL,
                        ID_TECNOLOGIA = item.IDTECNOLOGIA,
                        REALM = item.REALM,
                        UBICACION = UBICACION,
                        MODEM_DISPONIBLE = disponible_activar,
                        Marca = item.Marca,
                        Modelo = item.Modelo,
                        Faja = item.Faja,
                        TipoTecnologia = tipo_tecnologia,
                        suspensionCompleto = suspensionCompleto
                    }).ToList();

                return Json(new { success = true, datos = srvCnt });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, mensaje = ex.Message });
            }
        }
        #endregion




        #region Activar 
        [HttpPost("activarcm")]
        public async Task<IActionResult> activarcm(int ideftocnt)
        {
            try
            {
                var query = await _estadoModemService.ObtenerQueryContrato(ideftocnt);
                var contrato = query.FirstOrDefault();

                if (contrato == null)
                    return Json(new { success = false, mensaje = "Contrato no encontrado" });
                var internet = query.Any(o => o.id_servicio == 3);

                if (!query.Any(o => o.id_servicio == 3))
                {
                    return Json(new { success = false, mensaje = "Contrato solo contiene TV" });

                }



                var tipo_tecnologia = contrato.IDTECNOLOGIA;
                var activeIntegrations = await _context.Mstintegracions
                    .Where(i => i.Idtecnologia == tipo_tecnologia && i.Activa == true)
                    .Select(i => i.Idintegracion)
                    .ToListAsync();

                foreach (var integracion_ in activeIntegrations)
                {
                    switch (integracion_)
                    {
                        case 4: // Krill
                            await _krillService.ActivarAsync(
                                contrato.COD_SUC,
                                contrato.REALM);
                            break;

                        case 2: // SmartOlt
                            await _smartOltService.ActivarAsync(
                                contrato.COD_SUC);
                            break;
                    }
                }

                return Json(new { success = true, mensaje = "Activación exitosa" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, mensaje = ex.Message });
            }
        }
        #endregion

        #region Desactivar
        [HttpPost("desactivarcm")]
        public async Task<IActionResult> Desactivarcm([FromBody] DesactivarCmRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var query = await _estadoModemService.ObtenerQuerySuspencion(request.Ideftocnt);
                var suspesion = query.FirstOrDefault();

                if (suspesion == null)
                    return Json(new { success = false, mensaje = "Contrato no cuenta con factura pendiente" });

                var resultado = _estadoModemService.ValidarDiaPagoSuspension(suspesion.DiaPago);
                if (resultado.codigo != 0)
                    return Json(new { success = false, resultado.mensaje });

                var tipo_tecnologia = suspesion.IDTECNOLOGIA;
                var activeIntegrations = await _context.Mstintegracions
                    .Where(i => i.Idtecnologia == tipo_tecnologia && i.Activa == true)
                    .Select(i => i.Idintegracion)
                    .ToListAsync();

                if (activeIntegrations.Count > 0 && suspesion.ActivacionColector == 0 && suspesion.Aprovisiona == true)
                {
                    foreach (var integracion_ in activeIntegrations)
                    {
                        switch (integracion_)
                        {
                            case 4: // Krill
                                await _krillService.DesactivarAsync(
                                    suspesion.COD_SUC,
                                    suspesion.REALM);
                                break;

                            case 2: // SmartOlt
                                await _smartOltService.DesactivarAsync(
                                    suspesion.COD_SUC);
                                break;
                        }
                    }
                }

                var usuario = await _utils.ObtenerCodigoUsuarioPorIdAsync(request.iduser);
                if (string.IsNullOrEmpty(usuario))
                    return Json(new { success = false, mensaje = "Usuario no encontrado." });

                var ip = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
                var (success, error) = await _estadoModemService.RegistrarSuspensionAsync(
                    suspesion.IDCONTRATO,
                    request.Comentario,
                    usuario,
                    ip,
                    suspesion.CONTRATO.ToString()
                );

                if (!success)
                    return Json(new { success = false, mensaje = error });

                await transaction.CommitAsync();
                return Json(new { success = true, mensaje = "Suspension exitosa" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, mensaje = ex.Message });
            }
        }
        #endregion

        #region Habilitar Botón TV
        [HttpGet("habilitarBotonTv")]
        public async Task<IActionResult> HabilitarBotonTv(int ideftocnt)
        {
            var query = await _estadoModemService.ObtenerQueryContrato(ideftocnt);

            var contrato = query.FirstOrDefault();
            if (contrato == null || string.IsNullOrEmpty(contrato.COD_SUC) || contrato.IDTECNOLOGIA == 1)
                return Json(new { success = false, habilitado = 2 });

            var (_, disponibleActivar) = await _smartOltService.GetEstadoModemAsync(contrato.COD_SUC);
            int habilitado = disponibleActivar ? 1 : 0;
            return Json(new { success = true, habilitado });
        }
        #endregion

        #region Activar TV (solo CATV)
        [HttpPost("activarTv")]
        public async Task<IActionResult> ActivarTv(int ideftocnt)
        {
            try
            {
                var query = await _estadoModemService.ObtenerQueryContrato(ideftocnt);
                var contrato = query.FirstOrDefault();
                if (contrato == null || string.IsNullOrEmpty(contrato.COD_SUC))
                    return Json(new { success = false, mensaje = "Contrato no encontrado o sin CodSuc" });

                await _smartOlCatvService.DesactivarCatvAsync(contrato.COD_SUC);
                await _smartOlCatvService.ActivarCatvAsync(contrato.COD_SUC);

                return Json(new { success = true, mensaje = "CATV activado correctamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, mensaje = ex.Message });
            }
        }
        #endregion

     
    }
}