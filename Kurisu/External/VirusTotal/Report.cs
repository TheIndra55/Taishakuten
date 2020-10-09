using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Kurisu.External.VirusTotal
{
    public class Report
    {
        [JsonProperty("scan_id")]
        public string Id { get; set; }

        [JsonProperty("scan_date")]
        public DateTime Scanned { get; set; }

        [JsonProperty("permalink")]
        public string Link { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("positives")]
        public int Positives { get; set; }

        [JsonProperty("scans")]
        public Dictionary<string, Scan> Scans { get; set; }
    }
}
