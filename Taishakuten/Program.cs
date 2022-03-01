using DSharpPlus;
using DSharpPlus.SlashCommands;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Taishakuten.Commands;
using System.IO;
using System;
using System.Linq;
using System.Text.Json;
using Taishakuten.Entities;
using Microsoft.Extensions.DependencyInjection;
using DSharpPlus.SlashCommands.Attributes;
using Taishakuten.Modules;
using Microsoft.EntityFrameworkCore;
using DSharpPlus.EventArgs;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

namespace Taishakuten
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await new Program().MainAsync(args);
        }

        private DbContextOptions<DatabaseContext> _context;

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

            _context = new DbContextOptionsBuilder<DatabaseContext>()
                .UseMySql(config.ConnectionString, serverVersion)
                .Options;

            // setup discord 
            var client = new DiscordClient(new DiscordConfiguration
            {
                Token = config.Token,
                MinimumLogLevel = LogLevel.Debug,

                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.GuildMembers
            });

            client.GuildMemberAdded += GuildMemberAdded;

            var scheduler = new Scheduler(new DatabaseContext(_context));
            client.AddExtension(scheduler);

            var scan = new Scan();
            client.AddExtension(scan);

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

                if (args.Exception is NotFoundException)
                {
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

        private async Task GuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs e)
        {
            using var db = new DatabaseContext(_context);

            // fetch the welcome message for this guild
            var welcome = db.Welcomes.FirstOrDefault(x => x.Guild == e.Guild.Id);
            if (welcome == default) return;

            var channel = e.Guild.GetChannel(welcome.Channel);
            if (channel == null) return;

            Func<string, string> format = (text) =>
            {
                // {0} is username, {1} is id, {2} is mention
                return string.Format(text, e.Member.Username, e.Member.Id, e.Member.Mention);
            };

            // build embed and format body and title
            var embed = new DiscordEmbedBuilder()
                .WithTitle(format(welcome.Title))
                .WithDescription(format(welcome.Body))
                .WithColor(welcome.Color)
                .WithThumbnail(e.Member.AvatarUrl);

            await channel.SendMessageAsync(welcome.Mention ? e.Member.Mention : "", embed);
        }
    }
}
