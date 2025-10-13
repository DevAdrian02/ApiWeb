namespace ApiHerramientaWeb.Modelos.Result
{
    public class DataResult
    {
        public class ListaFactPend
        {
            public List<Utils.Resultado> res { get; set; }
            public List<CargarFacturas.FacturasPendientes> lstFactPend { get; set; }
        }
    }
}
