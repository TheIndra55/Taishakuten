using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Humanizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Taishakuten.Entities;

namespace Taishakuten.Commands
{
    class Remind : ApplicationCommandModule
    {
        private DatabaseContext _db;

        public Remind(DatabaseContext db)
        {
            _db = db;
        }

        [SlashCommand("remindme", "Set a reminder")]
        public async Task RemindCommand(InteractionContext ctx,
            [Option("timespan", "The amount of time in which to remind")] TimeSpan? span,
            [Option("message", "The message to get reminded about")] string message)
        {
            if (span == null)
            {
                await ctx.CreateResponseAsync($"The time span is not valid, try for example '3d 4h' to remind in 3 days and 4 hours", true);
                return;
            }

            var reminder = new Reminder
            {
                Message = message,

                Guild = ctx.Guild.Id,
                Channel = ctx.Channel.Id,
                User = ctx.User.Id,

                At = DateTime.Now + span.Value
            };

            _db.Add(reminder);
            _db.SaveChanges();

            await ctx.CreateResponseAsync($"⏰ Timer set for {span.Value.Humanize(2)}.");
        }
    }

    [SlashCommandGroup("reminders", "Manage all your reminders")]
    class Reminders : ApplicationCommandModule
    {
        private DatabaseContext _db;

        public Reminders(DatabaseContext db)
        {
            _db = db;
        }

        [SlashCommand("list", "List all reminders")]
        public async Task ListCommand(InteractionContext ctx)
        {
            var reminders = _db.Reminders.Where(x => x.User == ctx.User.Id).OrderByDescending(x => x.At).Take(5);

            var embed = new DiscordEmbedBuilder()
                .WithTitle("Closest 5 reminders");

            foreach (var reminder in reminders)
            {
                embed.AddField(reminder.Message, reminder.At.Humanize(false));
            }

            await ctx.CreateResponseAsync(embed);
        }

        [SlashCommand("cancel", "Cancel a reminder")]
        public async Task CancelCommand(InteractionContext ctx,
            [Option("reminder", "The reminder to cancel")]
            [Autocomplete(typeof(ReminderChoiceProvider))] string id) 
        {
            if (!int.TryParse(id, out var num))
            {
                await ctx.CreateResponseAsync("Please provide the reminder id or use the autocomplete", true);
                return;
            }

            var reminder = _db.Reminders.Where(x => x.Id == num).FirstOrDefault();

            if (reminder == default || reminder.User != ctx.User.Id)
            {
                await ctx.CreateResponseAsync("Reminder does not exist", true);
                return;
            }

            _db.Remove(reminder);
            _db.SaveChanges();

            await ctx.CreateResponseAsync("⏰ Reminder has been cancelled");
        }
    }

    class ReminderChoiceProvider : IAutocompleteProvider
    {
        public Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx)
        {
            // somehow dependency injection does not work in IAutocompleteProvider?
            var db = ctx.Services.GetService(typeof(DatabaseContext)) as DatabaseContext;

            var choices = db.Reminders.Where(x => x.User == ctx.User.Id && !x.Fired).OrderByDescending(x => x.At).Take(25);

            return Task.FromResult<IEnumerable<DiscordAutoCompleteChoice>>(
                choices.Select(x => new DiscordAutoCompleteChoice(x.Message.Substring(0, 20), x.Id.ToString())));
        }
    }
}
