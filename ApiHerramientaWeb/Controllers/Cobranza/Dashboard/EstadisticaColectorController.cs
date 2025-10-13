using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ModeloPrincipal.Entity;
using System.Data;

namespace ApiHerramientaWeb.Controllers.Cobranza.Dashboard
{
    [ApiController]
    [Route("api/[controller]")]
    public class EstadisticaColectorController : Controller
    {
       private readonly IConfiguration _configuration;

        public EstadisticaColectorController(
            IConfiguration configuration
            ) 
        {
            _configuration = configuration;
        }

       #region GetRankingEfectividad

        [HttpGet("GetRankingEfectividad")]
        public async Task<IActionResult> GetRankingEfectividad(
            int IdSucursal,
            CancellationToken cancellationToken = default
            )
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                
                await using var connection = new SqlConnection(connectionString);
               
                await connection.OpenAsync(cancellationToken);

                var parameters = new DynamicParameters();
                parameters.Add("@IdSucursal", IdSucursal, DbType.Int32);

                var rawData = await connection.QueryAsync(
                    sql: "CXC.spRankingEfectividadColectores",
                    param: parameters,
                    commandTimeout: 120,
                    commandType: CommandType.StoredProcedure
                );
                return Ok(rawData);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al obtener el ranking de efectividad", error = ex.Message });
            }
        }

        #endregion

        #region GetUsoCanalPago
        [HttpGet("GetUsoCanalPago")]
        public async Task<IActionResult> GetUsoCanalPago(
            string Usuario,
            CancellationToken cancellationToken = default
            )
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                
                await using var connection = new SqlConnection(connectionString);
               
                await connection.OpenAsync(cancellationToken);
                var parameters = new DynamicParameters();
                parameters.Add("@Usuario", Usuario, DbType.String);
                var rawData = await connection.QueryAsync(
                    sql: "CXC.spReporteUsoCanalesPorColector",
                    param: parameters,
                    commandTimeout: 120,
                    commandType: CommandType.StoredProcedure
                );
                return Ok(rawData);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al obtener el uso de canal de pago", error = ex.Message });
            }
        }
        #endregion

        #region GetGeoposicionCliente
        [HttpGet("GetGeoposicionCliente")]
        public async Task<IActionResult> GetGeoposicionCliente(
            int IdUsuario,
            CancellationToken cancellationToken = default
            )
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                
                await using var connection = new SqlConnection(connectionString);
               
                await connection.OpenAsync(cancellationToken);
                var parameters = new DynamicParameters();
                parameters.Add("@IdUsuario", IdUsuario, DbType.Int32);
                var rawData = await connection.QueryAsync(
                    sql: "CXC.spReporteGeoposicionCliente",
                    param: parameters,
                    commandTimeout: 120,
                    commandType: CommandType.StoredProcedure
                );
                return Ok(rawData);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al obtener la geoposición del cliente", error = ex.Message });
            }
        }
        #endregion

    }
}
