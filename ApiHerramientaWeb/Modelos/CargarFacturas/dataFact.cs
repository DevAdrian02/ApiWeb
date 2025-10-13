namespace ApiHerramientaWeb.Modelos.CargarFacturas
{
    public class dataFact
    {
        public int ideftocnt { get; set; }
        public string srefac { get; set; }
        public int numfisfac { get; set; }
        public string nomfac { get; set; }
        public decimal sdofacloc { get; set; }
        public decimal pago { get; set; }
        public decimal diferencia { get; set; }
        public string numref { get; set; }
        public bool pagada { get; set; }
        public bool process { get; set; }
    }
}
