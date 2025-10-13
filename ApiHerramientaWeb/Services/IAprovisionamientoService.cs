using static ApiHerramientaWeb.Modelos.Operaciones.Estructuras.DatosOpe.cablemodems;

namespace ApiHerramientaWeb.Services
{
    // Services/IAprovisionamientoService.cs
    public interface IAprovisionamientoService
    {
        Task<AprovisionamientoResult> GetEstadoModemAsync(string codSuc, string realm);
        Task ActivarAsync(string codSuc, string realm);
        Task DesactivarAsync(string codSuc, string realm);

    }

    public record AprovisionamientoResult(EstadoModem estadoModem, bool disponibleActivar);
}
