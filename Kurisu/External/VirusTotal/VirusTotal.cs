using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace Kurisu.External.VirusTotal
{
    static class VirusTotal
    {
        /// <summary>
        /// Retrieve file scan report from VirusTotal by resource (MD5, SHA-1, SHA-256 hash)
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public static async Task<Report> GetReport(string key, string resource)
        {
            var uri = new UriBuilder(new Uri("https://www.virustotal.com/vtapi/v2/file/report"));
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["apikey"] = key;
            query["resource"] = resource;

            uri.Query = query.ToString();

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(uri.ToString());
                if (!response.IsSuccessStatusCode)
                    throw new HttpStatusCodeException(response);

                return JsonConvert.DeserializeObject<Report>(await response.Content.ReadAsStringAsync());
            }
        }
    }
}
