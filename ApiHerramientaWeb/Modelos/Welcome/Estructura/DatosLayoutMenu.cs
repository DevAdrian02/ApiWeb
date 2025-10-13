namespace ApiHerramientaWeb.Modelos.Welcome.Estructura
{
    public class DatosLayoutMenu
    {
        public class MenuPad
        {
            public int IDEOBJ { get; set; }
            public int IDEOBJPAD { get; set; }
            public string CODOBJ { get; set; }
            public string DSCOBJ { get; set; }
            public string ICONO { get; set; }
            public string URL { get; set; }
            public string CONTROLADOR { get; set; }
            public string ACCION { get; set; }
        }
        public class MenuComplex
        {
            public int IDEOBJ { get; set; }
            public int IDEOBJPAD { get; set; }
            public string CODOBJ { get; set; }
            public string DSCOBJ { get; set; }
            public string ICONO { get; set; }
            public string URL { get; set; }
            public string CONTROLADOR { get; set; }
            public string ACCION { get; set; }
            public List<MenuPad> HIJOS { get; set; }
        }

    }
}
