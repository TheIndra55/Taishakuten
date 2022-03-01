using dnYara;
using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Taishakuten.Modules
{
    class Scan : BaseExtension
    {
        private YaraContext _context;
        private CompiledRules _rules;
        private Scanner _scanner;

        private HttpClient _client;

        protected override void Setup(DiscordClient client)
        {
            client.MessageCreated += MessageCreated;

            // compile all yara rules from the rules directory
            var rules = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "rules"), "*.yara");

            _context = new YaraContext();

            using var compiler = new Compiler();
            foreach (var rule in rules)
            {
                compiler.AddRuleFile(rule);
            }

            _rules = compiler.Compile();
            _scanner = new Scanner();

            // construct httpclient for downloading files
            _client = new HttpClient();
            _client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Taishakuten 1.0)");
        }

        private async Task MessageCreated(DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            // TODO guild settings check

            foreach (var attachment in e.Message.Attachments)
            {
                // download the attachment
                var response = await _client.GetAsync(attachment.Url);

                var stream = await response.Content.ReadAsStreamAsync();
                var hash = ComputeHash(stream);

                // scan the file
                var results = _scanner.ScanStream(stream, _rules);

                if (results.Count > 0)
                {
                    var embed = new DiscordEmbedBuilder()
                        .WithTitle(Path.GetFileName(attachment.Url))
                        .WithDescription(hash);

                    foreach (var result in results.Where(x => x.Matches.Count > 0).Take(4))
                    {
                        embed.AddField("yara:" + result.MatchingRule.Identifier, result.Matches.First().Value[0].ToString());
                    }

                    await e.Channel.SendMessageAsync(embed);
                }
            }
        }

        /// <summary>
        /// Computes the sha256 hash of a stream
        /// </summary>
        /// <param name="stream">The stream to compute the hash of</param>
        /// <returns>The sha256 hash</returns>
        private string ComputeHash(Stream stream)
        {
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(stream);

            // rewind stream back after we touched it
            stream.Position = 0;

            return string.Join(null, hash.Select(x => x.ToString("x2")));
        }
    }
}
