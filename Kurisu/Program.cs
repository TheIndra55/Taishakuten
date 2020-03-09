using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using Kurisu.Modules;
using Kurisu.Configuration;
using Kurisu.External.VirusTotal;
using Kurisu.Models;
using RethinkDb.Driver;
using RethinkDb.Driver.Net;
using static Kurisu.Configuration.ConVarManager;
using VirusScan = Kurisu.Commands.VirusScan;
using Welcome = Kurisu.Commands.Welcome;
using Scan = Kurisu.Modules.Scan;

namespace Kurisu
{
    class Program
    {
        public static RethinkDB R = RethinkDB.R;

        private static DiscordClient _discord { get; set; }
        private static InteractivityModule _interactivity { get; set; }

        public static Connection Connection { get; private set; }

        private static CommandsNextModule Commands { get; set; }

        public static ConVar Token { get; set; }
        public static ConVar Game { get; set; }
        public static ConVar Database { get; set; }

        public static ConVar VirusTotalKey { get; set; }

        public static Dictionary<ulong, Guild> Guilds = new Dictionary<ulong, Guild>();

        static void Main(string[] args)
        {
            MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            // register convars
            Token = RegisterConVar("token");
            Game = RegisterConVar("game", () =>
            {
                if (_discord == null) return;

                _discord.UpdateStatusAsync(new DiscordGame((string) Game.Value));
            });
            Database = RegisterConVar("database");
            VirusTotalKey = RegisterConVar("virustotal_key");

            if (args.Length == 0)
            {
                Console.WriteLine("No config file executed. Launch with 'dotnet Kurisu.dll bot.cfg'");
                return;
            }

            // execute launch file
            ReadConfig(args[0]);

            // connect to database
            var host = RegisterConVar("database_host");

            Connection = R.Connection()
                .Hostname((string) host.Value)
                .Port(RethinkDBConstants.DefaultPort)
                .Timeout(RethinkDBConstants.DefaultTimeout)
                .Connect();

            // initialize Discord bot
            _discord = new DiscordClient(new DiscordConfiguration
            {
                Token = (string) Token.Value,
                TokenType = TokenType.Bot,
                LogLevel = LogLevel.Debug,
                UseInternalLogHandler = true
            });

            _interactivity = _discord.UseInteractivity(new InteractivityConfiguration());
            _discord.AddModule(new Scheduler());

            // setup virusscan module
            var scan = new Scan
            {
                VirusTotal = new VirusTotal((string) VirusTotalKey.Value)
            };

            VirusTotalKey.Callback = () => { scan.VirusTotal.Key = (string) VirusTotalKey.Value; };

            _discord.AddModule(scan);

            // initialize CommandsNext
            Commands = _discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefix = "d!",
                EnableDms = false
            });

            // register commands
            Commands.RegisterCommands<Administration>();
            Commands.RegisterCommands<Information>();
            Commands.RegisterCommands<Moderation>();
            Commands.RegisterCommands<Remind>();

            Commands.RegisterCommands<Welcome>();
            Commands.RegisterCommands<VirusScan>();

            Commands.CommandErrored += async e =>
            {
                await e.Context.RespondAsync($"An error occured while executing the command:\n`{e.Exception.Message}`");
            };

            _discord.Ready += async e =>
            {
                await _discord.UpdateStatusAsync(new DiscordGame((string) Game.Value));
            };

            _discord.GuildMemberAdded += GuildMemberAdded;
            _discord.GuildAvailable += GuildAvailable;

            await _discord.ConnectAsync();
            await Task.Delay(-1);
        }

        /// <summary>
        /// Triggered when an user join a guild
        /// </summary>
        private static async Task GuildMemberAdded(GuildMemberAddEventArgs e)
        {
            var welcome = Guilds[e.Guild.Id].Welcome;
            if (welcome.Channel == null) return;

            var channel = e.Guild.GetChannel(ulong.Parse(welcome.Channel));
            if(channel == null) return;

            var embed = new DiscordEmbedBuilder()
                .WithTitle(string.Format(welcome.Header, e.Member.Username))
                .WithDescription(welcome.Body)
                .WithColor(new DiscordColor(welcome.Color))
                .WithThumbnailUrl(e.Member.AvatarUrl)
                .Build();

            await channel.SendMessageAsync(embed: embed, content: welcome.Mention ? e.Member.Mention : null);
        }

        /// <summary>
        /// Triggered when a new guild has become available
        /// </summary>
        private static async Task GuildAvailable(GuildCreateEventArgs e)
        {
            // check if guild exists in the database
            var guild = await R.Db(Database.Value).Table("guilds").Get(e.Guild.Id.ToString()).RunResultAsync<Guild>(Connection);
            if (guild == null)
            {
                // doesn't exist insert
                guild = new Guild
                {
                    Id = e.Guild.Id.ToString()
                };
                Guilds.Add(e.Guild.Id, guild);

                await R.Db(Database.Value).Table("guilds").Insert(guild).RunAsync(Connection);

                return;
            }

            // add to memory
            Guilds.Add(e.Guild.Id, guild);
        }

        /// <summary>
        /// Reads the config file by path into convars
        /// </summary>
        /// <param name="file">Path to the config file</param>
        private static void ReadConfig(string file)
        {
            // todo protection
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, file);
            var lines = File.ReadAllLines(path);

            foreach (var line in lines)
            {
                var split = line.Split(' ');

                var name = split[0];
                var value = string.Join(" ", split.Skip(1));

                SetConVar(name, value);
            }
        }
    }
}
