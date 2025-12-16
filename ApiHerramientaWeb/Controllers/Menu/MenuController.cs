using ApiHerramientaWeb.Modelos.Menu.Estructura;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using ModeloPrincipal.Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace ApiHerramientaWeb.Controllers.Menu
{
    [ApiController]
    [Route("api/[controller]")]
    public class MenuController : ControllerBase
    {
        private readonly CVGEntities _context;
        private readonly IMemoryCache _cache;
        private readonly string _connectionString;
        private const int CacheDurationMinutes = 120;

        // Constante para la clave de caché del hash
        private const string HashKeySuffix = "_hash";

        public MenuController(CVGEntities context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
            _connectionString = _context.Database.GetDbConnection().ConnectionString;
        }

        [HttpGet("getMenuStruct/{idusr}")]
        public async Task<ActionResult<List<object>>> GetMenuStruct(int idusr)
        {
            string cacheKey = $"menu_{idusr}";
            string hashKey = $"{cacheKey}{HashKeySuffix}";

            // 1. Verificar si el menú y el hash están en caché
            if (_cache.TryGetValue(cacheKey, out List<object> lstMenu) &&
                _cache.TryGetValue(hashKey, out string cachedHash))
            {
                // 2. Obtener datos completos de la DB para calcular el hash actual
                var menuData = await GetCompleteMenuDataAsync(idusr);

                // 3. Calcular el hash actual (representa el estado actual de la DB)
                string currentHash = ComputeMenuHash(menuData.Padres, menuData.Hijos);

                // 4. Comparar hashes: Si son iguales, el contenido del menú no ha cambiado.
                if (currentHash == cachedHash)
                {
                    return Ok(lstMenu); // ¡Servido desde la caché! (Rápido)
                }

                // Si los hashes son diferentes, el contenido ha cambiado. 
                // La caché está obsoleta y la reconstruiremos en el paso 5/6.
                _cache.Remove(cacheKey);
                _cache.Remove(hashKey);

                // 5. Reconstruir menú usando los datos recién obtenidos
                lstMenu = BuildMenuStructure(menuData.Padres, menuData.Hijos);

                // 6. Almacenar en caché con el nuevo hash
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(CacheDurationMinutes));

                _cache.Set(cacheKey, lstMenu, cacheOptions);
                _cache.Set(hashKey, currentHash, cacheOptions); // Usamos el 'currentHash' calculado antes

                return Ok(lstMenu);
            }

            // Si la caché está vacía (no se pudo obtener lstMenu o cachedHash):

            // 4. Reconstruir menú completo
            var newData = await GetCompleteMenuDataAsync(idusr);
            lstMenu = BuildMenuStructure(newData.Padres, newData.Hijos);

            // 5. Calcular el nuevo hash
            string newHash = ComputeMenuHash(newData.Padres, newData.Hijos);

            // 6. Almacenar en caché con el nuevo hash
            var finalCacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(CacheDurationMinutes));

            _cache.Set(cacheKey, lstMenu, finalCacheOptions);
            _cache.Set(hashKey, newHash, finalCacheOptions);

            return Ok(lstMenu);
        }

        // Se elimina GetCurrentMenuCountsAsync. La comparación por hash es más completa.

        // ----------------------------------------------------------------------
        // LÓGICA DE DATOS
        // ----------------------------------------------------------------------

        private async Task<(List<DatosLayoutMenu.MenuPad> Padres, List<DatosLayoutMenu.MenuPad> Hijos)>
            GetCompleteMenuDataAsync(int idusr)
        {
            var padres = await ExecuteStoredProcedureAsync(
                "CNF.spObtenerObjetosPadre",
                new (string, object)[] { ("@idUsuario", idusr) }
            );

            var allHijos = new List<DatosLayoutMenu.MenuPad>();
            foreach (var padre in padres)
            {
                var hijos = await ExecuteStoredProcedureAsync(
                    "CNF.spObtenerObjetosHijosPorPadre",
                    new (string, object)[] {
                        ("@idUsuario", idusr),
                        ("@idPadre", padre.IDEOBJ)
                    }
                );
                allHijos.AddRange(hijos);
            }

            return (padres, allHijos);
        }

        /// <summary>
        /// Calcula un hash SHA256 basado en los datos clave de los ítems del menú.
        /// Ordena por IDEOBJ para asegurar consistencia en el hash.
        /// </summary>
        private string ComputeMenuHash(
            List<DatosLayoutMenu.MenuPad> padres,
            List<DatosLayoutMenu.MenuPad> hijos)
        {
            var dataToHash = new StringBuilder();

            // 1. Concatenar Padres (ordenados por ID para un hash consistente)
            foreach (var padre in padres.OrderBy(p => p.IDEOBJ))
            {
                // Incluimos ID, nombre, icono y URL.
                dataToHash.Append($"P:{padre.IDEOBJ},{padre.DSCOBJ},{padre.ICONO},{padre.URL}|");
            }

            // 2. Concatenar Hijos (ordenados por ID para un hash consistente)
            foreach (var hijo in hijos.OrderBy(h => h.IDEOBJ))
            {
                // Incluimos ID, ID del padre, nombre, icono y URL.
                dataToHash.Append($"H:{hijo.IDEOBJ},{hijo.IDEOBJPAD},{hijo.DSCOBJ},{hijo.ICONO},{hijo.URL}|");
            }

            // 3. Calcular el hash SHA256
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(dataToHash.ToString());
                var hashBytes = sha256.ComputeHash(bytes);

                // 4. Convertir el hash a una cadena hexadecimal
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }

        // ----------------------------------------------------------------------
        // EJECUTOR DE PROCEDIMIENTOS ALMACENADOS (ExecuteStoredProcedureAsync)
        // ----------------------------------------------------------------------

        private async Task<List<DatosLayoutMenu.MenuPad>> ExecuteStoredProcedureAsync(
            string spName,
            params (string name, object value)[] parameters)
        {
            var results = new List<DatosLayoutMenu.MenuPad>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(spName, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    foreach (var (name, value) in parameters)
                    {
                        command.Parameters.Add(new SqlParameter(name, value ?? DBNull.Value));
                    }

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            results.Add(MapMenuPad(reader));
                        }
                    }
                }
            }
            return results;
        }

        private async Task<List<DatosLayoutMenu.MenuPad>> ExecuteStoredProcedureAsync(
            SqlConnection connection,
            string spName,
            params (string name, object value)[] parameters)
        {
            var results = new List<DatosLayoutMenu.MenuPad>();

            using (var command = new SqlCommand(spName, connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                foreach (var (name, value) in parameters)
                {
                    command.Parameters.Add(new SqlParameter(name, value ?? DBNull.Value));
                }

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        results.Add(MapMenuPad(reader));
                    }
                }
            }
            return results;
        }

        // ----------------------------------------------------------------------
        // CONSTRUCTOR DE ESTRUCTURA (BuildMenuStructure)
        // ----------------------------------------------------------------------

        private List<object> BuildMenuStructure(
     List<DatosLayoutMenu.MenuPad> padres,
     List<DatosLayoutMenu.MenuPad> hijos)
        {
            var menu = new List<object>();

            var padresOrdenados = padres
                .OrderBy(p => p.DSCOBJ == "Home" ? 0 : 1)
                .ToList();

            foreach (var padre in padresOrdenados)
            {
                var hijosDelPadre = hijos
                    .Where(h => h.IDEOBJPAD == padre.IDEOBJ)
                    .ToList();

                var childrenList = hijosDelPadre.Select(hijo => new
                {
                    title = hijo.DSCOBJ,
                    path = hijo.URL.StartsWith("/") ? hijo.URL : "/" + hijo.URL,
                    icon = hijo.ICONO
                }).ToList();

                bool tieneHijos = childrenList.Any();
                bool tieneUrl = !string.IsNullOrWhiteSpace(padre.URL);

                // ✅ PADRE CON HIJOS → SIN PATH
                if (tieneHijos)
                {
                    menu.Add(new
                    {
                        title = padre.DSCOBJ,
                        icon = padre.ICONO,
                        children = childrenList
                    });

                    continue;
                }

                // ✅ PADRE SIN HIJOS → CON PATH
                if (tieneUrl)
                {
                    menu.Add(new
                    {
                        title = padre.DSCOBJ,
                        icon = padre.ICONO,
                        path = padre.URL.StartsWith("/") ? padre.URL : "/" + padre.URL
                    });
                }
            }

            return menu;
        }



        // ----------------------------------------------------------------------
        // MAPEADOR DE DATOS (MapMenuPad)
        // ----------------------------------------------------------------------

        private DatosLayoutMenu.MenuPad MapMenuPad(SqlDataReader reader)
        {
            return new DatosLayoutMenu.MenuPad
            {
                IDEOBJ = reader.GetInt32(reader.GetOrdinal("IDEOBJ")),
                IDEOBJPAD = (int)(reader.IsDBNull(reader.GetOrdinal("IDEOBJPAD")) ?
                    null : (int?)reader.GetInt32(reader.GetOrdinal("IDEOBJPAD"))),
                CODOBJ = reader.GetString(reader.GetOrdinal("CODOBJ")),
                DSCOBJ = reader.GetString(reader.GetOrdinal("DSCOBJ")),
                ICONO = reader.IsDBNull(reader.GetOrdinal("ICONO")) ?
                    string.Empty : reader.GetString(reader.GetOrdinal("ICONO")),
                URL = reader.IsDBNull(reader.GetOrdinal("URL")) ?
                    string.Empty : reader.GetString(reader.GetOrdinal("URL")),
                CONTROLADOR = reader.IsDBNull(reader.GetOrdinal("CONTROLADOR")) ?
                    string.Empty : reader.GetString(reader.GetOrdinal("CONTROLADOR")),
                ACCION = reader.IsDBNull(reader.GetOrdinal("ACCION")) ?
                    string.Empty : reader.GetString(reader.GetOrdinal("ACCION"))
            };
        }

        // ----------------------------------------------------------------------
        // INVALIDACIÓN EXPLÍCITA (InvalidateMenuCache)
        // ----------------------------------------------------------------------

        [HttpPost("invalidateMenuCache/{idusr}")]
        public IActionResult InvalidateMenuCache(int idusr)
        {
            string cacheKey = $"menu_{idusr}";
            string hashKey = $"{cacheKey}{HashKeySuffix}";

            _cache.Remove(cacheKey);
            _cache.Remove(hashKey);
            return Ok(new { message = "Cache invalidado exitosamente" });
        }
    }
}