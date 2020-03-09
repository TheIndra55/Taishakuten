using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Kurisu.Modules
{
    class Moderation
    {
        [Command("purge"), Aliases("clear"), Description("Clear multiple messages from the channel"), RequirePermissions(Permissions.ManageMessages)]
        public async Task Clear(CommandContext ctx, int messages = 0)
        {
            if (messages < 2 || messages > 100)
            {
                await ctx.RespondAsync("Please specify a number between 2 and 100");
                return;
            }

            var msgs = await ctx.Channel.GetMessagesAsync(messages);
            await ctx.Channel.DeleteMessagesAsync(msgs);

            await ctx.RespondAsync("👌");
        }
    }
}
