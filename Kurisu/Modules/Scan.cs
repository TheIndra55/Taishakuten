using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Kurisu.External.VirusTotal;

namespace Kurisu.Modules
{
    class Scan : BaseModule
    {
        private DiscordClient _client { get; set; }

        public VirusTotal VirusTotal { get; set; }

        protected override void Setup(DiscordClient client)
        {
            _client = client;

            _client.MessageCreated += MessageCreated;
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

            var hash = await CalculateHashAsync(attachment.Url);
            Report report;

            try
            {
                report = await VirusTotal.GetReport(hash);
            }
            catch (HttpStatusCodeException)
            {
                return;
            }

            if (report.Positives > 0)
            {
                var embed = new DiscordEmbedBuilder()
                    .WithTitle(Path.GetFileName(attachment.Url) + (report.Positives > 4 ? " ❗" : "" ))
                    .WithDescription($"[{hash}]({report.Link})")
                    .AddField("Score", $"{report.Positives}/{report.Total} engines detected")
                    .AddField("Detection", report.Scans.First(x => x.Value.Detected).Value.Result)
                    .WithFooter("Powered by VirusTotal")
                    .WithColor(new DiscordColor(0x2b3bbf))
                    .Build();

                await e.Message.RespondAsync(embed: embed);
            }
        }

        private async Task<string> CalculateHashAsync(string url)
        {
            using (var client = new HttpClient())
            {
                var stream = await client.GetStreamAsync(url);
                using (var sha1 = SHA1.Create())
                {
                    var hash = sha1.ComputeHash(stream);
                    return string.Join(null, hash.Select(x => x.ToString("x2")));
                }
            }
        }
    }
}
