using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Kurisu.Configuration;
using Kurisu.VirusScan;
using Newtonsoft.Json;

namespace Kurisu.External.HybridAnalysis
{
    public class HybridAnalysis : IScan
    {
        public string Name { get; set; } = "Hybrid Analysis";
        public bool ThirdParty { get; set; } = true;

        public async Task<ScanResult> ScanAsync(Stream file, string hash)
        {
            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("api-key", Key);
            client.DefaultRequestHeaders.Add("User-Agent", "Kurisu");

            var response = await client.GetAsync("https://www.hybrid-analysis.com/api/v2/overview/" + hash);
            if (!response.IsSuccessStatusCode)
                throw new NoScanResultException(response.ReasonPhrase);

            var summary = JsonConvert.DeserializeObject<Summary>(await response.Content.ReadAsStringAsync());
            var images = await GetImage(summary.Reports[0]);

            return new ScanResult()
            {
                Score = summary.Score,
                Detection = summary.Verdict,
                Image = images.Count == 0 ? null : images.Last()
            };
        }

        private async Task<List<string>> GetImage(string report)
        {
            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 Kurisu");

            var response = await client.GetAsync("https://www.hybrid-analysis.com/sample/screenshots/" + report);
            if (!response.IsSuccessStatusCode) return new List<string>();

            var body = await response.Content.ReadAsStringAsync();
            var regex = new Regex("href=\"(\\/file-inline\\/[a-zA-Z0-9]+\\/screenshot\\/[a-zA-Z0-9_]+.(png|jpg))")
                .Matches(body);

            return regex.Select(x => "https://www.hybrid-analysis.com" + x.Groups[1]).ToList();
        }

        [ConVar("hybrid_analysis_key", HelpText = "The Hybrid Analysis API key")]
        public static string Key { get; set; }
    }
}
