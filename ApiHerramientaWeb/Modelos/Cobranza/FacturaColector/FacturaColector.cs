namespace ApiHerramientaWeb.Modelos.Cobranza.FacturaColector
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class FacturaColectorResponse
    {
        public int id { get; set; }
        public int contrato { get; set; } // Cambiado a int si viene como int

        public string email1 { get; set; }
        public string email2 { get; set; }
        public string name { get; set; }
        public decimal total { get; set; }
        public string COD_STS { get; set; }
        public int paymentDay { get; set; }
        public DateTime invoiceDate { get; set; }
        public decimal primeraMensualidad { get; set; }
        public string sucursal { get; set; }
        public string faja { get; set; }
        public string factura { get; set; }
        public int idZona { get; set; }
        public int idColector { get; set; }
        public string colector { get; set; }
        public string entregadoPor { get; set; }
        public string EstadoContrato { get; set; }

        public bool suspensionCompleto { get; set; }

        public ZonaResponse zona { get; set; }
    }

    public class ZonaResponse
    {
        public int id { get; set; }
        public string nombre { get; set; }
    }

}
