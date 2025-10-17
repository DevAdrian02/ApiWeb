using System.Data;
using ApiHerramientaWeb.Modelos.Cobranza.CanalPago;
using ApiHerramientaWeb.Services;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ModeloPrincipal.Entity;

namespace ApiHerramientaWeb.Controllers.Zona
{
    [ApiController]
    [Route("api/[controller]")]
    public class CanalPagoController : Controller
    {
        private readonly CVGEntities _context;
        private readonly IConfiguration _configuration;
        private readonly OrdenService _repository;


        public CanalPagoController(CVGEntities context, IConfiguration configuration, OrdenService repository)
        {
            _context = context;
            _configuration = configuration;
            _repository = repository;

        }

        [HttpGet("GetCanalPago")]
        public async Task<IActionResult> GetCanalPago()
        {
            try
            {
                var canalespagos = await _context.Mstcanalpagos
                    .Select(m => new
                    {
                        m.Idcanal,
                        m.Descripcion,
                    })
                    .ToListAsync();
                return Ok(canalespagos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al obtener las zonas", error = ex.Message });
            }
        }



        [HttpPost("SaveCanalPago")]
        public async Task<IActionResult> SaveCanalPago([FromBody] CanalPagoDto screenData)
        {
            if (screenData == null)
            {
                return BadRequest(new
                {
                    code = 0,
                    message = "Datos de entrada inválidos.",
                    error = "El cuerpo de la solicitud está vacío o mal formado."
                });
            }

            try
            {
                // 1️⃣ Buscar el usuario en la BD
                var usuario = await _context.Mstusrs
                           .Where(u => u.Ideusr == screenData.iduser)
                           .Select(u => u.Codusr)
                           .FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(usuario))
                {
                    return BadRequest(new
                    {
                        code = 0,
                        message = "Usuario no encontrado.",
                        error = "No existe un usuario con el id proporcionado."
                    });
                }

                // 2️⃣ Registrar en auditoría
                var logAud = new Auditactweb
                {
                    Ideftocnt = screenData.ideftocnt,
                    Idcanal = screenData.idcanal,
                    Codref = screenData.codref,
                    Fchcre = DateTime.Now,
                    Fchappcre = DateTime.Now,
                    Creips = "1",
                    Crehsn = "",
                    Codusrcre = usuario,
                    Appname = "HerramientaWeb3.0"
                };

                _context.Auditactwebs.Add(logAud);
                await _context.SaveChangesAsync();

                // 3️⃣ Cancelar entrega (flujo normal)
                await CancelarEntrega(screenData.ideftocnt);

                try
                {
                    using var scope = HttpContext.RequestServices.CreateScope();
                    var scopedContext = scope.ServiceProvider.GetRequiredService<CVGEntities>();
                    var scopedRepository = scope.ServiceProvider.GetRequiredService<OrdenService>();

                    var idecnt = await scopedContext.Mstcnts
                        .Where(c => c.Ideftocnt == screenData.ideftocnt)
                        .Select(c => c.Idecnt)
                        .FirstOrDefaultAsync();

                    if (idecnt != 0)
                    {
                        var ideticket = await scopedContext.Msttickets
                            .Where(t => t.Codesttkt == "00020" && t.Idecnt == idecnt && t.Ideord == 91)
                            .Select(t => t.Ideticket)
                            .FirstOrDefaultAsync();

                        if (ideticket != 0)
                        {
                            await scopedRepository.AtenderOrdenColectorAsync(
                                numeroContrato: screenData.ideftocnt,
                                numeroOrden: ideticket,
                                userName: usuario,
                                userId: screenData.iduser,
                                observacion: "CIERRE DE VISITA CLIENTE REALIZA PAGO"
                            );
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[AtenderOrden ERROR]: {ex.Message}");
                }


                // ✅ 5️⃣ Retornar flujo normal de SaveCanalPago
                return Ok(new
                {
                    code = 1,
                    message = "Canal de pago guardado correctamente"
                });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new
                {
                    code = 0,
                    message = "Error de base de datos al guardar el canal de pago.",
                    error = dbEx.InnerException?.Message ?? dbEx.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    code = 0,
                    message = "Error inesperado al guardar el canal de pago.",
                    error = ex.Message
                });
            }
        }



        private async Task CancelarEntrega(
            int ideftocnt,
            CancellationToken cancellationToken = default
            )
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");

                await using var connection = new SqlConnection(connectionString);

                await connection.OpenAsync(cancellationToken);

                var parameters = new DynamicParameters();
                parameters.Add("@IDEFTOCNT", ideftocnt, DbType.Int32); 

                await connection.ExecuteAsync(
                    sql: "CXC.SPcancelarFacturaEntrega",
                    param: parameters,
                    commandTimeout: 120,
                    commandType: CommandType.StoredProcedure
                );
                

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }


    }
}
