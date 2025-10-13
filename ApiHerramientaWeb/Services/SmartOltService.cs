using ApiHerramientaWeb.Controllers.Integraciones.SmartOlt;
using static ApiHerramientaWeb.Modelos.Operaciones.Estructuras.DatosOpe.cablemodems;

namespace ApiHerramientaWeb.Services
{
    // Services/SmartOltService.cs
    public class SmartOltService : IAprovisionamientoService
    {
        private readonly SmartOltController _smartOltController;

        public SmartOltService(SmartOltController smartOltController)
        {
            _smartOltController = smartOltController;
        }

        public async Task<AprovisionamientoResult> GetEstadoModemAsync(string codSuc, string realm = null)
        {
            var result = await _smartOltController.GetAdministrativeOnu(codSuc);
            bool disponibleActivar = result.administrative_status == "Enabled";
            return new AprovisionamientoResult(disponibleActivar ? EstadoModem.Activo : EstadoModem.Inactivo, disponibleActivar);
        }

        public async Task ActivarAsync(string codSuc, string realm = null)
        {
            var resultado = await _smartOltController.Enable(codSuc);
            if (resultado != "success") throw new Exception($"Error activando ONU: {resultado}");

       
        }

        public async Task DesactivarAsync(string codSuc, string realm = null)
        {
            var resultado = await _smartOltController.DisableOnu(codSuc);
            if (resultado != "success") throw new Exception($"Error desactivando ONU: {resultado}");

            //resultado = await _smartOltController.DisableTV(codSuc);
            //if (resultado != "success") throw new Exception($"Error desactivando CATV: {resultado}");
        }
    }
}
