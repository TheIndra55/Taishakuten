using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using RethinkDb.Driver;

namespace Kurisu.Commands
{
    [Group("welcome"), Aliases("joinmessage"), Description("Change properties for the welcome message"), RequireUserPermissions(Permissions.ManageGuild)]
    class Welcome
    {
        public RethinkDB R = RethinkDB.R;

        [Command("preview"), Description("Preview the current welcome message")]
        public async Task Preview(CommandContext ctx)
        {
            var welcome = Program.Guilds[ctx.Guild.Id].Welcome;

            var embed = new DiscordEmbedBuilder()
                .WithTitle(string.Format(welcome.Header, ctx.User.Username))
                .WithDescription(welcome.Body)
                .WithColor(new DiscordColor(welcome.Color))
                .WithThumbnailUrl(ctx.User.AvatarUrl)
                .Build();

            await ctx.RespondAsync(embed: embed, content: welcome.Mention ? ctx.Member.Mention : null);
        }

        [Command("body"), Description("Set the body")]
        public async Task Body(CommandContext ctx, string body)
        {
            Program.Guilds[ctx.Guild.Id].Welcome.Body = body;
            await ctx.RespondAsync("Body has been updated.");

            await Update(ctx.Guild.Id);
        }

        [Command("title"), Description("Set the title, use {0} to display the user's username")]
        public async Task Title(CommandContext ctx, string title)
        {
            Program.Guilds[ctx.Guild.Id].Welcome.Header = title;
            await ctx.RespondAsync("Title has been updated.");

            await Update(ctx.Guild.Id);
        }

        [Command("channel"), Description("Set the channel in which the welcome message will be send")]
        public async Task Body(CommandContext ctx, DiscordChannel channel)
        {
            Program.Guilds[ctx.Guild.Id].Welcome.Channel = channel.Id.ToString();
            await ctx.RespondAsync("Channel has been updated.");

            await Update(ctx.Guild.Id);
        }

        [Command("disable"), Description("Disable welcome messages for this guild")]
        public async Task Body(CommandContext ctx)
        {
            Program.Guilds[ctx.Guild.Id].Welcome.Channel = null;
            await ctx.RespondAsync("Welcome messages have been disabled in this guild.");

            await Update(ctx.Guild.Id);
        }

        [Command("color"), Description("Set the color")]
        public async Task Color(CommandContext ctx, int color)
        {
            Program.Guilds[ctx.Guild.Id].Welcome.Color = color;
            await ctx.RespondAsync("Color has been updated.");

            await Update(ctx.Guild.Id);
        }

        [Command("mention"), Description("Set whenever it should ping the user in the message")]
        public async Task Color(CommandContext ctx, bool mention)
        {
            Program.Guilds[ctx.Guild.Id].Welcome.Mention = mention;
            await ctx.RespondAsync("Mentions in welcome messages are now " + (mention ? "enabled." : "disabled."));

            await Update(ctx.Guild.Id);
        }

        public async Task Update(ulong guild)
        {
            await R.Table("guilds").Get(guild.ToString()).Update(Program.Guilds[guild]).RunAsync(Program.Connection);
        }
    }
}
