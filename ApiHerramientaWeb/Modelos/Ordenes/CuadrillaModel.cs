namespace ApiHerramientaWeb.Modelos.Ordenes
{
    public class CuadrillaModel
    {
        public int IDCuadrilla { get; set; }
        public string Codigo { get; set; }
        public string Descripcion { get; set; }
        public string Tipo { get; set; }
        public string Placa { get; set; }
        public int IDSucursal { get; set; }
        public string Bodega { get; set; }
    }
}
