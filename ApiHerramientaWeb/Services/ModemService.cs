using Microsoft.EntityFrameworkCore;
using ModeloPrincipal.Entity;
using static ApiHerramientaWeb.Modelos.Operaciones.Estructuras.DatosOpe.cablemodems;

namespace ApiHerramientaWeb.Services
{
    public class ModemService : IEstadoModemService
    {
        public readonly CVGEntities _context;
        public readonly KrillService _krillService;
        public readonly SmartOltService _smartOltService;

        public ModemService(CVGEntities context, KrillService krillService, SmartOltService smartOltService)
        {
            _context = context;
            _krillService = krillService;
            _smartOltService = smartOltService;
        }


        #region Centralización de consulta de contrato
        public async Task<List<query_contrato>> ObtenerQueryContrato(int? numero_contrato)
        {
            if (!numero_contrato.HasValue)
                return new List<query_contrato>();

            return await _context.VwContratoActivars
                .Where(c => c.Ideftocnt == numero_contrato)
                .Select(c => new query_contrato
                {
                    IDCONTRATO = c.Idcontrato,
                    IDTECNOLOGIA = c.Idtecnologia ?? 0,
                    TENENCIA = c.Tenencia,
                    SERVICIO = c.Servicio,
                    CONTRATO = c.Contrato,
                    ESTADO_CONTRATO_NUM = c.EstadoContratoNum ?? 0,
                    ESTADO_CONTRATO = c.EstadoContrato,
                    NOMBRE = c.Nombre,
                    DIRECCION = c.Direccion,
                    DICMACCM = c.Dicmacmm,
                    SUCURSAL = c.Sucursal,
                    COD_SUC = c.CodSuc,
                    LATITUD = (double?)(c.Latitud ?? 0),
                    LONGITUD = (double?)(c.Longitud ?? 0),
                    id_servicio = c.IdServicio,
                    PRIMARIO = c.Primario ?? false,
                    REALM = c.Realm,
                    ID_SUCURSAL = c.IdSucursal,
                    IDPerfil = c.Idperfil ?? 0,
                    Marca = c.Marca,
                    Modelo = c.Modelo,
                    Faja = c.Faja,
                    suspensionCompleto = c.SuspensionCompleta ?? false

                }).ToListAsync();
        }
        #endregion

        #region Centralización de consulta de suspension
        public async Task<List<query_suspesion>> ObtenerQuerySuspencion(int? numero_contrato)
        {
            if (!numero_contrato.HasValue)
                return new List<query_suspesion>();

            return await _context.VwNuevaSuspensiones
                .Where(c => c.NoContrato == numero_contrato)
                .Select(c => new query_suspesion
                {
                    IDCONTRATO = c.Idcontrato,
                    IDTECNOLOGIA = c.Idtecnologia,
                    TENENCIA = c.Tenencia,
                    SERVICIO = c.Servicio,
                    CONTRATO = c.NoContrato,
                    ESTADO_CONTRATO_NUM = c.EstadoContratoNum,
                    ESTADO_CONTRATO = c.EstadoContrato,
                    NOMBRE = c.Nombre,
                    DIRECCION = c.Direccion,
                    DICMACCM = c.Dicmaccm,
                    SUCURSAL = c.Sucursal,
                    COD_SUC = c.CodSuc,
                    LATITUD = (double?)c.Latitud,
                    LONGITUD = (double?)c.Longitud,
                    id_servicio = c.IdServicio,
                    PRIMARIO = c.Primario,
                    REALM = c.Realm,
                    ID_SUCURSAL = c.Idsucursal,
                    IDPerfil = c.Idperfil,
                    Marca = c.Marca,
                    Modelo = c.Modelo,
                    Faja = c.Faja,
                    Factura = c.Factura,
                    FECHA = c.Fecha.HasValue ? c.Fecha.Value.ToDateTime(TimeOnly.MinValue) : DateTime.MinValue,
                    SALDO = c.Saldo,
                    DiaPago = c.DiaPago,
                    PrimeraMensualidad = c.PrimeraMensualidad,
                    Aprovisiona = c.Aprovisiona,
                    ActivacionColector = c.ActivacionColector
                }).ToListAsync();
        }
        #endregion



