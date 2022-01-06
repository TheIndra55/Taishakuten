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
using DSharpPlus.SlashCommands.Attributes;

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

            slash.SlashCommandErrored += async (sender, args) =>
            {
                if (args.Exception is SlashExecutionChecksFailedException failed)
                {
                    var check = failed.FailedChecks[0];

                    if (check is SlashRequirePermissionsAttribute)
                        await args.Context.CreateResponseAsync("Bot or user lacks permission for this command", true);
                }
            };

            // register slash commands
            slash.RegisterCommands<Info>(config.Guild);
            slash.RegisterCommands<Remind>(config.Guild);
            slash.RegisterCommands<Reminders>(config.Guild);
            slash.RegisterCommands<Moderation>(config.Guild);

            await client.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}
