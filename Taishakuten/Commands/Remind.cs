using DSharpPlus.SlashCommands;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using System;
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
            [Option("span", "The amount of time in which to remind")] TimeSpan? span,
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
}
