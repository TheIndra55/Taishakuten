using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Kurisu.Configuration;
using Kurisu.VirusScan;
using Newtonsoft.Json;

namespace Kurisu.External.VirusTotal
{
    class VirusTotal : IScan
    {
        public string Name { get; set; } = "VirusTotal";
        public bool ThirdParty { get; set; } = true;

        public async Task<ScanResult> ScanAsync(Stream file, string hash)
        {
            var uri = new UriBuilder(new Uri("https://www.virustotal.com/vtapi/v2/file/report"));
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["apikey"] = Key;
            query["resource"] = hash;

            uri.Query = query.ToString()!;

            using var client = new HttpClient();

            var response = await client.GetAsync(uri.ToString());
            if (!response.IsSuccessStatusCode)
                throw new NoScanResultException(response.ReasonPhrase);

            var report = JsonConvert.DeserializeObject<Report>(await response.Content.ReadAsStringAsync());

            return new ScanResult()
            {
                Score = report.Positives,
                Detection = report.Scans.FirstOrDefault(x => x.Value.Detected).Value?.Result
            };
        }

        [ConVar("virustotal_key", HelpText = "The VirusTotal API key")]
        public static string Key { get; set; }
    }
}
