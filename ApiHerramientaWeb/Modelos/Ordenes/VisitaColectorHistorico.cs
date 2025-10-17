namespace ApiHerramientaWeb.Modelos.Ordenes
{
    public class VisitaColectorHistorico
    {

        public int IdAuditoria { get; set; }
        public int IdUser { get; set; }
        public int IdEtTicket { get; set; }
        public int Contrato { get; set; }
        public string EstadoOrden { get; set; } = string.Empty;
        public string ResultadoVisita { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; }
    }
}
