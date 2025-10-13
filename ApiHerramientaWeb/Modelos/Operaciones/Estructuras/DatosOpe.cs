using System.Runtime.InteropServices.Marshalling;

namespace ApiHerramientaWeb.Modelos.Operaciones.Estructuras
{
    public class DatosOpe
    {
        public class cablemodems
        {
            public class activaCmDt
            {
                public string TENENCIA { get; set; }
                public string CONTRATO { get; set; }
                public int ESTTEC { get; set; }
                public string ESTADO { get; set; }
                public EstadoModem ESTADO_MODEM { get; set; } // Cambiado a enum
                public string NOMBRE { get; set; }
                public string COD_SUC { get; set; }
                public double LATITUD { get; set; }
                public double LONGITUD { get; set; }
                public string DIRECCION { get; set; }
                public string SERVICIO { get; set; }
                public string DICMACCM { get; set; }
                public string SUCURSAL { get; set; }
                public int ID_TECNOLOGIA { get; set; }
                public string REALM { get; set; }
                public bool UBICACION { get; set; }
                public bool MODEM_DISPONIBLE { get; set; }
                public string Marca { get; set; }
                public string Modelo { get; set; }
                public string Faja { get; set; }
                public int TipoTecnologia { get; set; }
                public bool suspensionCompleto { get; set; }

            }

            public class query_contrato
            {
                public int IDCONTRATO { get; set; }
                public int IDTECNOLOGIA { get; set; }
                public string TENENCIA { get; set; }
                public string SERVICIO { get; set; }
                public int CONTRATO { get; set; }
                public int ESTADO_CONTRATO_NUM { get; set; }
                public string ESTADO_CONTRATO { get; set; }
                public string NOMBRE { get; set; }
                public string DIRECCION { get; set; }
                public string DICMACCM { get; set; }
                public string SUCURSAL { get; set; }
                public string COD_SUC { get; set; }
                public double? LATITUD { get; set; }
                public double? LONGITUD { get; set; }
                public int id_servicio { get; set; }
                public int? IDPerfil { get; set; }
                public bool PRIMARIO { get; set; }
                public string REALM { get; set; }
                public int ID_SUCURSAL { get; set; }
                public string Marca { get; set; }
                public string Modelo { get; set; }
                public string Faja { get; set; }
                public bool suspensionCompleto { get; set; }
            }
            public class query_suspesion
            {
                public int IDCONTRATO { get; set; }
                public int IDTECNOLOGIA { get; set; }
                public string TENENCIA { get; set; }
                public string SERVICIO { get; set; }
                public int CONTRATO { get; set; }
                public int ESTADO_CONTRATO_NUM { get; set; }
                public string ESTADO_CONTRATO { get; set; }
                public string NOMBRE { get; set; }
                public string DIRECCION { get; set; }
                public string DICMACCM { get; set; }
                public string SUCURSAL { get; set; }
                public string COD_SUC { get; set; }
                public double? LATITUD { get; set; }
                public double? LONGITUD { get; set; }
                public int id_servicio { get; set; }
                public int? IDPerfil { get; set; }
                public bool PRIMARIO { get; set; }
                public string REALM { get; set; }
                public int ID_SUCURSAL { get; set; }
                public string Marca { get; set; }
                public string Modelo { get; set; }
                public string Faja { get; set; }
                public decimal SALDO { get; set; }
                public string Factura { get; set; }
                public DateTime FECHA { get; set; }
                public int DiaPago { get; set; }
                public bool? Aprovisiona { get; set; }

                public int? PrimeraMensualidad { get; set; }
                public int? ActivacionColector { get; set; }
            }

            public class auditActCm
            {
                public int IDEFTOCNT { get; set; }
                public string CODUSRCRE { get; set; }
                public DateTime FCHAPPCRE { get; set; }
            }
            public class actCmDt
            {
                public string tenencia { get; set; }
                public string mac { get; set; }
                public int ideftocnt { get; set; }
                public string codusr { get; set; }
                public string cod_suc { get; set; }
                public DateTime fechaAct { get; set; }
                public int idcanal { get; set; }
                public string codref { get; set; }
                public string realm { get; set; }
                public int estado_modem { get; set; }
            }
            public class respuestaWs
            {
                public bool flag { get; set; }
                public string mensaje { get; set; }
            }
            public class listaCanalPago
            {
                public int idCanal { get; set; }
                public string descripcion { get; set; }
            }

            public enum EstadoModem
            {
                Inactivo = 0,
                Activo = 1,
                SoloTV = 2            }

            public class DesactivarCmRequest
            {

                public int iduser { get; set; }

                public int Ideftocnt { get; set; }
                public string Comentario { get; set; }

               
            }


        }
    }
}
