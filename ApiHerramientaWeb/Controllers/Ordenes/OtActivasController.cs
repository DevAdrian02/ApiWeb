using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using ModeloPrincipal.Entity;
using System.Data;

namespace ApiHerramientaWeb.Controllers.Ordenes
{
    public class OrdenesActivasRequest
    {
        public int idUsuario { get; set; }
        public int idSucursal { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class OtActivasController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;
        private readonly ILogger<OtActivasController> _logger;

        private static readonly Dictionary<string, SemaphoreSlim> _userSemaphores = new();

        public OtActivasController(
            CVGEntities context,
            IConfiguration configuration,
            IMemoryCache cache,
            ILogger<OtActivasController> logger
        )
        {
            _configuration = configuration;
            _cache = cache;
            _logger = logger;
        }

        [HttpPost("OrdenesActivasPorSucursal")]
        public async Task<IActionResult> GetOrdenesActivasPorSucursal(
            [FromBody] OrdenesActivasRequest request,
            CancellationToken cancellationToken = default)
        {
            var semaphoreKey = $"OrdenesActivasPorSucursal_{request.idUsuario}";
            bool semaphoreAcquired = false;
            SemaphoreSlim userSemaphore;

            try
            {
                lock (_userSemaphores)
                {
                    if (!_userSemaphores.TryGetValue(semaphoreKey, out userSemaphore))
                    {
                        userSemaphore = new SemaphoreSlim(1, 1);
                        _userSemaphores[semaphoreKey] = userSemaphore;
                    }
                }

                semaphoreAcquired = await userSemaphore.WaitAsync(TimeSpan.FromSeconds(30), cancellationToken);

                if (!semaphoreAcquired)
                {
                    _logger.LogWarning("Timeout al adquirir semáforo para {SemaphoreKey}", semaphoreKey);
                    return StatusCode(503, new { Message = "El servicio está ocupado, intente nuevamente más tarde" });
                }

                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                await using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync(cancellationToken);

                var parameters = new DynamicParameters();
                parameters.Add("@idSucursal", request.idSucursal, DbType.Int32);

                var rawData = await connection.QueryAsync(
                    sql: "ORD.spObtenerOtActivasPorSucursal",
                    param: parameters,
                    commandTimeout: 120,
                    commandType: CommandType.StoredProcedure
                );

                var result = rawData.ToList();

                return Ok(result);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Operación cancelada para usuario {UserId}", request.idUsuario);
                return StatusCode(499, new { Message = "Solicitud cancelada por el cliente" });
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError(sqlEx, "Error SQL [{ErrorNumber}] para usuario {UserId}: {Message}",
                    sqlEx.Number, request.idUsuario, sqlEx.Message);

                return StatusCode(503, new
                {
                    Code = sqlEx.Number,
                    Message = "Error temporal en la base de datos",
                    Details = sqlEx.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Error crítico procesando usuario {UserId}: {Message}",
                    request.idUsuario, ex.Message);

                return StatusCode(500, new
                {
                    Message = "Error interno del servidor",
                    Details = ex.Message
                });
            }
            finally
            {
                if (semaphoreAcquired)
                {
                    if (_userSemaphores.TryGetValue(semaphoreKey, out var userSemaphoreFinal))
                    {
                        userSemaphoreFinal.Release();
                        _logger.LogDebug("Semáforo liberado para {SemaphoreKey}", semaphoreKey);
                    }
                }
            }
        }

        [HttpPost("UltimaOrdenActiva")]
        public async Task<IActionResult> GetUltimaOrdenActiva(
       [FromBody] OrdenesActivasRequest request,
       CancellationToken cancellationToken = default)
        {
            string cacheKey = $"UltimaOrden_{request.idSucursal}";

            if (_cache.TryGetValue(cacheKey, out object cachedOrder))
            {
                return Ok(cachedOrder);
            }

            try
            {
                await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                await connection.OpenAsync(cancellationToken);

                var parameters = new DynamicParameters();
                parameters.Add("@idSucursal", request.idSucursal, DbType.Int32);

                var order = await connection.QueryFirstOrDefaultAsync<object>(
                    sql: "ORD.spObtenerUltimaOrdenActiva",
                    param: parameters,
                    commandTimeout: 10,
                    commandType: CommandType.StoredProcedure
                );

                if (order != null)
                {
                    _cache.Set(cacheKey, order, TimeSpan.FromSeconds(5));
                    return Ok(order);
                }

                return NotFound(new { Message = "No hay órdenes activas recientes" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo última orden para sucursal {SucursalId}", request.idSucursal);
                return StatusCode(500, new
                {
                    Message = "Error al obtener última orden",
                    Details = ex.Message
                });
            }
        }


    }

}
