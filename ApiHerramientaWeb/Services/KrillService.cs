using ApiHerramientaWeb.Controllers.Integraciones.Krill;
using static ApiHerramientaWeb.Modelos.Operaciones.Estructuras.DatosOpe.cablemodems;

namespace ApiHerramientaWeb.Services
{
    // Services/KrillService.cs
    public class KrillService : IAprovisionamientoService
    {
        private readonly KrillController _krillController;

        public KrillService(IConfiguration configuration)
        {
            _krillController = new KrillController(configuration);
        }

        public async Task<AprovisionamientoResult> GetEstadoModemAsync(string codSuc, string realm)
        {
            int customerId = _krillController.GetCustomerId(realm, codSuc);
            var response = _krillController.GetCpe(customerId);

            if (response?.results?.Count > 0)
            {
                var primerResultado = response.results[0];
                bool disponibleActivar = primerResultado.active && primerResultado.access;
                return new AprovisionamientoResult(disponibleActivar ? EstadoModem.Activo : EstadoModem.Inactivo, disponibleActivar);
            }
            return new AprovisionamientoResult(EstadoModem.Inactivo, false);
        }

        public async Task ActivarAsync(string codSuc, string realm)
        {
            int customerId = _krillController.GetCustomerId(realm, codSuc);
            var cpeData = _krillController.GetCpe(customerId);

            if (cpeData?.results?.Count > 0)
            {
                await _krillController.UpdateCpeAsync(cpeData.results[0].id, true, true);
            }
        }

        public async Task DesactivarAsync(string codSuc, string realm)
        {
            int customerId = _krillController.GetCustomerId(realm, codSuc);
            var cpeData = _krillController.GetCpe(customerId);

            if (cpeData?.results?.Count > 0)
            {
                await _krillController.UpdateCpeAsync(cpeData.results[0].id, false, false);
            }
        }
    }
}
