using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using static Kurisu.Configuration.ConVarManager;

namespace Kurisu.Modules
{
    class Administration
    {
        [Command("convar"), RequireOwner]
        public async Task ConVar(CommandContext ctx, string name, string value = null)
        {
            // set or get?
            if (value == null)
            {
                if (!DoesConVarExist(name)) return;
                var convar = GetConVar(name);

                await ctx.RespondAsync($"\"{name}\" is set to \"{convar.Value}\"\ntype is \"{convar.Value.GetType()}\"");
                return;
            }

            SetConVar(name, value);
            return;
        }
    }
}
