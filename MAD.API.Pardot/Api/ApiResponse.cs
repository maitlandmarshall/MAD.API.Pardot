using MAD.JsonConverters.NestedJsonConverterNS;
using Newtonsoft.Json;

namespace MAD.API.Pardot.Api
{
    [JsonConverter(typeof(NestedJsonConverter))]
    public abstract class ApiResponse
    {
        public struct QueryResponseAttributes
        {
            public string Stat { get; set; }
            public int? Version { get; set; }

            [JsonProperty("err_code")]
            public int? ErrorCode { get; set; }
        }

        [JsonProperty("err")]
        public string Error { get; set; }

        [JsonProperty("@attributes")]
        public QueryResponseAttributes Attributes { get; set; }
    }
}
