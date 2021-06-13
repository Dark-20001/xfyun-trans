using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APITest
{
    public class XunFeiRequestClass
    {
        [JsonProperty("common")]
        public common c { get; set; }
        [JsonProperty("business")]
        public business b { get; set; }
        [JsonProperty("data")]
        public data d { get; set; }
    }

    public class common
    {
        [JsonProperty("app_id")]
        public string app_id { get; set; }
    }

    public class business
    {
        [JsonProperty("from")]
        public string from { get; set; }
        [JsonProperty("to")]
        public string to { get; set; }
    }

    public class data
    {
        [JsonProperty("text")]
        public string text { get; set; }
    }
}
