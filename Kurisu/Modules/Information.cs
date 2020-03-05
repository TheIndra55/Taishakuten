using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Humanizer;
using Newtonsoft.Json;

namespace Kurisu.Modules
{
    class Information
    {
        [Command("about"), Aliases("info"), Description("Show information about the bot")]
        public async Task About(CommandContext ctx)
        {
            var embed = new DiscordEmbedBuilder()
                .WithTitle(ctx.Client.CurrentUser.Username)
                .AddField("Author", "TheIndra", true)
                .AddField("Software", "Kurisu", true)
                .AddField("Library", "DSharpPlus", true)
                .AddField("Guilds", ctx.Client.Guilds.Count.ToString(), true)
                .AddField("Uptime", (DateTime.Now - Process.GetCurrentProcess().StartTime).Humanize(), true);

            await ctx.RespondAsync("", false, embed);
        }

        [Command("guild"), Aliases("server", "serverinfo"), Description("Shows information about the current guild")]
        public async Task Guild(CommandContext ctx)
        {
            var guild = ctx.Guild;

            var embed = new DiscordEmbedBuilder()
                .WithTitle(guild.Name)
                .WithDescription(guild.Id.ToString())
                .AddField("Owner", guild.Owner.Username)
                .AddField("Members", guild.MemberCount.ToString())
                .AddField("Age", $"{guild.CreationTimestamp.Humanize()} ({guild.CreationTimestamp:g})")
                .AddField("Region", guild.RegionId)
                .WithThumbnailUrl(guild.IconUrl)
                .Build();

            await ctx.RespondAsync(embed: embed);
        }

        [Command("user"), Aliases("whois", "userinfo", "member"), Description("See info about a member")]
        public async Task User(CommandContext ctx, DiscordMember user)
        {
            var embed = new DiscordEmbedBuilder()
                .WithTitle($"{user.Username}#{user.Discriminator}")
                .WithDescription(user.Id.ToString())
                .AddField("Account creation", $"{user.CreationTimestamp.Humanize()} ({user.CreationTimestamp:g})")
                .AddField("Guild join", $"{user.JoinedAt.Humanize()} ({user.JoinedAt:g})")
                .AddField("Roles", string.Join(", ", user.Roles.Select(x => $"`{x.Name}`")))
                .WithThumbnailUrl(user.AvatarUrl)
                .Build();

            await ctx.RespondAsync(embed: embed);
        }

        [Command("avatar"), Aliases("pf", "pic"), Description("Shows the user's avatar")]
        public async Task Avatar(CommandContext ctx, DiscordUser user)
        {
            var embed = new DiscordEmbedBuilder()
                .WithTitle($"Avatar of {user.Username}")
                .WithImageUrl(user.AvatarUrl)
                .Build();

            await ctx.RespondAsync(embed: embed);
        }

        [Command("channel"), Description("See info about a channel")]
        public async Task Channel(CommandContext ctx, DiscordChannel chan = null)
        {
            var channel = chan ?? ctx.Channel;

            var embed = new DiscordEmbedBuilder()
                .WithTitle($"#{channel.Name}")
                .WithDescription(channel.Id.ToString())
                .AddField("Age", $"{channel.CreationTimestamp.Humanize()} ({channel.CreationTimestamp:g})");

            if (!string.IsNullOrWhiteSpace(channel.Topic))
            {
                // remove messy newlines
                var topic = channel.Topic.Replace("\n", "");

                embed.AddField("Topic", topic.Substring(0,
                    topic.Length < 300 ? topic.Length : 300));
            }

            if (channel.Type == ChannelType.Voice)
                embed.AddField("Bitrate", $"{channel.Bitrate / 1000}kbps");

            await ctx.RespondAsync(embed: embed.Build());
        }

        [Command("ping"), Description("See the bot's ping")]
        public async Task Ping(CommandContext ctx)
        {
            var message = await ctx.RespondAsync("Pong!");

            var ping = message.CreationTimestamp - ctx.Message.CreationTimestamp;
            await message.ModifyAsync($"Ping: {ping.Humanize()}");
        }

        [Command("settings")]
        public async Task Settings(CommandContext ctx)
        {
            await ctx.RespondAsync(JsonConvert.SerializeObject(Program.Guilds[ctx.Guild.Id]));
        }
    }
}
