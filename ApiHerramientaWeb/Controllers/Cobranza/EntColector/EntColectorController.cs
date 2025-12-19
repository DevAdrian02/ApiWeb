using ApiHerramientaWeb.Modelos.Cobranza.FacturaColector;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Diagnostics;
using System.Threading;

[ApiController]
[Route("api/[controller]")]
public class EntColectorController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EntColectorController> _logger;

    public EntColectorController(
        IConfiguration configuration,
        ILogger<EntColectorController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    #region FacturaColector

    [HttpGet("FacturaColector")]
    public async Task<IActionResult> GetContractsWithPerfil(
        int idUsuario,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Consultando base de datos para usuario {UserId}...", idUsuario);

            var stopwatch = Stopwatch.StartNew();
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            var parameters = new DynamicParameters();
            parameters.Add("@IdUsuario", idUsuario, DbType.Int32);

            var rawData = await connection.QueryAsync<FacturaColectorRaw>(
                "CXC.spObtenerFacturasColector",
                parameters,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 120
            );


            var result = rawData?.Select(item => new FacturaColectorResponse
            {
                id = item.IDContrato,
                contrato = item.NoContrato ?? 0,
                email1 = item.EMAIL1 ?? string.Empty,
                email2 = item.EMAIL2 ?? string.Empty,
                name = item.NOMFAC ?? string.Empty,
                total = item.SALDO ?? 0m,
                COD_STS = !string.IsNullOrEmpty(item.EstadoPagoFactura) ? item.EstadoPagoFactura : "S",
                paymentDay = item.DiaPago ?? 0,
                invoiceDate = item.FECHA ?? DateTime.MinValue,
                primeraMensualidad = Convert.ToBoolean(item.PrimeraMensualidad ?? false),
                sucursal = item.Sucursal ?? "N/A",
                idsucursal = item.IDSucursal,
                faja = item.NumeroFaja ?? "N/A",
                factura = item.Factura ?? "N/A",
                idZona = item.IDEUBIGEO ?? 0,
                idColector = item.IDColector ?? 0,
                colector = item.Colector ?? string.Empty,
                entregadoPor = string.IsNullOrEmpty(item.EntregadoPor) ? string.Empty : item.EntregadoPor,
                EstadoContrato = string.IsNullOrEmpty(item.ESTADO_CONTRATO) ? string.Empty : item.ESTADO_CONTRATO,
                zona = new ZonaResponse
                {
                    id = item.IDEUBIGEO ?? 0,
                    nombre = item.Zona ?? "Desconocido"
                },
                suspensionCompleto = Convert.ToBoolean(item.SuspensionCompleta ?? false)
            }).ToList() ?? new List<FacturaColectorResponse>();


            _logger.LogInformation("Consulta exitosa para usuario {UserId}. Tiempo: {Elapsed} ms",
                idUsuario, stopwatch.ElapsedMilliseconds);

            return Ok(result);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Operación cancelada para usuario {UserId}", idUsuario);
            return StatusCode(499, new { Message = "Solicitud cancelada por el cliente" });
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "Error SQL [{ErrorNumber}] para usuario {UserId}: {Message}",
                sqlEx.Number, idUsuario, sqlEx.Message);

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
                idUsuario, ex.Message);

            return StatusCode(500, new
            {
                Message = "Error interno del servidor",
                Details = ex.Message
            });
        }
    }
    #endregion

    #region EntragaColector
    [HttpGet("EntregaColector")]
    public async Task<IActionResult> GetEntregaColector(
        int idUsuario,
        DateTime fechaInicio,
        DateTime fechaFin,
        CancellationToken cancellationToken = default)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("@IdUsuario", idUsuario, DbType.Int32);
        parameters.Add("@FechaInicio", fechaInicio, DbType.Date);
        parameters.Add("@FechaFin", fechaFin, DbType.Date);

        var rawData = await connection.QueryAsync(
            sql: "CXC.SpEntregaxColector",
            param: parameters,
            commandTimeout: 120,
            commandType: CommandType.StoredProcedure
        );

        return Ok(rawData);
    }
    #endregion


    #region DetalleEntregaColector
    [HttpGet("DetFacturaEntrega")]
    public async Task<IActionResult> DetFacturaEntrega(
        int idEntCol,
        CancellationToken cancellationToken = default)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("@IDEENTCOL", idEntCol, DbType.Int32);

        var rawData = await connection.QueryAsync(
            sql: "CXC.SPdetalleFacturasEntrega",
            param: parameters,
            commandTimeout: 120,
            commandType: CommandType.StoredProcedure
        );

        return Ok(rawData);
    }
    #endregion

    #region CostosEntrega
    [HttpGet("CostosEntrega")]
    public async Task<IActionResult> CostosEntrega(
        int idEntCol,
        CancellationToken cancellationToken = default)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("@IDEENTCOL", idEntCol, DbType.Int32);

        var rawData = await connection.QueryAsync(
            sql: "CXC.spObtenerCostoServiciosPorEntrega",
            param: parameters,
            commandTimeout: 120,
            commandType: CommandType.StoredProcedure
        );

        return Ok(rawData);
    }
    #endregion


}