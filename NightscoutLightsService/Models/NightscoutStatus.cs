using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace NightscoutLightsService.Models
{
    class NightscoutStatus
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("dateString")]
        public string DateString { get; set; }

        [JsonPropertyName("date")]
        public double Date { get; set; }

        [JsonPropertyName("sgv")]
        public double Sgv { get; set; }

        [JsonPropertyName("direction")]
        public string Direction { get; set; }


        [JsonPropertyName("filtered")]
        public string Filtered { get; set; }


        [JsonPropertyName("unfiltered")]
        public string UnFiltered { get; set; }


        [JsonPropertyName("rssi")]
        public string Rssi { get; set; }
    }
}



