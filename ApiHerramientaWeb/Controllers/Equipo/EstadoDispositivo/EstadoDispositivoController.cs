using ApiHerramientaWeb.Controllers.Integraciones.Krill;
using ApiHerramientaWeb.Controllers.Integraciones.SmartOlt;
using ApiHerramientaWeb.Modelos;
using ApiHerramientaWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ModeloPrincipal.Entity;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using static ApiHerramientaWeb.Modelos.Operaciones.Estructuras.DatosOpe.cablemodems;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        private readonly MoviTvService _moviTvServices;
        private readonly ModemService _estadoModemService;
        private readonly Utils _utils;
        private readonly IDesactivarDispositivoService _desactivarDispositivoService;


        public EstadoDispositivoController(
            CVGEntities context,
            IConfiguration configuration,
            KrillService krillService,
            SmartOltService smartOltService,
            SmartOltCatvService smartOlCatvService,
            MoviTvService moviTvServices,
            ModemService estadoModemService,
            IDesactivarDispositivoService desactivarDispositivoService)
        {
            _context = context;
            _krillService = krillService;
            _smartOltService = smartOltService;
            _utils = new Utils(_context, configuration);
            _smartOlCatvService = smartOlCatvService;
            _moviTvServices = moviTvServices;
            _estadoModemService = estadoModemService;
            _desactivarDispositivoService = desactivarDispositivoService;

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
                var contratosConMoviTv = query.Where(c => c.id_servicio == 5).ToList();


                if (contrato == null)
                    return Json(new { success = false, mensaje = "Contrato no encontrado" });
                var internet = query.Any(o => o.id_servicio == 3);

                if (!query.Any(o => o.id_servicio == 3))
                {
                    return Json(new { success = false, mensaje = "Contrato solo contiene TV" });

                }

                if (contratosConMoviTv.Any())
                {
                    foreach (var contrato_ in contratosConMoviTv)
                    {
                        try
                        {
                            var partnerId = contrato_.CONTRATO.ToString();

                            // Llamada a MoviTvService para suspender o desactivar
                            await _moviTvServices.ActivarAsync(partnerId);

                            Console.WriteLine($"Usuario MoviTV {partnerId} suspendido correctamente.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error al suspender MoviTV para {contrato.CONTRATO}: {ex.Message}");
                        }
                    }
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
                try

                {
                    var ip = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";


                    var resultado = await _desactivarDispositivoService.DesactivarCmInternoAsync(request,ip);



                    if (!resultado.Success)
                        return Json(new { success = false, mensaje = resultado.Mensaje });

                    return Json(new { success = true, mensaje = resultado.Mensaje });
                }
                catch (Exception ex)
                {
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