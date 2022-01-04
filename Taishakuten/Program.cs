using DSharpPlus;
using DSharpPlus.SlashCommands;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Taishakuten.Commands;

namespace Taishakuten
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await new Program().MainAsync(args);
        }

        public async Task MainAsync(string[] args)
        {
            var client = new DiscordClient(new DiscordConfiguration
            {
                Token = "",
                MinimumLogLevel = LogLevel.Debug
            });

            var slash = client.UseSlashCommands();

            slash.RegisterCommands<Info>(832001341865197579);

            await client.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}
