
using ApiHerramientaWeb.Controllers.MoviTv;

namespace ApiHerramientaWeb.Services
{
    // Services/MoviTvService.cs
    public class MoviTvService 
    {
        private readonly MoviTvServicesController _moviTvController;

        public MoviTvService(IConfiguration configuration)
        {
            _moviTvController = new MoviTvServicesController(configuration);
        }

        public async Task ActivarAsync(string partnerId)
        {
            var resultado = await _moviTvController.UnsuspendUserAsync(partnerId);

            if (!resultado)
                throw new Exception("Error reactivando usuario en MoviTV");
        }

        public async Task DesactivarAsync(string partnerId)
        {
            var resultado = await _moviTvController.SuspendedUserAsync(partnerId);

            if (!resultado)
                throw new Exception("Error reactivando usuario en MoviTV");
        }

    }
}
