using System;
using System.Collections.Generic;

namespace Kurisu.Configuration
{
    static class ConVarManager
    {
        public static Dictionary<string, ConVar> ConVars = new Dictionary<string, ConVar>();

        public static ConVar RegisterConVar(string name, Action callback = null)
        {
            // if already exists locally set value
            if (ConVars.ContainsKey(name))
            {
                ConVars[name].Callback = callback;
                return ConVars[name];
            }

            var convar = new ConVar
            {
                Callback = callback
            };

            ConVars.Add(name, convar);
            return ConVars[name];
        }

        public static void SetConVar(string name, string value)
        {
            int integer;
            object val = value;

            // try to parse it as an integer
            if (int.TryParse(value, out integer))
            {
                val = integer;
            }

            if (ConVars.ContainsKey(name))
            {
                if (ConVars[name].Value != null && ConVars[name].Value.GetType() != val.GetType())
                    throw new Exception($"Type '{val.GetType()}' does not match convar type '{ConVars[name].Value.GetType()}'");

                ConVars[name].Value = val;
            }
            else
            {
                ConVars.Add(name, new ConVar
                {
                    Value = val
                });
            }
        }

        public static bool DoesConVarExist(string name)
        {
            return ConVars.ContainsKey(name);
        }

        public static ConVar GetConVar(string name)
        {
            return ConVars[name];
        }
    }
}
