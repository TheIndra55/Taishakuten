using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using DSharpPlus;
using Kurisu.Configuration;
using Kurisu.Models;
using RethinkDb.Driver;
using RethinkDb.Driver.Net;

namespace Kurisu.Modules
{
    class Scheduler : BaseModule
    {
        public static RethinkDB R = RethinkDB.R;

        private DiscordClient _client;
        private Timer _timer;

        [ConVar("scheduler_timeout", HelpText = "The time between polling the database for reminders")]
        public static int Timeout { get; set; } = 30;

        protected override void Setup(DiscordClient client)
        {
            _client = client;

            _timer = new Timer(Poll, null, 0, (int)TimeSpan.FromSeconds(Timeout).TotalMilliseconds);
        }

        private async void Poll(object state)
        {
            // get all reminders which passed and did not fire yet
            Cursor<Reminder> cursor = await R
                .Table("reminders")
                .Filter(x => 
                    x["remind_at"].Lt(R.Now())
                        .And(x["is_fired"]
                            .Eq(false)))
                .RunCursorAsync<Reminder>(Program.Connection);

            // return if not any
            if(cursor.BufferedSize == 0) return;

            foreach (var reminder in cursor)
            {
                try
                {
                    var channel = await _client.GetChannelAsync(ulong.Parse(reminder.ChannelId));
                    var user = await _client.GetUserAsync(ulong.Parse(reminder.UserId));

                    await channel.SendMessageAsync($"⏰ {user.Mention} you wanted to be reminded about: {reminder.Message}.");
                }
                catch(Exception ex)
                {
                    reminder.LastError = ex.Message;
                }

                // set is_fired to true and update record in database
                reminder.Fired = true;
                await R
                    .Table("reminders")
                    .Get(reminder.Id)
                    .Update(reminder)
                    .RunAsync(Program.Connection);
            }
        }
    }
}
