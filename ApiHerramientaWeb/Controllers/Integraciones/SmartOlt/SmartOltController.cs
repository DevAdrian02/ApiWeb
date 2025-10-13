using ApiHerramientaWeb.Modelos.SmartOlt;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace ApiHerramientaWeb.Controllers.Integraciones.SmartOlt
{
    public class SmartOltController
    {
        private readonly string _xTokenHeaderValueR;
        private readonly string _xTokenHeaderValueRW;
        private readonly string _apiBaseUrl;

        public SmartOltController(IConfiguration configuration)
        {
            _xTokenHeaderValueR = configuration["SmartOltSettings:XTokenHeaderValueR"];
            _xTokenHeaderValueRW = configuration["SmartOltSettings:XTokenHeaderValueRW"];
            _apiBaseUrl = configuration["SmartOltSettings:ApiBaseUrl"];
        }

        #region Obtener

        #region GetAdministrativeOnu
        public async Task<SmartListOltModel.AdministrativeOnuResponse> GetAdministrativeOnu(string externalId)
        {
            try
            {
                var apiUrl = $"{_apiBaseUrl}api/onu/get_onu_administrative_status/{externalId}";


                using (var client = new HttpClient())
                {
                    // Agregar la autorización al encabezado de la solicitud
                    client.DefaultRequestHeaders.Add("Authorization", "inherit auto from parent");

                    // Agregar el encabezado X-Token
                    client.DefaultRequestHeaders.Add("X-Token", _xTokenHeaderValueR);

                    // Realizar la solicitud GET al servidor
                    var response = await client.GetAsync(apiUrl).ConfigureAwait(false);

                    // Capturar el código de estado de la respuesta
                    if (!response.IsSuccessStatusCode)
                    {
                        string errorResponse = await response.Content.ReadAsStringAsync();
                        throw new HttpRequestException($"Error en la solicitud HTTP: {response.StatusCode}, Contenido: {errorResponse}");
                    }

                    // Leer el contenido de la respuesta
                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Response Content: {responseContent}");

                    // Deserealizar la respuesta JSON en un objeto
                    SmartListOltModel.AdministrativeOnuResponse root = JsonConvert.DeserializeObject<SmartListOltModel.AdministrativeOnuResponse>(responseContent);

                    return root;
                }
            }
            catch (HttpRequestException httpEx)
            {
                string errorMessage = $"Error en la solicitud HTTP: {httpEx.Message}";
                Console.WriteLine($"Error Message: {errorMessage}");
                throw new Exception(errorMessage, httpEx);
            }
            catch (JsonException jsonEx)
            {
                string errorMessage = $"Error al deserializar la respuesta JSON: {jsonEx.Message}";
                Console.WriteLine($"Error Message: {errorMessage}");
                throw new Exception(errorMessage, jsonEx);
            }
            catch (Exception ex)
            {
                string errorMessage = $"Error: {ex.Message}";
                Console.WriteLine($"Error Message: {errorMessage}");
                throw new Exception(errorMessage, ex);
            }
        }

        #endregion

        #endregion

        #region ActivarOnu

        #region Enable
        public async Task<string> Enable(string externalId)
        {
            try
            {
                var apiUrl = $"{_apiBaseUrl}api/onu/enable/{externalId}";
                Console.WriteLine($"API URL: {apiUrl}");

                using (var client = new HttpClient())
                {
                    // Agregar la autorización al encabezado de la solicitud
                    client.DefaultRequestHeaders.Add("Authorization", "inherit auto from parent");

                    // Agregar el encabezado X-Token
                    client.DefaultRequestHeaders.Add("X-Token", _xTokenHeaderValueRW);

                    // Realizar la solicitud POST al servidor (en lugar de GET)
                    var response = await client.PostAsync(apiUrl, null).ConfigureAwait(false);

                    response.EnsureSuccessStatusCode();

                    // Leer el contenido de la respuesta
                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Response Content: {responseContent}");

                    // Deserializar el JSON completo
                    var responseObject = JsonConvert.DeserializeAnonymousType(responseContent, new { response = "", response_code = "" });

                    // Obtener el valor de response_code cuando response es "ONU enabled"
                    if (responseObject.response == "ONU enabled")
                    {
                        string responseCode = responseObject.response_code;
                        Console.WriteLine($"Response Code: {responseCode}");
                        return responseCode;
                    }
                    else
                    {
                        // Manejar la situación donde response no es "ONU enabled"
                        Console.WriteLine("El estado de la ONU no es 'ONU enabled'");
                        return "Estado no válido";
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = "Error: " + ex.Message;
                Console.WriteLine($"Error Message: {errorMessage}");

                // Puedes manejar el error de la manera que desees o lanzar una excepción
                throw new Exception(errorMessage);
            }
        }
        #endregion

        #region EnableCatTv
        public async Task<string> EnableCatTv(string externalId)
        {
            try
            {
                var apiUrl = $"{_apiBaseUrl}api/onu/enable_catv/{externalId}";
                Console.WriteLine($"API URL: {apiUrl}");

                using (var client = new HttpClient())
                {
                    // Agregar la autorización al encabezado de la solicitud
                    client.DefaultRequestHeaders.Add("Authorization", "inherit auto from parent");

                    // Agregar el encabezado X-Token
                    client.DefaultRequestHeaders.Add("X-Token", _xTokenHeaderValueRW);

                    // Realizar la solicitud POST al servidor (en lugar de GET)
                    var response = await client.PostAsync(apiUrl, null).ConfigureAwait(false);

                    response.EnsureSuccessStatusCode();

                    // Leer el contenido de la respuesta
                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Response Content: {responseContent}");

                    // Deserializar solo el campo response_code
                    var responseObject = JsonConvert.DeserializeAnonymousType(responseContent, new { response_code = "" });

                    // Obtener el valor de response_code
                    string responseCode = responseObject.response_code;

                    Console.WriteLine($"Response Code: {responseCode}");

                    return responseCode;
                }
            }
            catch (Exception ex)
            {
                string errorMessage = "Error: " + ex.Message;
                Console.WriteLine($"Error Message: {errorMessage}");

                // Puedes manejar el error de la manera que desees o lanzar una excepción
                throw new Exception(errorMessage);
            }
        }
        #endregion




        #region Disable
        public async Task<string> DisableOnu(string externalId)
        {
            try
            {
                var apiUrl = $"{_apiBaseUrl}api/onu/disable/{externalId}";
                Console.WriteLine($"API URL: {apiUrl}");

                using (var client = new HttpClient())
                {
                    // Agregar la autorización al encabezado de la solicitud
                    client.DefaultRequestHeaders.Add("Authorization", "inherit auto from parent");

                    // Agregar el encabezado X-Token
                    client.DefaultRequestHeaders.Add("X-Token", _xTokenHeaderValueRW);

                    // Realizar la solicitud POST al servidor (en lugar de GET)
                    var response = await client.PostAsync(apiUrl, null).ConfigureAwait(false);

                    response.EnsureSuccessStatusCode();

                    // Leer el contenido de la respuesta
                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Response Content: {responseContent}");

                    // Deserializar el JSON completo
                    var responseObject = JsonConvert.DeserializeAnonymousType(responseContent, new { response = "", response_code = "" });

                    // Obtener el valor de response_code cuando response es "ONU disable"
                    if (responseObject.response == "ONU disabled")
                    {
                        string responseCode = responseObject.response_code;
                        Console.WriteLine($"Response Code: {responseCode}");
                        return responseCode;
                    }
                    else
                    {
                        // Manejar la situación donde response no es "ONU disable"
                        Console.WriteLine("El estado de la ONU no es 'ONU disable'");
                        return "Estado no válido";
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = "Error: " + ex.Message;
                Console.WriteLine($"Error Message: {errorMessage}");

                // Puedes manejar el error de la manera que desees o lanzar una excepción
                throw new Exception(errorMessage);
            }
        }
        #endregion

        #region DisableTV
        public async Task<string> DisableTV(string externalId)
        {
            try
            {
                var apiUrl = $"{_apiBaseUrl}api/onu/disable_catv/{externalId}";
                Console.WriteLine($"API URL: {apiUrl}");

                using (var client = new HttpClient())
                {
                    // Agregar la autorización al encabezado de la solicitud
                    client.DefaultRequestHeaders.Add("Authorization", "inherit auto from parent");

                    // Agregar el encabezado X-Token
                    client.DefaultRequestHeaders.Add("X-Token", _xTokenHeaderValueRW);

                    // Realizar la solicitud POST al servidor (en lugar de GET)
                    var response = await client.PostAsync(apiUrl, null).ConfigureAwait(false);

                    response.EnsureSuccessStatusCode();

                    // Leer el contenido de la respuesta
                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Response Content: {responseContent}");

                    // Deserializar el JSON completo
                    var responseObject = JsonConvert.DeserializeAnonymousType(responseContent, new { response = "", response_code = "" });

                    // Obtener el valor de response_code cuando response es "ONU enabled"
                    if (responseObject.response == "CATV disabled")
                    {
                        string responseCode = responseObject.response_code;
                        Console.WriteLine($"Response Code: {responseCode}");
                        return responseCode;
                    }
                    else
                    {
                        // Manejar la situación donde response no es "ONU enabled"
                        Console.WriteLine("No se pudo desabilitar el catTV");
                        return "Estado no válido";
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = "Error: " + ex.Message;
                Console.WriteLine($"Error Message: {errorMessage}");

                // Puedes manejar el error de la manera que desees o lanzar una excepción
                throw new Exception(errorMessage);
            }
        }
        #endregion

        #endregion
    }
}
