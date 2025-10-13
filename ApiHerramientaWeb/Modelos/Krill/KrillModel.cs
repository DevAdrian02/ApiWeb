using System.Globalization;
using System.Text;

namespace ApiHerramientaWeb.Modelos.Krill
{
    public class KrillModel
    {
        public class RootKrill
        {
            private string _name;
            private string _surname;
            private string _address;

            public string external_id { get; set; }

            public string city { get; set; }
            public string country { get; set; }
            public double latitude { get; set; }
            public double longitude { get; set; }
            public string realm { get; set; }
            public string comment { get; set; }


            public string name
            {
                get { return _name; }
                set { _name = RemoveDiacritics(value); }
            }
            public string surname
            {
                get { return _surname; }
                set { _surname = RemoveDiacritics(value); }
            }
            public string address
            {
                get { return _address; }
                set { _address = QuitarCaracteresEspeciales(RemoveDiacritics(value)); }
            }

            // Función para quitar caracteres especiales
            private string QuitarCaracteresEspeciales(string str)
            {
                StringBuilder sb = new StringBuilder();
                foreach (char c in str)
                {
                    if (
                            (c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')
                            || c == '.' || c == '/' || c == '@' || c == '$' || c == '+' || c == ','
                            || c == '(' || c == ')' || c == '-' || c == ' ')
                    {
                        sb.Append(c);
                    }
                }
                return sb.ToString();
            }
        }
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public class RootKrillCpe
        {
            public int customer { get; set; }
            public string sn { get; set; }
            public string mac { get; set; }
            public string model { get; set; }
            public int profile { get; set; }
            public bool active { get; set; }
            public bool access { get; set; }
            public string city { get; set; }
            public string country { get; set; }
            public string realm { get; set; }
            public bool bridge { get; set; }
            public string address_profile { get; set; }
            public string dsn { get; set; }
            public double latitude { get; set; }
            public string longitude { get; set; }
            public string line_profile { get; set; }
            public string external_id { get; set; }
            public string tech { get; set; }

            public string notes { get; set; }
        }

        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public class RootResultKrillCPE
        {
            public int id { get; set; }
            public bool access { get; set; }
            public string address { get; set; }
            public bool active { get; set; }
            public string address_profile { get; set; }
            public bool bridge { get; set; }
            public string city { get; set; }
            public string country { get; set; }
            public string cpename { get; set; }
            public DateTime created { get; set; }
            public int customer { get; set; }
            public bool disable_provision { get; set; }
            public bool disable_reconfig { get; set; }
            public object expiration_date { get; set; }
            public string dsn { get; set; }
            public bool enable_notifications { get; set; }
            public string external_id { get; set; }
            public bool external_voip { get; set; }
            public object fixed_address { get; set; }
            public bool has_tr069 { get; set; }
            public string internal_id { get; set; }
            public bool is_bitstream { get; set; }
            public string latitude { get; set; }
            public string line_profile { get; set; }
            public object locality { get; set; }
            public string longitude { get; set; }
            public string mac { get; set; }
            public string model { get; set; }
            public object mtamac { get; set; }
            public string notes { get; set; }
            public object postalcode { get; set; }
            public bool probe { get; set; }
            public string pppoe_password { get; set; }
            public object pppoe_username { get; set; }
            public int profile { get; set; }
            public string realm { get; set; }
            public object remote_id { get; set; }
            public object sn { get; set; }
            public string tech { get; set; }
            public DateTime? tech_updated { get; set; }
            public string topology { get; set; }
            public object tv_profile { get; set; }
            public DateTime updated { get; set; }
            public object voip_profile { get; set; }
            public object wanmac { get; set; }
            public object me_vlan { get; set; }
            public object lan_cidr { get; set; }
            public object wifi_password { get; set; }
            public object wifi_profile { get; set; }
            public object wifi_ssid { get; set; }
        }

        //Funcion para normalizar nombre
        private static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

            for (int i = 0; i < normalizedString.Length; i++)
            {
                char c = normalizedString[i];
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder
                .ToString()
                .Normalize(NormalizationForm.FormC);
        }

        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public class ResultCustumer
        {
            public int id { get; set; }
            public string name { get; set; }
            public string surname { get; set; }
            public string comment { get; set; }
            public string realm { get; set; }
            public string address { get; set; }
            public string city { get; set; }
            public object postalcode { get; set; }
            public string country { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }


            public string external_id { get; set; }
            public double? latitude { get; set; } // Cambiado a double nullable
            public double? longitude { get; set; }
            public object locality { get; set; }
        }

        public class Rootkrillcusumer
        {
            public List<ResultCustumer> results { get; set; }
        }
        public class Result
        {
            public int id { get; set; }
            public int customer { get; set; }
            public string tech { get; set; }
            public string mac { get; set; }
            public string mtamac { get; set; }
            public object wanmac { get; set; }
            public string model { get; set; }
            public object sn { get; set; }
            public object dsn { get; set; }
            public string address { get; set; }
            public string city { get; set; }
            public object postalcode { get; set; }
            public string country { get; set; }
            public int profile { get; set; }
            public bool active { get; set; }
            public bool access { get; set; }
            public string line_profile { get; set; }
            public object tv_profile { get; set; }
            public string address_profile { get; set; }
            public object voip_profile { get; set; }
            public bool bridge { get; set; }
            public bool disable_reconfig { get; set; }
            public string latitude { get; set; }
            public string longitude { get; set; }
            public string notes { get; set; }
            public string topology { get; set; }
            public string external_id { get; set; }
            public object remote_id { get; set; }
            public bool external_voip { get; set; }
            public bool probe { get; set; }
            public bool enable_notifications { get; set; }
            public object pppoe_username { get; set; }
            public string pppoe_password { get; set; }
            public object fixed_address { get; set; }
            public object wifi_ssid { get; set; }
            public object wifi_password { get; set; }
            public object expiration_date { get; set; }
            public string cpename { get; set; }
            public bool has_tr069 { get; set; }
            public DateTime created { get; set; }
            public DateTime updated { get; set; }
            public DateTime? tech_updated { get; set; }
            public string realm { get; set; }
            public List<int> potses { get; set; }
        }

        public class Root
        {
            public int count { get; set; }
            public object next { get; set; }
            public object previous { get; set; }
            public List<Result> results { get; set; }
        }
    }
}