        public async Task<EstadoModem> ObtenerEstadoModemPorContrato(int numeroContrato)
        {
            var query = await ObtenerQueryContrato(numeroContrato);

            if (!query.Any())
                return  EstadoModem.Inactivo; // Devuelve SinDato si no hay información

            var tipoTecnologia = query.FirstOrDefault()?.IDTECNOLOGIA ?? 0;
            var internet = query.Any(o => o.id_servicio == 3);
      
            var activeIntegrations = await _context.Mstintegracions
                .Where(i => i.Idtecnologia == tipoTecnologia && i.Activa == true)
                .Select(i => i.Idintegracion)
                .ToListAsync();


            

            EstadoModem estadoModem = EstadoModem.Inactivo;

            if (!query.Any(o => o.id_servicio == 3))
            {
                estadoModem = EstadoModem.SoloTV;

            }

            foreach (var integracion in activeIntegrations)
            {
                if (internet)
                {
                    switch (integracion)
                    {
                        case 4:
                            try
                            {
                                var krillData = await _krillService.GetEstadoModemAsync(
                                    query.FirstOrDefault()?.COD_SUC,
                                    query.FirstOrDefault()?.REALM
                                );
                                if (krillData == null)
                                {
                                    // Aquí puedes registrar el error o mostrarlo según tu necesidad
                                    Console.WriteLine("Error: Respuesta nula de KrillService.");
                                    estadoModem = EstadoModem.Inactivo;
                                }
                                else
                                {
                                    estadoModem = krillData.estadoModem;
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error al consultar KrillService: {ex.Message}");
                                estadoModem = EstadoModem.Inactivo;
                            }
                            break;

                        case 2:
                            try
                            {
                                var smartOltData = await _smartOltService.GetEstadoModemAsync(
                                    query.FirstOrDefault()?.COD_SUC);
                                if (smartOltData == null)
                                {
                                    Console.WriteLine("Error: Respuesta nula de SmartOltService.");
                                    estadoModem = EstadoModem.Inactivo;
                                }
                                else
                                {
                                    estadoModem = smartOltData.estadoModem;
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error al consultar SmartOltService: {ex.Message}");
                                estadoModem = EstadoModem.Inactivo;
                            }
                            break;

                    }
                }
                else
                {
                    estadoModem = EstadoModem.SoloTV;
                    estadoModem = EstadoModem.Activo;
                     
                }
            }

            return estadoModem;
        }

        public async Task<string?> ObtenerIdentificadorPorContrato(int numeroContrato)
        {
            // Consulta para obtener el identificador del contrato
            var identificador = await _context.VwObtenerEquipoContratos
                .Where(c => c.Contrato == numeroContrato)
                .Select(c => c.IdentificadorEquipo)
                .FirstOrDefaultAsync();

            return identificador;
        }

        #region Métodos de Soporte

        #region ValidarDiaPago
        public (int codigo, string mensaje) ValidarDiaPagoSuspension(int? diaPago)
        {
            int hoy = DateTime.Now.Day;
            int horaActual = DateTime.Now.Hour;
            int mesActual = DateTime.Now.Month;
            int anioActual = DateTime.Now.Year;
            int ultimoDiaMesPago = DateTime.DaysInMonth(anioActual, mesActual);
            int diaPagoInt = diaPago ?? 0;
            int diaPagoEfectivo = diaPagoInt > ultimoDiaMesPago ? ultimoDiaMesPago : diaPagoInt;

            if (hoy < diaPagoEfectivo)
                return (1, $"No es posible suspender el servicio antes del día de pago ({diaPago}).");

            if (hoy == diaPagoEfectivo)
            {
                if (hoy == ultimoDiaMesPago && horaActual <= 12)
                    return (2, "No es posible suspender el servicio en este momento. Por favor, intente más tarde.");

                return (3, $"No es posible suspender el servicio el mismo día de pago ({diaPago}). La suspensión estará disponible a partir del día siguiente.");
            }

            return (0, "Suspensión permitida.");
        }
        #endregion

        #region RegistrarSuspension
        public async Task<List<Detcnt>> ObtenerDetallesContratoAsync(int idContrato)
        {
            return await _context.Detcnts
                .Where(d => d.Idecnt == idContrato)
                .ToListAsync();
        }

        public async Task<bool> ExisteSuspensionEnMesAsync(int idContrato, DateTime primerDiaMes, DateTime ultimoDiaMes)
        {
            return await _context.Mstspdidos
                .AnyAsync(sp => sp.Idecnt == idContrato
                             && sp.Fchspdido >= primerDiaMes
                             && sp.Fchspdido <= ultimoDiaMes
                             && sp.Idestatra == 24);
        }

        public async Task<decimal> ObtenerSaldoUltimaFacturaAsync(int idContrato)
        {
            return await _context.Mstfacs
                .Where(f => f.Idecnt == idContrato)
                .OrderByDescending(f => f.Crefch)
                .Select(f => f.Sdofacloc)
                .FirstOrDefaultAsync();
        }

        public async Task<Mstcnt?> ObtenerContratoAsync(int idContrato)
        {
            return await _context.Mstcnts
                .FirstOrDefaultAsync(o => o.Idecnt == idContrato);
        }

        public async Task<(bool success, string error)> RegistrarSuspensionAsync(
            int idContrato,
            string comentario,
            string usuario,
            string ip,
            string contratoNumero)
        {
            var detallesContrato = await ObtenerDetallesContratoAsync(idContrato);
            if (!detallesContrato.Any())
                return (false, "No se encontraron detalles del contrato.");

            var contrato = await ObtenerContratoAsync(idContrato);
            var servicioContrato = await _context.VwNuevaSuspensiones
                .Where(o => o.Idcontrato == idContrato)
                .ToListAsync();

            var serviciosActivos = servicioContrato
                .Where(o => o.EstadoContratoNum == 1)
                .ToList();

            if (!serviciosActivos.Any())
                return (false, "El contrato ya está cortado y no puede ser suspendido");

            bool alMenosUnoSuspendido = false;

            foreach (var servicio in serviciosActivos)
            {
                var primerDiaMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var ultimoDiaMes = primerDiaMes.AddMonths(1).AddDays(-1);

                if (await ExisteSuspensionEnMesAsync(idContrato, primerDiaMes, ultimoDiaMes))
                    return (false, "Ya existe una suspensión agendada para este contrato en el mes actual.");

                var saldoFactura = await ObtenerSaldoUltimaFacturaAsync(idContrato);
                string noDocumento = $"{DateTime.Now:yyyyMMdd}-{contratoNumero}";

                var nuevoSpdido = new Mstspdido
                {
                    Idecnt = idContrato,
                    Idedetcnt = servicio.IddetalleContrato, // Usar el detalle del servicio activo
                    Idecia = 1,
                    Codesttkt = "00070",
                    Observ = comentario,
                    Fchspdido = DateTime.Now,
                    Mtopdente = saldoFactura,
                    Dscusrreg = usuario,
                    Dscusrauto = "",
                    Idestatra = 24,
                    Totlin = 0,
                    Creusr = usuario,
                    Modusr = usuario,
                    Crefch = DateTime.Now,
                    Modfch = DateTime.Now,
                    Mesfac = DateTime.Now.Month,
                    Yerfac = DateTime.Now.Year,
                    Creips = ip,
                    Modips = ip,
                    Crehsn = System.Net.Dns.GetHostName(),
                    Modhsn = System.Net.Dns.GetHostName(),
                    Nodocument = noDocumento,
                    Tap = 0,
                    Proceso = 2
                };
                _context.Mstspdidos.Add(nuevoSpdido);

                // Solo modificar el detalle correspondiente al servicio activo
                var detalle = detallesContrato.FirstOrDefault(d => d.Idedetcnt == servicio.IddetalleContrato);
                if (detalle != null)
                {
                    detalle.Esttec = 32;
                    _context.Entry(detalle).State = EntityState.Modified;
                }

                alMenosUnoSuspendido = true;
            }

            if (contrato != null && alMenosUnoSuspendido)
            {
                contrato.Codestfin = "00020";
                _context.Entry(contrato).State = EntityState.Modified;
            }

            if (alMenosUnoSuspendido)
            {
                await _context.SaveChangesAsync();
                return (true, null);
            }
            else
            {
                return (false, "No se encontró ningún servicio activo para suspender.");
            }
        }
        #endregion

        #endregion
    }
}
