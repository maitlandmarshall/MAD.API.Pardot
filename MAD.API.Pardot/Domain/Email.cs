using MAD.JsonConverters.NestedJsonConverterNS;
using Newtonsoft.Json;
using System;

namespace MAD.API.Pardot.Domain
{
    [JsonConverter(typeof(NestedJsonConverter))]
    public class Email : IImmutableEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string Subject { get; set; }

        [JsonProperty("message.html")]
        public string MessageHtml { get; set; }

        [JsonProperty("message.text")]
        public string MessageText { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
