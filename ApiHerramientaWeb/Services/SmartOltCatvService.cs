using ApiHerramientaWeb.Controllers.Integraciones.SmartOlt;

namespace ApiHerramientaWeb.Services
{
    // Services/SmartOltCatvService.cs
    public class SmartOltCatvService : ICatvService
    {
        private readonly SmartOltController _smartOltController;

        public SmartOltCatvService(SmartOltController smartOltController)
        {
            _smartOltController = smartOltController;
        }

        public async Task ActivarCatvAsync(string codSuc)
        {
            var resultado = await _smartOltController.EnableCatTv(codSuc);
            if (resultado != "success")
                throw new Exception($"Error activando CATV: {resultado}");
        }

        public async Task DesactivarCatvAsync(string codSuc)
        {
            var resultado = await _smartOltController.DisableTV(codSuc);
            if (resultado != "success")
                throw new Exception($"Error activando CATV: {resultado}");
        }

    }
}
