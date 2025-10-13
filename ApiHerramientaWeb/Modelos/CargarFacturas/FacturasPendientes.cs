namespace ApiHerramientaWeb.Modelos.CargarFacturas
{
    public class FacturasPendientes
    {
        public int nContrato { get; set; }
        public string nombreCli { get; set; }

        public string email1 { get; set; }

        public string zona { get; set; }
        public string subzona { get; set; }


        public string email2 { get; set; }
        public string direccionCobro { get; set; }
        public string facturaCompleta { get; set; }
        public string serie { get; set; }
        public int factura { get; set; }
        public string periodo { get; set; }
        public decimal saldoCor { get; set; }
        public decimal saldoDol { get; set; }
        public string colector { get; set; }
        public string fchVen { get; set; }
        public int sucursal { get; set; }

        public string SucursalDesc { get; set; }
        public byte? Diapag { get; set; }
        public string DescripcionEstado { get; set; }
        public string DescripcionPago { get; set; }
        public string DescripcionTipo { get; set; }
        public string CodPago { get; set; }

        public string[] Servicios { get; set; }
        public string EstadoDispositivo { get; set; }
        public string EstadoContrato { get; set; }
        public string IdentificadorEquipo { get; set; }
        public decimal Latitud { get; set; }
        public decimal Longitud { get; set; }
        public string NumeroFaja { get; set; }

        public string Celcli { get; set; }

        public bool suspensionCompleto { get; set; }


    }

}
