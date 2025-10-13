namespace ApiHerramientaWeb.Modelos.SmartOlt
{
    public class SmartListOltModel
    {

        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public class Response
        {
            public string id { get; set; }
            public string name { get; set; }
            public string olt_hardware_version { get; set; }
            public string ip { get; set; }
            public string telnet_port { get; set; }
            public string snmp_port { get; set; }
        }

        public class Root
        {
            public List<Response> response { get; set; }
            public string response_code { get; set; }
            public bool status { get; set; }
        }

        public class AdministrativeOnuResponse
        {
            public string administrative_status { get; set; }
            public bool status { get; set; }
            public string response_code { get; set; }
        }
    }
}
