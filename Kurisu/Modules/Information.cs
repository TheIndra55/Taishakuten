using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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

        [Command("settings")]
        public async Task Settings(CommandContext ctx)
        {
            await ctx.RespondAsync(JsonConvert.SerializeObject(Program.Guilds[ctx.Guild.Id]));
        }
    }
}
