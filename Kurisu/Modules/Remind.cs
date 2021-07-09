using System;
using System.Collections.Generic;
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

            await R.Table("reminders").Insert(reminder).RunAsync(Program.Connection);
            await ctx.RespondAsync($"⏰ Timer set for {offset.Humanize(2)}.");
        }

        [Command("remindme"), Aliases("reminder", "remind"), Description("Set a reminder")]
        public async Task Reminder(CommandContext ctx, [Description("The date when to be reminded")] DateTime date, [RemainingText, Description("Description about the reminder")] string message = "")
        {
            if (date < DateTime.Now)
            {
                throw new ArgumentException("Date given in parameter must be in the futur", nameof(date));
            }

            var reminder = new Reminder
            {
                Message = message,
                ChannelId = ctx.Channel.Id.ToString(),
                GuildId = ctx.Guild.Id.ToString(),
                UserId = ctx.User.Id.ToString(),
                At = date
            };

            var offset = date - DateTime.Now;

            await R.Table("reminders").Insert(reminder).RunAsync(Program.Connection);
            await ctx.RespondAsync($"⏰ Timer set for {offset.Humanize(2)}.");
        }

        [Command("reminders"), Description("List all reminders.")]
        public async Task Reminders(CommandContext ctx, string filter = null)
        {
            // get reminders
            RethinkDb.Driver.Ast.Filter query = null;
            if(filter != "all")
            {
                query = R.Table("reminders").Filter(x => x["user_id"].Eq(ctx.User.Id.ToString()).And(x["guild_id"].Eq(ctx.Guild.Id.ToString())));
            }
            else
            {
                query = R.Table("reminders").Filter(x => x["user_id"].Eq(ctx.User.Id.ToString()));
            }

            var reminders = await query
                .OrderBy(R.Desc("remind_at"))
                .Limit(5)
                .RunAtomAsync<List<Reminder>>(Program.Connection);

            if (reminders.Count == 0)
            {
                await ctx.RespondAsync($"You don't have any reminders{(filter != "all" ? " in this guild" : "")}.");
                return;
            }

            var hints = new string[] { 
                "Use d!cancel to cancel any future reminder",
                "Use d!reminders all to see all your reminders across servers" };

            var hint = filter == "all" ? hints[0] : hints[new Random().Next(0, hints.Length)];

            var embed = new DiscordEmbedBuilder()
                .WithTitle("Closest 5 reminders " + (filter == "all" ? "(all)" : "(guild)"))
                .WithFooter(hint);

            reminders.ForEach(x => embed.AddField(x.Message, x.At.Humanize(false)));

            await ctx.RespondAsync(embed: embed.Build());
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
