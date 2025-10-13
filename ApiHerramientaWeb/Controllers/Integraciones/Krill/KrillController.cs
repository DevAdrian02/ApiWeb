using Newtonsoft.Json;
using static ApiHerramientaWeb.Modelos.Krill.KrillModel;
using System.Text;
using ApiHerramientaWeb.Modelos.Krill;

namespace ApiHerramientaWeb.Controllers.Integraciones.Krill
{
    public class KrillController
    {
        private HttpClient client;
        private string baseUrl;
        private string authorizationToken;
        private string username;
        private string password;

        public KrillController(IConfiguration configuration)
        {
            client = new HttpClient();
            baseUrl = configuration["KrillSettings:BaseUrl"];
            authorizationToken = configuration["KrillSettings:AuthorizationToken"];
            username = configuration["KrillSettings:Username"];
            password = configuration["KrillSettings:Password"];
        }

        #region Consulta

        public class NullToDefaultValueConverter : JsonConverter
        {

            public override bool CanConvert(Type objectType)
            {
                // Permitir la conversión para todos los tipos
                return true;
            }
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                // Manejar el valor nulo según el tipo de objeto
                if (reader.TokenType == JsonToken.Null)
                {
                    if (objectType == typeof(int) || objectType == typeof(double))
                    {
                        // Si es un número, se asigna 0
                        return 0;
                    }
                    else if (objectType == typeof(string))
                    {
                        // Si es una cadena, se asigna una cadena vacía
                        return string.Empty;
                    }
                    else
                    {
                        // Para otros tipos, se devuelve el valor predeterminado
                        return Activator.CreateInstance(objectType);
                    }
                }

                // Si no es un valor nulo, se deja que el JSON.NET maneje la deserialización normalmente
                return serializer.Deserialize(reader, objectType);
            }
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                // No es necesario implementar este método para la deserialización
                throw new NotImplementedException();
            }
        }

        public int GetCustomerId(string realm, string externalId)
        {
            try
            {
                var apiUrl = $"{baseUrl}/isp/customers/?realm={realm}&external_id={externalId}";
                Console.WriteLine(apiUrl);

                // Agregar la autorización básica al encabezado de la solicitud
                string _username = username;
                string _password = password;
                string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_username}:{_password}"));
                client.DefaultRequestHeaders.Add("Authorization", $"Basic {credentials}");

                // Realizar la solicitud al servidor de manera sincrónica
                HttpResponseMessage response = client.GetAsync(apiUrl).Result;
                response.EnsureSuccessStatusCode();

                // Leer el contenido de la respuesta
                string responseContent = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine(responseContent);

                // Deserializar la respuesta en un objeto Rootkrillcusumer
                Rootkrillcusumer responseModel = JsonConvert.DeserializeObject<Rootkrillcusumer>(responseContent);

                // Verificar si la lista de resultados tiene elementos antes de acceder al primer elemento
                if (responseModel?.results != null && responseModel.results.Count > 0)
                {
                    int customerId = responseModel.results[0].id;
                    return customerId;
                }
                else
                {
                    throw new Exception("No se encontraron resultados en la respuesta de la API.");
                }
            }
            catch (Exception ex)
            {
                string errorMessage = "Error: " + ex.Message;
                Console.WriteLine(errorMessage);

                // Puedes manejar el error de la manera que desees o lanzar una excepción
                throw new Exception(errorMessage);
            }
        }



        public Root GetCpe(int idcustomer)
        {
            try
            {
                string url = "https://cav.phicus.app/api/customers";
                string apiUrl = $"{url}/{idcustomer}/cpes/";
                string basicAuth = "Basic Y2F2YWRtaW46azIyT3N2WDU="; // Esto es tu cadena de autorización básica

                Console.WriteLine(apiUrl);
                client.DefaultRequestHeaders.Remove("Authorization");

                // Agregar la autorización básica al encabezado de la solicitud
                string _username = username;
                string _password = password;
                string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_username}:{_password}"));
                client.DefaultRequestHeaders.Add("Authorization", $"Basic {credentials}");

                // Realizar la solicitud al servidor de manera sincrónica
                HttpResponseMessage response = client.GetAsync(apiUrl).Result;
                response.EnsureSuccessStatusCode();

                // Leer el contenido de la respuesta
                string responseContent = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine(responseContent);

                // Deserializar la respuesta en un objeto Root
                Root responseModel = JsonConvert.DeserializeObject<Root>(responseContent);
                return responseModel;
            }
            catch (Exception ex)
            {
                string errorMessage = "Error: " + ex.Message;
                Console.WriteLine(errorMessage);

                // Puedes manejar el error de la manera que desees o lanzar una excepción
                throw new Exception(errorMessage);
            }
        }

        public async Task UpdateCpeAsync(int idCpe, bool access, bool active)
        {
            try
            {
                var apiUrl = $"{baseUrl}/isp/cpes/{idCpe}";
                Console.WriteLine(apiUrl);

                // Crear un objeto JSON con los datos requeridos en el cuerpo de la solicitud
                var requestData = new
                {
                    access = access,
                    active = active
                };

                // Convertir el objeto JSON en una cadena
                var jsonContent = JsonConvert.SerializeObject(requestData);

                // Crear un contenido de solicitud con formato JSON
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Crear la autorización básica y asignarla al encabezado
                string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

                // Realizar la solicitud PATCH al servidor.
                var response = await client.PatchAsync(apiUrl, content).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                // Leer el contenido de la respuesta
                string responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseContent);
            }
            catch (Exception ex)
            {
                string errorMessage = "Error: " + ex.Message;
                Console.WriteLine(errorMessage);
                throw new Exception(errorMessage);
            }
        }




        #endregion

    }
}
