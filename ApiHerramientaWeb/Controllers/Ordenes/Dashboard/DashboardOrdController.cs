using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ApiHerramientaWeb.Controllers.Ordenes.Dashboard
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardOrdController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public DashboardOrdController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public class TicketRaw
        {
            public int IDETEC { get; set; }
            public string NOMBRE { get; set; } = string.Empty;
            public string Placa { get; set; } = string.Empty;
            public int CONTRATO { get; set; }
            public string CLIENTE { get; set; } = string.Empty;
            public double? Latitud { get; set; }
            public double? Longitud { get; set; }
            public string Tickets { get; set; } = string.Empty;
        }

        public class OrdenDto
        {
            public int CONTRATO { get; set; }
            public string CLIENTE { get; set; } = string.Empty;
            public double? Latitud { get; set; }
            public double? Longitud { get; set; }
            public List<string> Tickets { get; set; } = new();
        }

        public class TecnicoDto
        {
            public int IDETEC { get; set; }
            public string NOMBRE { get; set; } = string.Empty;
            public string Placa { get; set; } = string.Empty;
            public List<OrdenDto> Ordenes { get; set; } = new();
        }

        [HttpGet("GetDashboardOrd")]
        public async Task<IActionResult> GetDashboardOrd(int idSuc, CancellationToken cancellationToken = default)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                await using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync(cancellationToken);

                var parameters = new DynamicParameters();
                parameters.Add("@IDESUC", idSuc, DbType.Int32);

                var rawData = await connection.QueryAsync<TicketRaw>(
                    "ORD.spObtenerTicketsPorSucursal",
                    parameters,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 60
                );

                // Agrupar por técnico y mapear a DTO más limpio
                var grouped = rawData
                    .AsParallel()
                    .GroupBy(r => r.IDETEC)
                    .Select(g => new TecnicoDto
                    {
                        IDETEC = g.Key,
                        NOMBRE = g.First().NOMBRE,
                        Placa = g.First().Placa,
                        Ordenes = g.Select(x => new OrdenDto
                        {
                            CONTRATO = x.CONTRATO,
                            CLIENTE = x.CLIENTE,
                            Latitud = x.Latitud,
                            Longitud = x.Longitud,
                            Tickets = x.Tickets?
                                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(t => t.Trim())
                                .ToList() ?? new List<string>()
                        }).ToList()
                    })
                    .OrderBy(t => t.NOMBRE)
                    .ToList();

                // Enviar respuesta estandarizada (útil para frontend)
                return Ok(new
                {
                    success = true,
                    totalTecnicos = grouped.Count,
                    data = grouped
                });
            }
            catch (OperationCanceledException)
            {
                return StatusCode(499, new { success = false, message = "Solicitud cancelada por el cliente" });
            }
            catch (SqlException sqlEx)
            {
                return StatusCode(503, new { success = false, message = "Error en la base de datos", details = sqlEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error interno del servidor", details = ex.Message });
            }
        }
    }
}
