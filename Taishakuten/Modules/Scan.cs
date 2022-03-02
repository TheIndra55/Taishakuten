using dnYara;
using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Taishakuten.Modules
{
    class Scan : BaseExtension
    {
        // yara stuff
        private YaraContext _context;
        private CompiledRules _rules;
        private Scanner _scanner;

        private HttpClient _client;
        private DatabaseContext _db;

        public Scan(DatabaseContext context)
        {
            _db = context;
        }

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
            if (!(e.Message.Attachments.Count > 0 &&
                // query could be _db.Guilds.Find but selecting ScanEnabled optimizes the query to only do
                // SELECT `r`.`ScanEnabled` instead of selecting all fields
                _db.Guilds.Where(x => x.Id == e.Guild.Id).Select(x => x.ScanEnabled).FirstOrDefault()))
            {
                return;
            }

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

                    foreach (var result in results.Take(4))
                    {
                        var match = "-";
                        if (result.Matches.Count > 0)
                        {
                            match = result.Matches.First().Value[0].ToString();
                        }

                        embed.AddField("yara:" + result.MatchingRule.Identifier, match);
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
