using static ApiHerramientaWeb.Modelos.Operaciones.Estructuras.DatosOpe.cablemodems;

namespace ApiHerramientaWeb.Services
{
    public interface IEstadoModemService
    {
        Task<EstadoModem> ObtenerEstadoModemPorContrato(int numeroContrato);
    }
}
