using ApiHerramientaWeb.Modelos;
using ApiHerramientaWeb.Modelos.CargarFacturas;
using ApiHerramientaWeb.Modelos.Result;
using ApiHerramientaWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ModeloPrincipal.Entity;
using static ApiHerramientaWeb.Modelos.Krill.KrillModel;
using System.ServiceModel.Channels;
using static ApiHerramientaWeb.Modelos.Operaciones.Estructuras.DatosOpe.cablemodems;
using static ApiHerramientaWeb.Modelos.Utils;

namespace ApiHerramientaWeb.Controllers.Cobranza.Facturas
{
    [ApiController]
    [Route("api/[controller]")]
    public class FacturasController : Controller
    {
        private readonly CVGEntities _context;
        private readonly Utils _utils;
        private readonly IConfiguration _configuration;
        private readonly ModemService _estadoModemService;


        public FacturasController(CVGEntities context, IConfiguration configuration, ModemService estadoModemService)
        {
            _context = context;
            _configuration = configuration;
            _utils = new Utils(_context, _configuration);
            _estadoModemService = estadoModemService;          // <- lo guardamos

        }

        [HttpGet("FacturasPendientes")]
        public async Task<IActionResult> GetFacturasPendientes(int cnt)
        {
            try
            {
                var resultado = await LoadFactPenAsync(cnt);
                var data = resultado.FirstOrDefault();
                var estado = data?.res?.FirstOrDefault();

                if (estado != null && estado.result)
                {
                    return Ok(new
                    {
                        code = 1,
                        message = estado.mensaje,
                        data = data.lstFactPend
                    });
                }
                else
                {
                    return Ok(new
                    {
                        code = 0,
                        message = estado?.mensaje ?? "No se encontraron datos.",
                        data = new List<FacturasPendientes>()
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    code = 0,
                    message = "Error interno del servidor.",
                    error = ex.Message
                });
            }
        }

        [HttpGet("HistorialFactura")]
        public async Task<IActionResult> GetHistorialFact (int cnt)
        {
            try 
            {
                var historial = await _context.VwHistorialFacturas
                    .Where(h => h.NumeroContrato == cnt)
                    .OrderByDescending(h => h.FechaFactura)
                    .Select(h => new HistorialFactura
                   
                    {
                        Id = h.Id,
                        NumeroContrato = h.NumeroContrato,
                        IdFactura = h.IdFactura,
                        NumeroFactura = h.NumeroFactura,
                        FechaFactura = h.FechaFactura,
                        SubTotal = h.SubTotal,
                        Iva = h.Iva,
                        Total = h.Total,
                        Cliente = h.Cliente,
                        FechaPago = h.FechaP ?? default,
                        Direccion = h.Direccion,
                        Correo1 = h.Correo1,
                        Correo2 = h.Correo2,
                        Codsts = h.Codsts,
                        STDFAC = h.Stdfac,
                        Nomfac = h.Nomfac,
                        Servicios = h.Servicios
                    })
                    .ToListAsync();
                if (historial != null && historial.Any())
                {
                    return Ok(new
                    {
                        code = 1,
                        message = "Historial de facturas encontrado.",
                        data = historial
                    });
                }
                else
                {
                    return Ok(new
                    {
                        code = 0,
                        message = "No se encontraron datos en el historial de facturas.",
                        data = new List<HistorialFactura>()
                    });
                }


            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    code = 0,
                    message = "Error interno del servidor.",
                    error = ex.Message
                });
            }
        }

