using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ModeloPrincipal.Entity;
using System.Data;
using ApiHerramientaWeb.Modelos.Venta.ContratoAsignado;

namespace ApiHerramientaWeb.Controllers.Ventas.ContratoAsignado
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContratoAsigController : Controller
    { 
        private readonly IConfiguration _configuration;
        private readonly CVGEntities _context;
        public ContratoAsigController(IConfiguration configuration, CVGEntities context)
        {
            _configuration = configuration;
            _context = context;
        }

        [HttpGet("GetContratoAsignado")]
        public async Task<IActionResult> GetContratoAsignado(
            int idUsuario,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");

                await using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync(cancellationToken);

                var parameters = new DynamicParameters();
                parameters.Add("@IDEUSUARIO", idUsuario, DbType.Int32);

                var result = await connection.QueryAsync(
                    sql: "CLI.spObtenerContratosDisponibles",
                    param: parameters,
                    commandTimeout: 120,
                    commandType: CommandType.StoredProcedure
                );

                // Agrupamos por agente y sucursal, devolviendo lista de números
                var agrupado = result
                    .GroupBy(r => new { r.IDEAGENTE, r.AGENTE })
                    .Select(gAgente => new
                    {
                        ideagente = gAgente.Key.IDEAGENTE,
                        agente = gAgente.Key.AGENTE,
                        sucursales = gAgente
                            .GroupBy(s => new { s.IDESUC, s.SUCURSAL })
                            .Select(gSucursal => new
                            {
                                idesuc = gSucursal.Key.IDESUC,
                                sucursal = gSucursal.Key.SUCURSAL,
                                contratos = gSucursal
                                    .Select(c => (int)c.IDEFTOCNT) // solo los números
                                    .ToList()
                            }).ToList()
                    }).ToList();

                if (!agrupado.Any())
                    return NotFound($"No se encontraron contratos asignados para el usuario {idUsuario}.");

                return Ok(agrupado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener contrato asignado: {ex.Message}");
            }
        }


        [HttpPost("RegistrarVentaGeo")]
        public async Task<IActionResult> RegistrarVentaGeo([FromBody] VentaGeoDto dto)
        {
            try
            {
                // Buscar el usuario para obtener el coduser
                var usuario = await _context.Mstusrs
                    .Where(u => u.Ideusr == dto.IDEUSER)
                    .Select(u => u.Codusr)
                    .FirstOrDefaultAsync();

                if (usuario == null)
                    return BadRequest("Usuario no encontrado.");

                // Crear el nuevo registro de venta geolozalizada
                var ventaGeo = new Mstventgeo
                {
                    Ideftocnt = dto.IDEFTOCNT,
                    Iduser = dto.IDEUSER,
                    Latitud = dto.LATITUD,
                    Longitud = dto.LONGITUD,
                    Creusr = usuario,
                    Crefch = DateTime.Now
                };

                _context.Mstventgeos.Add(ventaGeo);
                await _context.SaveChangesAsync();
               
                return Ok(new { code = 1, message = "Venta Geolocalizada correctamente." });


            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al registrar venta: {ex.Message}");
            }
        }



    }
}
