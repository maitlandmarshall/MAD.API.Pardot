﻿using Newtonsoft.Json;
using System;

namespace MAD.API.Pardot.Domain
{
    public class Visit : IMutableEntity
    {
        public int Id { get; set; }

        [JsonProperty("visitor_id")]
        public int VisitorId { get; set; }

        [JsonProperty("prospect_id")]
        public int? ProspectId { get; set; }

        [JsonProperty("visitor_page_view_count")]
        public int VisitorPageViewCount { get; set; }

        [JsonProperty("first_visitor_page_view_at")]
        public DateTime? FirstVisitorPageViewAt { get; set; }

        [JsonProperty("last_visitor_page_view_at")]
        public DateTime? LastVisitorPageViewAt { get; set; }

        [JsonProperty("duration_in_seconds")]
        public int DurationInSeconds { get; set; }

        [JsonProperty("campaign_parameter")]
        public string CampaignParameter { get; set; }

        [JsonProperty("medium_parameter")]
        public string MediumParameter { get; set; }

        [JsonProperty("source_parameter")]
        public string SourceParameter { get; set; }

        [JsonProperty("content_parameter")]
        public string ContentParameter { get; set; }

        [JsonProperty("term_parameter")]
        public string TermParameter { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}