        #region FuncionFacturasPendientes
        private async Task<List<DataResult.ListaFactPend>> LoadFactPenAsync(int cnt)
        {
            List<DataResult.ListaFactPend> resp = new();
            List<Resultado> resul = new();
            List<FacturasPendientes> facPen = new();

            // 1. Consulta principal (factura)
            var factura = await (from fact in _context.VwFacturasWs
                                 join cnts in _context.VwContratosWs on fact.Idcontrato equals cnts.Idcontrato
                                 where cnts.NumeroContrato == cnt
                                 orderby fact.FechaVencimiento descending
                                 select new
                                 {
                                     cnts.NumeroContrato,  
                                     cnts.Email1,
                                     cnts.Email2, 
                                     cnts.Zona,
                                     cnts.Subzona,
                                     cnts.FacturarA,
                                     cnts.DireccionContrato,
                                     fact.Factura,
                                     fact.Serie,
                                     fact.NumeroFactura,
                                     fact.Periodo,
                                     fact.SaldoLocal,
                                     fact.SaldoDolar,
                                     fact.Colector,
                                     fact.FechaVencimiento,
                                     cnts.Diapag,
                                     cnts.Sucursal,
                                     fact.DescripcionEstado,
                                     fact.DescripcionPago,
                                     fact.DescripcionTipo,
                                     fact.Idfactura,
                                     fact.CodPago,
                                     cnts.EstadoContrato,
                                     cnts.Celcli,
                                     cnts.Idcontrato
                                 }).FirstOrDefaultAsync();

            if (factura == null)
            {
                resul.Add(new Resultado()
                {
                    codMes = 1,
                    result = false,
                    mensaje = "No se encontraron facturas pendientes, si tiene cualquier duda contactarse con su respectiva sucursal de Casavisión.",
                });

                resp.Add(new DataResult.ListaFactPend()
                {
                    res = resul,
                    lstFactPend = facPen
                });

                return resp;
            }

            if (factura.EstadoContrato == "Cortado")
            {
                resul.Add(new Resultado()
                {
                    codMes = 0,
                    result = false,
                    mensaje = $"Contrato en estado: {factura.EstadoContrato}",
                });

                resp.Add(new DataResult.ListaFactPend()
                {
                    res = resul,
                    lstFactPend = facPen // estará vacío
                });

                return resp;
            }

            resul.Add(new Resultado()
            {
                codMes = 1,
                result = true,
                mensaje = "Éxito: Factura encontrada.",
            });

            // 2. Consultas a la base de datos (todas secuenciales)
            int sucursalId = await _utils.getIdSucursal(cnt);

            var servicios = await (
                from detfact in _context.VwFacturasServiciosWs
                join srv in _context.VwServiciosWs on detfact.Idservicio equals srv.Idservicio
                where detfact.Idfactura == factura.Idfactura
                select srv.Servicio
            ).Take(2).ToArrayAsync();

            var contrato = await _context.Mstcnts
                .Where(c => c.Ideftocnt == factura.NumeroContrato)
                .Select(c => new { c.Latitud, c.Longitud, c.Idtecnologia })
                .FirstOrDefaultAsync();

            var numeroFaja = await _context.Detcnts
                       .Where(c => c.Idecnt == factura.Idcontrato)
                       .Select(c => c.Numfaj)
                       .FirstOrDefaultAsync();

            var idModelo = await _context.Mstequipos
                .Where(c => c.Bodega == factura.NumeroContrato.ToString())
                .Select(c => c.Idmodelo)
                .FirstOrDefaultAsync();

            var modelo = await _context.Mstmodelos
                .Where(m => m.Idmodelo == idModelo)
                .FirstOrDefaultAsync();

            // servicios es un array de string[] con los nombres de los servicios
            bool? suspensionCompleta;
            if (contrato?.Idtecnologia == 1)
            {
                // Si solo hay un servicio y es "TV Basico"
                if (servicios.Length == 1 && servicios[0].Equals("TV Basico", StringComparison.OrdinalIgnoreCase))
                {
                    suspensionCompleta = false;
                }
                else
                {
                    suspensionCompleta = true;
                }
            } else if (contrato?.Idtecnologia == 2)
            {
                // Si solo hay un servicio y es "TV Basico"
                if (servicios.Length == 1 && servicios[0].Equals("TV Basico", StringComparison.OrdinalIgnoreCase))
                {
                    suspensionCompleta = modelo?.SuspensionCompleta;
                }
                else
                {
                    suspensionCompleta = true;
                }
            }
            else
            {
                suspensionCompleta = modelo?.SuspensionCompleta;
            }
            // 3. Llamadas a servicios externos (si NO usan el mismo contexto, puedes paralelizar)
            var estadoDispositivoEnum = await _estadoModemService.ObtenerEstadoModemPorContrato(cnt);
            string estadoDispositivo = estadoDispositivoEnum.ToString();
            string? identificadorEquipo = await _estadoModemService.ObtenerIdentificadorPorContrato(cnt);

            // 4. Construcción del resultado
            facPen.Add(new FacturasPendientes()
            {
                nContrato = factura.NumeroContrato,
                email1 = factura.Email1,
                email2 = factura.Email2,
                zona = factura.Zona,
                subzona = factura.Subzona,
                nombreCli = factura.FacturarA,
                direccionCobro = factura.DireccionContrato,
                facturaCompleta = factura.Factura,
                serie = factura.Serie,
                factura = factura.NumeroFactura,
                periodo = factura.Periodo,
                saldoCor = factura.SaldoLocal,
                saldoDol = factura.SaldoDolar,
                colector = factura.Colector,
                fchVen = factura.FechaVencimiento.ToString("yyyy-MM-dd"),
                sucursal = sucursalId,
                SucursalDesc = factura.Sucursal,
                Diapag = factura.Diapag,
                DescripcionEstado = factura.DescripcionEstado,
                DescripcionTipo = factura.DescripcionTipo,
                DescripcionPago = factura.DescripcionPago,
                CodPago = factura.CodPago,
                Servicios = servicios,
                EstadoDispositivo = estadoDispositivo,
                EstadoContrato = factura.EstadoContrato,
                IdentificadorEquipo = identificadorEquipo,
                Latitud = (decimal)contrato?.Latitud,
                Longitud = (decimal)contrato?.Longitud,
                NumeroFaja = numeroFaja,
                Celcli = factura.Celcli,
                suspensionCompleto = suspensionCompleta ?? false
            });

            resp.Add(new DataResult.ListaFactPend()
            {
                res = resul,
                lstFactPend = facPen
            });

            return resp;
        }


        #endregion


    }
}
