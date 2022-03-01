using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using System.Threading.Tasks;

namespace Taishakuten.Commands
{
    class Moderation : ApplicationCommandModule
    {
        [SlashCommand("purge", "Delete messages from the current channel.")]
        [SlashRequirePermissions(DSharpPlus.Permissions.ManageMessages)]
        public async Task PurgeCommand(InteractionContext ctx, [Option("amount", "Amount of messages to delete (max 100)")] long amount)
        {
            if (amount < 1 || amount > 100)
            {
                await ctx.CreateResponseAsync("Amount of messages to delete must be between 1 and 100", true);
                return;
            }

            var messages = await ctx.Channel.GetMessagesAsync((int)amount);
            await ctx.Channel.DeleteMessagesAsync(messages, "Purged by " + ctx.User.Username);

            await ctx.CreateResponseAsync("👌");
        }
    }
}
