using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using ApiHerramientaWeb.Modelos.Ordenes;

namespace ApiHerramientaWeb.Services
{
    public class OrdenService
    {
        private readonly string _connectionString;

        public OrdenService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");


        }

        public async Task<List<string>> CrearOrdenColectorAsync(
     int idecnt,
     int idetecnico,
     int idcuadrilla,
     string usuario)
        {
            var mensajes = new List<string>();

            using var connection = new SqlConnection(_connectionString);

            // Captura los PRINT del SP
            connection.InfoMessage += (sender, e) =>
            {
                mensajes.Add(e.Message);
            };

            using var command = new SqlCommand("ORD.spOrdenAperturaColector", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@IDECNT", idecnt);
            command.Parameters.AddWithValue("@IDETECASG", idetecnico);
            command.Parameters.AddWithValue("@IDCUADRILLA", idcuadrilla);
            command.Parameters.AddWithValue("@Usuario", usuario);

            await connection.OpenAsync();

            try
            {
                await command.ExecuteNonQueryAsync();
            }
            catch (SqlException ex)
            {
                mensajes.Add($"Error SQL: {ex.Message}");
            }
            catch (Exception ex)
            {
                mensajes.Add($"Error general: {ex.Message}");
            }

            return mensajes;
        }



        //creacion de orden de rx de tap
        public async Task<List<string>> CrearOrdenReconexionTapNapAsync(
    int idecnt,
    int idetecnico,
    int idcuadrilla,
    string usuario)
        {
            var mensajes = new List<string>();

            using var connection = new SqlConnection(_connectionString);

            // Captura los mensajes PRINT del SP (Ej: "Orden creada..." o "Servicio omitido...")
            connection.InfoMessage += (sender, e) =>
            {
                mensajes.Add(e.Message);
            };

            // Apuntamos al nuevo SP que acabamos de crear
            using var command = new SqlCommand("ORD.spOrdenReconexionTapNap", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Los parámetros coinciden con los del script SQL
            command.Parameters.AddWithValue("@IDECNT", idecnt);
            command.Parameters.AddWithValue("@IDETECASG", idetecnico);
            command.Parameters.AddWithValue("@IDCUADRILLA", idcuadrilla);
            command.Parameters.AddWithValue("@Usuario", usuario);

            await connection.OpenAsync();

            try
            {
                await command.ExecuteNonQueryAsync();
            }
            catch (SqlException ex)
            {
                mensajes.Add($"Error SQL: {ex.Message}");
            }
            catch (Exception ex)
            {
                mensajes.Add($"Error general: {ex.Message}");
            }

            return mensajes;
        }


        public async Task<List<string>> CrearOrdenDesconexionAsync(int idecnt, int idetecasg, int idcuadrilla, string usuario)
        {
            var mensajes = new List<string>();

            using var connection = new SqlConnection(_connectionString);

            // Captura los mensajes PRINT del SP
            connection.InfoMessage += (sender, e) =>
            {
                mensajes.Add(e.Message);
            };

            using var command = new SqlCommand("ORD.spOrdenAperturarDesconexion", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Parámetros del SP
            command.Parameters.AddWithValue("@IDECNT", idecnt);
            command.Parameters.AddWithValue("@IDETECASG", idetecasg);
            command.Parameters.AddWithValue("@IDCUADRILLA", idcuadrilla);
            command.Parameters.AddWithValue("@Usuario", usuario);

            await connection.OpenAsync();

            try
            {
                await command.ExecuteNonQueryAsync();
            }
            catch (SqlException ex)
            {
                mensajes.Add($"Error SQL: {ex.Message}");
            }
            catch (Exception ex)
            {
                mensajes.Add($"Error general: {ex.Message}");
            }

            return mensajes;
        }



        public async Task<(int ResultCode, string ResultMessage)> AtenderOrdenColectorAsync(
       int numeroContrato,
       int numeroOrden,
       string userName,
       int userId,
       string observacion,
       string numFaj = "")
        {
            try
            {
                await using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new DynamicParameters();
                parameters.Add("@NumeroContrato", numeroContrato, DbType.Int32);
                parameters.Add("@NumeroOrden", numeroOrden, DbType.Int32);
                parameters.Add("@UserName", userName, DbType.String);
                parameters.Add("@UserId", userId, DbType.Int32);
                parameters.Add("@Observacion", observacion, DbType.String);
                parameters.Add("@NUMFAJ", numFaj, DbType.String);

                // Parámetros de salida
                parameters.Add("@ResultCode", dbType: DbType.Int32, direction: ParameterDirection.Output);
                parameters.Add("@ResultMessage", dbType: DbType.String, size: 4000, direction: ParameterDirection.Output);

                await connection.ExecuteAsync(
                    sql: "ORD.spAtenderOrdenTecnico",
                    param: parameters,
                    commandType: CommandType.StoredProcedure
                );

                // Obtener valores de salida
                int resultCode = parameters.Get<int>("@ResultCode");
                string resultMessage = parameters.Get<string>("@ResultMessage");

                return (resultCode, resultMessage);
            }
            catch (Exception ex)
            {
                // Podés manejar errores de forma más específica si querés
                throw new Exception($"Error al atender la orden: {ex.Message}", ex);
            }
        }



        public async Task<List<VisitaColectorHistorico>> ObtenerVisitasColectorPorUsuarioAsync(int idUser)
        {
            var visitas = new List<VisitaColectorHistorico>();

            using var connection = new SqlConnection(_connectionString);

            using var command = new SqlCommand("ORD.spObtenerVisitasColectorPorUsuario", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@IDUSER", idUser);

            await connection.OpenAsync();

            try
            {
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    visitas.Add(new VisitaColectorHistorico
                    {
                        IdAuditoria = reader.GetInt32(reader.GetOrdinal("ID_AUDITORIA")),
                        IdUser = reader.GetInt32(reader.GetOrdinal("IDUSER")),
                        IdEtTicket = reader.GetInt32(reader.GetOrdinal("IDETTICKET")),
                        Contrato = reader.GetInt32(reader.GetOrdinal("CONTRATO")),
                        EstadoOrden = reader["ESTADO_ORDEN"].ToString() ?? string.Empty,
                        ResultadoVisita = reader["RESULTADO_VISITA"].ToString() ?? string.Empty,
                        FechaRegistro = reader.GetDateTime(reader.GetOrdinal("FECHA_REGISTRO"))
                    });
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error SQL: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error general: {ex.Message}");
            }

            return visitas;
        }

    }
}
