using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ApiHerramientaWeb.Controllers.MoviTv
{
    public class MoviTvServicesController
    {
        private readonly HttpClient client;
        private readonly string baseUrl;
        private readonly string partner;
        private readonly string password;

        public MoviTvServicesController(IConfiguration configuration)
        {
            client = new HttpClient();
            baseUrl = configuration["MoviTvSettings:BaseUrl"];
            partner = configuration["MoviTvSettings:Partner"];
            password = configuration["MoviTvSettings:Password"];
        }

        /// <summary>
        /// Reactiva (unsuspend) un usuario en MoviTV
        /// </summary>
        /// <param name="partnerId">ID del usuario/contrato en MoviTV</param>
        public async Task<bool> UnsuspendUserAsync(string partnerId)
        {
            var url = $"{baseUrl}unsuspend-user" +
                      $"?partner={partner}" +
                      $"&password={password}" +
                      $"&partnerid={partnerId}";

            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(
                    $"MoviTV unsuspend-user falló. Status: {response.StatusCode}. Detalle: {error}"
                );
            }

            return true;
        }

        /// <summary>
        /// Suspende (Suspended) un usuario en MoviTV
        /// </summary>
        /// <param name="partnerId">ID del usuario/contrato en MoviTV</param>
        public async Task<bool> SuspendedUserAsync(string partnerId)
        {
            try
            {
                var url = $"{baseUrl}suspend-user" +
                          $"?partner={partner}" +
                          $"&password={password}" +
                          $"&partnerid={partnerId}";

                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    // Solo registramos el error en logs, pero NO lanzamos excepción
                    var error = await response.Content.ReadAsStringAsync();
                    // Aquí podrías agregar logging si lo necesitas
                    Console.WriteLine($"MoviTV suspend-user falló. Status: {response.StatusCode}. Detalle: {error}");

                    // Importante: NO throw, solo retornamos true
                }

                return true; // Siempre retorna true
            }
            catch (Exception ex)
            {
                // Capturamos cualquier excepción y la registramos
                Console.WriteLine($"Error en SuspendedUserAsync: {ex.Message}");

                return true; // Siempre retorna true incluso con excepciones
            }
        }
    }
}
