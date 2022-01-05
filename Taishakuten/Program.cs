using DSharpPlus;
using DSharpPlus.SlashCommands;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Taishakuten.Commands;
using System.IO;
using System;
using System.Text.Json;
using Taishakuten.Entities;
using Microsoft.Extensions.DependencyInjection;

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
            // read the config
            var path = Path.Combine(AppContext.BaseDirectory, "config.json");

            if (!File.Exists(path))
            {
                Console.WriteLine("config.json not found, aborting.");
                return;
            }

            var config = JsonSerializer.Deserialize<Configuration>(File.ReadAllText(path));

            // database stuff
            var database = new DatabaseContext(config.ConnectionString);

            // setup discord 
            var client = new DiscordClient(new DiscordConfiguration
            {
                Token = config.Token,
                MinimumLogLevel = LogLevel.Debug
            });

            var slash = client.UseSlashCommands(new SlashCommandsConfiguration
            {
                // inject config to commands
                Services = new ServiceCollection()
                    .AddSingleton(config)
                    .AddSingleton(database)
                    .BuildServiceProvider()
            });

            slash.RegisterCommands<Info>(832001341865197579);
            slash.RegisterCommands<Remind>(832001341865197579);

            await client.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}
