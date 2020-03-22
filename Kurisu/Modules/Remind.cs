using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Humanizer;
using Kurisu.Models;
using RethinkDb.Driver;

namespace Kurisu.Modules
{
    class Remind
    {
        public static RethinkDB R = RethinkDB.R;

        [Command("remindme"), Aliases("reminder", "remind"), Description("Set a reminder")]
        public async Task Reminder(CommandContext ctx, [Description("The amount of time in which to remind")] TimeSpan offset, [RemainingText, Description("Description about the reminder")] string message = "")
        {
            var reminder = new Reminder
            {
                Message = message,
                ChannelId = ctx.Channel.Id.ToString(),
                GuildId = ctx.Guild.Id.ToString(),
                UserId = ctx.User.Id.ToString(),
                At = DateTime.Now + offset
            };

            var cursor = await R.Table("reminders").Insert(reminder).RunAsync(Program.Connection);
            await ctx.RespondAsync($"⏰ Timer set for {offset.Humanize(2)}.");
        }

        [Command("reminders"), Description("List all reminders.")]
        public async Task Reminders(CommandContext ctx)
        {
            // get all reminders
            var reminders = await R.Table("reminders")
                .Filter(x => x["user_id"].Eq(ctx.User.Id.ToString())).RunCursorAsync<Reminder>(Program.Connection);

            if (reminders.BufferedSize == 0)
            {
                await ctx.RespondAsync("You don't have any reminders.");
                return;
            }

            var response = string.Join("\n", reminders.Select(x => $"{x.At.Humanize(false)}: {x.Message}"));
            await ctx.RespondAsync(response);
        }

        [Command("cancel"), Description("Cancel a reminder.")]
        public async Task Cancel(CommandContext ctx)
        {
            // get all active reminders
            var reminders = await R.Table("reminders")
                .Filter(x => x["user_id"].Eq(ctx.User.Id.ToString()).And(x["is_fired"].Eq(false)))
                .RunCursorAsync<Reminder>(Program.Connection);

            if (reminders.BufferedSize == 0)
            {
                await ctx.RespondAsync("You don't have any active reminders.");
                return;
            }

            var items = reminders.BufferedItems;

            var embed = new DiscordEmbedBuilder()
                .WithDescription(string.Join("\n", reminders.Select((v, i) => $"`{i}` {v.Message}")))
                .Build();

            await ctx.RespondAsync("Please type which reminder you want to cancel.", false, embed);

            var interactivity = ctx.Client.GetInteractivityModule();

            // wait for user response with an integer
            var msg = await interactivity.WaitForMessageAsync(m => m.Author.Id == ctx.User.Id && int.TryParse(m.Content, out _), TimeSpan.FromMinutes(1));
            if (msg != null)
            {
                // get the reminder which user wants to cancel
                var reminder = items.ElementAtOrDefault(int.Parse(msg.Message.Content));
                if (reminder == null)
                {
                    await ctx.RespondAsync("That item doesn't exist");
                    return;
                }

                await R.Table("reminders").Get(reminder.Id).Delete().RunAsync(Program.Connection);
                await ctx.RespondAsync("This reminder has been canceled.");
            }
        }
    }
}
