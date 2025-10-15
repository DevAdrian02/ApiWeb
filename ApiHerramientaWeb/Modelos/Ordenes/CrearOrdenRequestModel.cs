namespace ApiHerramientaWeb.Modelos.Ordenes
{
    public class CrearOrdenRequestModel
    {
        public int IDECNT { get; set; }
        public int IDETECAsg { get; set; }
        public int IDCUADRILLA { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }
}
