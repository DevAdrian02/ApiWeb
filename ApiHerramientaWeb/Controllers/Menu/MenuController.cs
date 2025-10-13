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
            string countKey = $"{cacheKey}_count";

            // 1. Verificar caché existente
            if (_cache.TryGetValue(cacheKey, out List<object> lstMenu) &&
                _cache.TryGetValue(countKey, out (int Padres, int Hijos) cachedCounts))
            {
                // 2. Obtener conteos actuales usando los SPs
                var currentCounts = await GetCurrentMenuCountsAsync(idusr);

                // 3. Comparar conteos
                if (currentCounts.Padres == cachedCounts.Padres &&
                    currentCounts.Hijos == cachedCounts.Hijos)
                {
                    return Ok(lstMenu); // Menú no ha cambiado
                }

                // Eliminar caché obsoleta
                _cache.Remove(cacheKey);
                _cache.Remove(countKey);
            }

            // 4. Reconstruir menú completo
            var menuData = await GetCompleteMenuDataAsync(idusr);
            lstMenu = BuildMenuStructure(menuData.Padres, menuData.Hijos);

            // 5. Almacenar en caché con los nuevos conteos
            var newCounts = (menuData.Padres.Count, menuData.Hijos.Count);
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(CacheDurationMinutes));

            _cache.Set(cacheKey, lstMenu, cacheOptions);
            _cache.Set(countKey, newCounts, cacheOptions);

            return Ok(lstMenu);
        }

        private async Task<(int Padres, int Hijos)> GetCurrentMenuCountsAsync(int idusr)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Contar padres usando el SP existente
                var padres = await ExecuteStoredProcedureAsync(
                    connection,
                    "CNF.spObtenerObjetosPadre",
                    new (string, object)[] { ("@idUsuario", idusr) }
                );

                // Contar hijos sumando resultados de todos los padres
                int totalHijos = 0;
                foreach (var padre in padres)
                {
                    var hijos = await ExecuteStoredProcedureAsync(
                        connection,
                        "CNF.spObtenerObjetosHijosPorPadre",
                        new (string, object)[] {
                            ("@idUsuario", idusr),
                            ("@idPadre", padre.IDEOBJ)
                        }
                    );
                    totalHijos += hijos.Count;
                }

                return (padres.Count, totalHijos);
            }
        }

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

        private List<object> BuildMenuStructure(
            List<DatosLayoutMenu.MenuPad> padres,
            List<DatosLayoutMenu.MenuPad> hijos)
        {
            var menu = new List<object>();

            foreach (var padre in padres)
            {
                var hijosDelPadre = hijos.FindAll(h => h.IDEOBJPAD == padre.IDEOBJ);
                var childrenList = new List<object>();

                foreach (var hijo in hijosDelPadre)
                {
                    childrenList.Add(new
                    {
                        title = hijo.DSCOBJ,
                        path = hijo.URL,
                        icon = hijo.ICONO
                    });
                }

                menu.Add(new
                {
                    title = padre.DSCOBJ,
                    icon = padre.ICONO,
                    children = childrenList
                });
            }

            return menu;
        }

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

        [HttpPost("invalidateMenuCache/{idusr}")]
        public IActionResult InvalidateMenuCache(int idusr)
        {
            string cacheKey = $"menu_{idusr}";
            string countKey = $"{cacheKey}_count";
            _cache.Remove(cacheKey);
            _cache.Remove(countKey);
            return Ok(new { message = "Cache invalidado exitosamente" });
        }
    }
}