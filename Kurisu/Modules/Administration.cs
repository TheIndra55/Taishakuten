using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Kurisu.Configuration;

namespace Kurisu.Modules
{
    class Administration : BaseCommandModule
    {
        [Command("convar"), RequireOwner]
        public async Task Convar(CommandContext ctx, string name, string value = null)
        {
            // set or get?
            if (value == null)
            {
                var val = ConVar.Get<string>(name);

                var type = ConVar.Convars[name].Value;
                var convar = ConVar.Convars[name].Key;

                await ctx.RespondAsync($"\"{name}\" is set to \"{val}\"\n" +
                    $"type is \"{type.PropertyType}\"\n" +
                    $"defined in \"{type.DeclaringType.FullName}\"\n" +
                    $"description is \"{convar.HelpText}\"");
                return;
            }

            ConVar.Set(name, value);
            return;
        }
    }
}
