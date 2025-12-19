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
        public bool primeraMensualidad { get; set; }
        public string sucursal { get; set; }
        public int idsucursal { get; set; }
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

    public class FacturaColectorRaw
    {
        public int IDContrato { get; set; }
        public int? NoContrato { get; set; }
        public string EMAIL1 { get; set; }
        public string EMAIL2 { get; set; }
        public string NOMFAC { get; set; }
        public decimal? SALDO { get; set; }
        public string EstadoPagoFactura { get; set; }
        public int? DiaPago { get; set; }
        public DateTime? FECHA { get; set; }
        public bool? PrimeraMensualidad { get; set; }
        public string Sucursal { get; set; }
        public int IDSucursal { get; set; }
        public string NumeroFaja { get; set; }
        public string Factura { get; set; }
        public int? IDEUBIGEO { get; set; }
        public int? IDColector { get; set; }
        public string Colector { get; set; }
        public string EntregadoPor { get; set; }
        public string ESTADO_CONTRATO { get; set; }
        public bool? SuspensionCompleta { get; set; }
        public string Zona { get; set; }
    }

}
