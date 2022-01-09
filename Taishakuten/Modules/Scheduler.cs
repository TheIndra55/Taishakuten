using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using DSharpPlus.Entities;

namespace Taishakuten.Modules
{
    class Scheduler : BaseExtension
    {
        private DatabaseContext _db;
        private DiscordClient _client;
        private Timer _timer;

        public Scheduler(DatabaseContext context)
        {
            _db = context;
        }

        protected override void Setup(DiscordClient client)
        {
            _client = client;
            _timer = new Timer(Schedule, null, 0, (int)TimeSpan.FromSeconds(30).TotalMilliseconds);
        }

        private async void Schedule(object state)
        {
            await Schedule();
        }

        public async Task Schedule()
        {
            // very basic logic, TODO schedule closest reminders in memory
			
            var reminders = _db.Reminders.Where(x => !x.Fired && x.At < DateTime.Now);

            foreach (var reminder in reminders)
            {
                try
                {
                    var channel = await _client.GetChannelAsync(reminder.Channel);
                    var user = await _client.GetUserAsync(reminder.User);

                    var message = new DiscordMessageBuilder()
                        .WithContent($"⏰ {user.Mention} you wanted to be reminded about: {reminder.Message}.")
                        .WithAllowedMention(new UserMention(user));

                    await channel.SendMessageAsync(message);
                }
                catch(Exception e)
                {
                    reminder.LastError = e.Message;
                }

                reminder.Fired = true;
            }

            _db.SaveChanges();
        }
    }
}
