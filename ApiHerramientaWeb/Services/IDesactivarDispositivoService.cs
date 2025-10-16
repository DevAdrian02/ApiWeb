using ApiHerramientaWeb.Modelos.Result;
using static ApiHerramientaWeb.Modelos.Operaciones.Estructuras.DatosOpe.cablemodems;

namespace ApiHerramientaWeb.Services
{
    public interface IDesactivarDispositivoService
    {
       Task<DesactivarResultModels> DesactivarCmInternoAsync(DesactivarCmRequest request, string ip);

    }
}
