using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using RethinkDb.Driver;

namespace Kurisu.Commands
{
    [Group("virusscan"), Aliases("virustotal"), Description("Setup virus scanning for uploads on this guild"), RequirePermissions(Permissions.ManageGuild)]
    class VirusScan
    {
        public RethinkDB R = RethinkDB.R;

        [Command("enable"), Description("Enable virus scanning in this guild")]
        public async Task Enable(CommandContext ctx)
        {
            Program.Guilds[ctx.Guild.Id].VirusScan.Enabled = true;
            await ctx.RespondAsync("Virus scanning has been enabled in this guild.");

            await Update(ctx.Guild.Id);
        }

        [Command("disable"), Description("Disable virus scanning in this guild")]
        public async Task Disable(CommandContext ctx)
        {
            Program.Guilds[ctx.Guild.Id].VirusScan.Enabled = true;
            await ctx.RespondAsync("Virus scanning has been disabled in this guild.");

            await Update(ctx.Guild.Id);
        }

        [Command("extensions"), Description("Set which file extensions should be scanned by the bot")]
        public async Task Extensions(CommandContext ctx, params string[] extensions)
        {
            Program.Guilds[ctx.Guild.Id].VirusScan.Extensions = extensions.ToList();
            await ctx.RespondAsync($"The bot will now scan `{string.Join(", ", extensions)}`");

            await Update(ctx.Guild.Id);
        }

        public async Task Update(ulong guild)
        {
            await R.Db(Program.Database.Value).Table("guilds").Get(guild.ToString()).Update(Program.Guilds[guild]).RunAsync(Program.Connection);
        }
    }
}
