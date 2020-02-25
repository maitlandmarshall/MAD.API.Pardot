using MAD.JsonConverters.NestedJsonConverterNS;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MAD.API.Pardot.Api
{
    [JsonConverter(typeof(NestedJsonConverter))]
    public class QueryResponse<T> : ApiResponse
    {
        [JsonConverter(typeof(NestedJsonConverter))]
        public class QueryResponseResult
        {
            [JsonProperty("total_results")]
            public int? TotalResults { get; set; }

            [JsonProperty("*")]
            public List<T> Items { get; set; }
        }

        public QueryResponseResult Result { get; set; }
    }
}
