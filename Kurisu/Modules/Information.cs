using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
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

        [Command("guild")]
        public async Task Guild(CommandContext ctx)
        {
            await ctx.RespondAsync(JsonConvert.SerializeObject(Program.Guilds[ctx.Guild.Id]));
        }
    }
}
