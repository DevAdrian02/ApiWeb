using ApiHerramientaWeb.Modelos;
using ApiHerramientaWeb.Modelos.Result;
using Microsoft.EntityFrameworkCore;
using ModeloPrincipal.Entity;
using System.Diagnostics.Contracts;
using static ApiHerramientaWeb.Modelos.Operaciones.Estructuras.DatosOpe.cablemodems;

namespace ApiHerramientaWeb.Services
{
    public class DesactivarDispositivoService: IDesactivarDispositivoService
    {

        private readonly ModemService _estadoModemService;
        private readonly KrillService _krillService;
        private readonly SmartOltService _smartOltService;
        private readonly Utils _utils;
        private readonly CVGEntities _context;
        private readonly MoviTvService _moviTvServices;




        public DesactivarDispositivoService(
        CVGEntities context,
        ModemService estadoModemService,
        KrillService krillService,
        SmartOltService smartOltService,
        MoviTvService moviTvServices,

        Utils utils)
        {
            _context = context;
            _estadoModemService = estadoModemService;
            _krillService = krillService;
            _smartOltService = smartOltService;
            _utils = utils;
            _moviTvServices = moviTvServices;

        }

        public async Task<DesactivarResultModels> DesactivarCmInternoAsync(DesactivarCmRequest request, string ip)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var query = await _estadoModemService.ObtenerQuerySuspencion(request.Ideftocnt);
                var contratosConMoviTv = query.Where(c => c.id_servicio == 5).ToList();

                var suspesion = query.FirstOrDefault();

                if (suspesion == null)
                    return new DesactivarResultModels { Success = false, Mensaje = "Contrato no cuenta con factura pendiente" };

                //var resultado = _estadoModemService.ValidarDiaPagoSuspension(suspesion.DiaPago);
                //if (resultado.codigo != 0)
                //    return new DesactivarResultModels { Success = false, Mensaje = resultado.mensaje };




                var tipo_tecnologia = suspesion.IDTECNOLOGIA;
                var activeIntegrations = await _context.Mstintegracions
                    .Where(i => i.Idtecnologia == tipo_tecnologia && i.Activa == true)
                    .Select(i => i.Idintegracion)
                    .ToListAsync();



                if (contratosConMoviTv.Any())
                {
                    foreach (var contrato in contratosConMoviTv)
                    {
                        try
                        {
                            var partnerId = contrato.CONTRATO.ToString();

                            // Llamada a MoviTvService para suspender o desactivar
                            await _moviTvServices.DesactivarAsync(partnerId);

                            Console.WriteLine($"Usuario MoviTV {partnerId} suspendido correctamente.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error al suspender MoviTV para {contrato.CONTRATO}: {ex.Message}");
                        }
                    }
                }

                if (activeIntegrations.Count > 0 && suspesion.ActivacionColector == 0 && suspesion.Aprovisiona == true)
                {
                    foreach (var integracion_ in activeIntegrations)
                    {
                        switch (integracion_)
                        {
                            case 4:
                                await _krillService.DesactivarAsync(suspesion.COD_SUC, suspesion.REALM);
                                break;
                            case 2:
                                await _smartOltService.DesactivarAsync(suspesion.COD_SUC);
                                break;
                        }
                    }
                }

                var usuario = await _utils.ObtenerCodigoUsuarioPorIdAsync(request.iduser);
                if (string.IsNullOrEmpty(usuario))
                    return new DesactivarResultModels { Success = false, Mensaje = "Usuario no encontrado." };

                var (success, error) = await _estadoModemService.RegistrarSuspensionAsync(
                    suspesion.IDCONTRATO,
                    request.Comentario,
                    usuario,
                    ip,
                    suspesion.CONTRATO.ToString()
                );

                if (!success)
                    return new DesactivarResultModels { Success = false, Mensaje = error };

                await transaction.CommitAsync();
                return new DesactivarResultModels { Success = true, Mensaje = "Suspensión exitosa" };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new DesactivarResultModels { Success = false, Mensaje = ex.Message };
            }
        }


    }
}
