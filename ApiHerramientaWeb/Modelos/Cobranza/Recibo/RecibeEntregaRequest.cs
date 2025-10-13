namespace ApiHerramientaWeb.Modelos.Cobranza.Recibe
{
    public class RecibeEntregaRequest
    {
        public EntregaModel Entrega { get; set; }
        public string Agente { get; set; }
        public UsuarioModel User { get; set; }
        public decimal TotalCanceladas { get; set; }
        public decimal TotalPendientes { get; set; }
        public List<FacturaModel> FacturasCanceladas { get; set; }
        public List<FacturaModel> FacturasPendientes { get; set; }
        public List<CostoModel> Costos { get; set; }
        public string Destinatario { get; set; }

        // Nuevas propiedades para el arqueo
        public decimal TotalArqueo { get; set; }
        public decimal Diferencia { get; set; }
        public List<DenominacionModel> Denominaciones { get; set; }
        public string Mensaje { get; set; }
        public string Asunto { get; set; }
    }

    // Modelo para las denominaciones
    public class DenominacionModel
    {
        public decimal Valor { get; set; }
        public int Cantidad { get; set; }
        public decimal Total { get; set; }
    }

    // Los demás modelos se mantienen igual...
    public class CostoModel
    {
        public string Servicio { get; set; }
        public int CantidadFacturas { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal TotalServicio { get; set; }
    }

    public class EntregaModel
    {
        public int IDEENTCOL { get; set; }
        public DateTime FCHENT { get; set; }
        public bool Arqueada { get; set; }
    }

    public class UsuarioModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class FacturaModel
    {
        public string Numero { get; set; }
        public decimal Monto { get; set; }
    }
}