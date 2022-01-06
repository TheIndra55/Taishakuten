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
using Taishakuten.Modules;
using Microsoft.EntityFrameworkCore;

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

            // setup database context
            var serverVersion = ServerVersion.AutoDetect(config.ConnectionString);

            var database = new DbContextOptionsBuilder<DatabaseContext>()
                .UseMySql(config.ConnectionString, serverVersion)
                .Options;

            // setup discord 
            var client = new DiscordClient(new DiscordConfiguration
            {
                Token = config.Token,
                MinimumLogLevel = LogLevel.Debug
            });

            var scheduler = new Scheduler(new DatabaseContext(database));
            client.AddExtension(scheduler);

            var slash = client.UseSlashCommands(new SlashCommandsConfiguration
            {
                // inject config and database to commands
                Services = new ServiceCollection()
                    .AddSingleton(config)
                    // not sure how to pass DbContextOptionsBuilder here
                    .AddDbContext<DatabaseContext>(options => options.UseMySql(config.ConnectionString, serverVersion))
                    .BuildServiceProvider()
            });

            slash.SlashCommandErrored += async (sender, args) =>
            {
                if (args.Exception is SlashExecutionChecksFailedException failed)
                {
                    var check = failed.FailedChecks[0];

                    if (check is SlashRequirePermissionsAttribute)
                        await args.Context.CreateResponseAsync("Bot or user lacks permission for this command", true);

                    return;
                }

                await args.Context.Channel.SendMessageAsync("An error occurred while executing the command:\n```" + args.Exception.Message + "```");
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
