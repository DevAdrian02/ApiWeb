using System.Numerics;

namespace ApiHerramientaWeb.Modelos.CargarFacturas
{
    public class HistorialFactura
    {
        public int Id { get; set; }
        public int NumeroContrato { get; set; }
        public int IdFactura { get; set; }
        public string NumeroFactura { get; set; }
        public DateOnly FechaFactura { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Iva { get; set; }
        public decimal Total { get; set; }
        public string Cliente { get; set; }
        public DateOnly FechaPago { get; set; }
        public string Direccion { get; set; }
        public string Correo1 { get; set; }
        
        public string Correo2 { get; set; }
        public string Codsts { get; set; }
        public int STDFAC { get; set; }
        public string Nomfac { get; set; }

        // Nuevo campo agregado
        public string Servicios { get; set; }
    }
}
