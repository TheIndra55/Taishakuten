using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
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

        [ConVar("scheduler_timeout", HelpText = "The time between polling the database for reminders")]
        public static int Timeout { get; set; } = 30;

        protected override void Setup(DiscordClient client)
        {
            _client = client;

            if (!R.DbList().Contains(Program.Database).Run<bool>(Program.Connection))
            {
                R.DbCreate(Program.Database).Run(Program.Connection);
            }
            if (!R.TableList().Contains("reminders").Run<bool>(Program.Connection))
            {
                R.TableCreate("reminders").Run(Program.Connection);
            }

            _ = new Timer(Poll, null, 0, (int)TimeSpan.FromSeconds(Timeout).TotalMilliseconds);
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
                    // fail
                    reminder.LastError = ex.Message;

                    // fallback
                    var user = ulong.Parse(reminder.UserId);

                    try
                    {
                        // why not just expose this
                        var apiClient = _client.GetType().GetProperty("ApiClient", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_client, null);
                        var dm = await (Task<DiscordDmChannel>)apiClient.GetType().GetMethod("CreateDmAsync", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(apiClient, new object[] { user });

                        await dm.SendMessageAsync($"⏰ It seems your reminder in <#{reminder.ChannelId}> failed, your reminder was: {reminder.Message}");
                    }
                    catch(Exception)
                    {
                        // well we tried everything
                    }
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
