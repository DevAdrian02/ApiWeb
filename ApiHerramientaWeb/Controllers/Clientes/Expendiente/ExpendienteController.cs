using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ModeloPrincipal.Entity;
using System.Data;

namespace ApiHerramientaWeb.Controllers.Clientes.Expendiente
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExpendienteController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly CVGEntities _context;

        public ExpendienteController(IConfiguration configuration, CVGEntities context)
        {
            _configuration = configuration;
            _context = context;
        }


        #region GetExpendientexContrato
        [HttpGet("GetExpendiente")]
        public async Task<IActionResult> GetExpendiente(
            int contrato,
            CancellationToken cancellationToken = default)



        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                await using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync(cancellationToken);
                var parameters = new DynamicParameters();
                parameters.Add("@Contrato", contrato, DbType.Int32);

                using var multi = await connection.QueryMultipleAsync(
                    sql: "CXC.spObtenerDatosExpedienteCliente",
                    param: parameters,
                    commandTimeout: 120,
                    commandType: CommandType.StoredProcedure
                );

                var tabla1 = await multi.ReadAsync();
                var tabla2 = await multi.ReadAsync();
                var tabla3 = await multi.ReadAsync();
                var tabla4 = await multi.ReadAsync();
                var tabla5 = await multi.ReadAsync();

                return Ok(new
                {
                    InformacionGeneralContrato = tabla1,
                    ServiciosActivos = tabla2,
                    HistorialOtrosPagos = tabla3,
                    HistorialMensualidades = tabla4,
                    HistorialOrdenes = tabla5
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al obtener el expendiente", error = ex.Message });
            }
        }
        #endregion

        #region GetContratoXNombre

        [HttpGet("BuscarContrato")]
        public async Task<IActionResult> BuscarContrato(
            string nombre,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                await using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync(cancellationToken);
                var parameters = new DynamicParameters();
                parameters.Add("@Nombre", nombre, DbType.String);

                var resultados = await connection.QueryAsync(
                    sql: "CXC.spBuscarContratosPorNombre",
                    param: parameters,
                    commandType: CommandType.StoredProcedure
                );

                return Ok(resultados);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al buscar contratos", error = ex.Message });
            }
        }
        #endregion

        #region ActualizarInformacion

        [HttpPost("UpdateInformacion")]
        public async Task<IActionResult> UpdateInformacion([FromBody] UpdateInformacionRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var contrato = await _context.Mstcnts
                    .Where(c => c.Ideftocnt == request.Cpm)
                    .FirstOrDefaultAsync();
                if (contrato == null)
                {
                    return NotFound(new { code = 0, message = "Contrato no encontrado." });
                }

                contrato.Dircob010 = request.CpmText;
                _context.Mstcnts.Update(contrato);


                var cliente = await _context.Mstclis
                    .Where(c => c.Idecli == contrato.Idecli)
                    .FirstOrDefaultAsync();
                if (cliente == null)
                {
                    return NotFound(new { code = 0, message = "Cliente no encontrado." });
                }

                cliente.Celcli = request.Celcli;
                cliente.Dircli010 = request.CpmText;
                cliente.Email1 = request.Email; 

                _context.Mstclis.Update(cliente);


                await _context.SaveChangesAsync();

                return Ok(new { code = 1, message = "Datos actualizados correctamente." });

            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al actualizar la información", error = ex.Message });
            }
        }

        public class UpdateInformacionRequest
        {
            public int Cpm { get; set; }         // Contrato
            public string CpmText { get; set; }  // Nueva dirección
            public string Celcli { get; set; }   // Nuevo celular
            public string Email { get; set; }    // Nuevo email 

        }

        #endregion




    }
}
