using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Kurisu.External.HybridAnalysis
{
    public class Summary
    {
        [JsonProperty("multiscan_result")]
        public int Score { get; set; }

        [JsonProperty("verdict")]
        public string Verdict { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("reports")]
        public List<string> Reports { get; set; }
    }
}
