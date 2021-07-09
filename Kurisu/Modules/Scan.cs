using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Kurisu.Configuration;
using Kurisu.External.HybridAnalysis;
using Kurisu.External.VirusTotal;
using Kurisu.Scan;
using Kurisu.VirusScan;

namespace Kurisu.Modules
{
    class Scan : BaseModule
    {
        private DiscordClient _client;
        private List<IScan> _scans;

        protected override void Setup(DiscordClient client)
        {
            _client = client;
            _client.MessageCreated += MessageCreated;

            _scans = new List<IScan>()
            {
                new VirusTotal(),
                new HybridAnalysis(),
                new KurisuScan()
            };
        }

        private async Task MessageCreated(MessageCreateEventArgs e)
        {
            var guild = Program.Guilds[e.Guild.Id];

            if(!e.Message.Attachments.Any()) return;
            if (!guild.VirusScan.Enabled) return;

            // check if any of the attachments contain the right file extension
            var attachment = e.Message.Attachments.FirstOrDefault(x =>
                guild.VirusScan.Extensions.Contains(Path.GetExtension(x.Url)));

            if(attachment == null) return;

            var file = await GetFile(attachment.Url);

            // run all scans
            var results = new Dictionary<IScan, ScanResult>();
            foreach (var scan in _scans)
            {
                ScanResult result;

                try
                {
                    result = await scan.ScanAsync(file.Stream, file.Hash);
                }
                catch (NoScanResultException)
                {
                    continue;
                }

                if(result.Score == 0) continue;

                results.Add(scan, result);
            }

            if (results.Count > 0)
            {
                var embed = new DiscordEmbedBuilder()
                    .WithTitle(Path.GetFileName(attachment.Url))
                    .WithDescription(file.Hash)
                    .WithFooter($"Powered by " + string.Join(", ",
                        _scans.Where(x => x.ThirdParty).Select(x => x.Name)))
                    .WithColor(new DiscordColor(0x2b3bbf));

                foreach (var result in results)
                {
                    var scan = result.Value;
                    embed.AddField(result.Key.Name, $"Score: {scan.Score}, Detection: {scan.Detection}");

                    if (scan.Extra != null)
                        embed.AddField("Extra", scan.Extra);

                    if (scan.Image != null)
                        embed.ImageUrl = scan.Image;
                }

                await e.Message.RespondAsync(embed: embed.Build());
            }
        }

        private async Task<DownloadedFile> GetFile(string url)
        {
            using var client = new HttpClient();
            var response = await client.GetStreamAsync(url);
            var stream = new MemoryStream();

            response.CopyTo(stream);
            stream.Position = 0;

            using var sha1 = SHA256.Create();
            var hash = sha1.ComputeHash(stream);

            return new DownloadedFile()
            {
                Hash = string.Join(null, hash.Select(x => x.ToString("x2"))),
                Stream = stream
            };
        }

        private struct DownloadedFile
        {
            public string Hash { get; set; }
            public Stream Stream { get; set; }
        }
    }
}
