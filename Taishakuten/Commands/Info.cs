using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Threading.Tasks;

namespace Taishakuten.Commands
{
    class Info : ApplicationCommandModule
    {
        [SlashCommand("avatar", "Get the avatar from a user")]
        public async Task AvatarCommand(InteractionContext ctx, [Option("user", "The user to get the avatar from")] DiscordUser user = null)
        {
            if (user == null)
            {
                user = ctx.Guild == null ? ctx.User : ctx.Member;
            }

            var avatar = user is DiscordMember ? (user as DiscordMember).GuildAvatarUrl : user.AvatarUrl;

            var embed = new DiscordEmbedBuilder()
                .WithTitle($"Avatar of {user.Username}")
                .WithImageUrl(avatar);

            await ctx.CreateResponseAsync(embed);
        }
    }
}
