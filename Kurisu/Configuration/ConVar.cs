using System;
using System.Collections.Generic;
using System.Reflection;

namespace Kurisu.Configuration
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ConVar : Attribute
    {
        /// <summary>
        /// The name of the convar
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// A description describing the convar
        /// </summary>
        public string HelpText { get; set; }

        /// <summary>
        /// Mark this property as convar
        /// </summary>
        /// <param name="name"></param>
        public ConVar(string name)
        {
            Name = name;
        }

        // static convar functions
        public static Dictionary<string, KeyValuePair<ConVar, PropertyInfo>> Convars = new Dictionary<string, KeyValuePair<ConVar, PropertyInfo>>();

        /// <summary>
        /// Gets a convar value
        /// </summary>
        /// <param name="convar"></param>
        /// <returns></returns>
        public static object Get(string convar)
        {
            if (Convars.ContainsKey(convar))
            {
                return Convars[convar].Value.GetValue(null);
            }

            return null;
        }

        /// <summary>
        /// Gets a convar value by <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="convar"></param>
        /// <returns></returns>
        public static T Get<T>(string convar)
        {
            var value = Get(convar);

            return value == null ? default : (T)Convert.ChangeType(value, typeof(T));
        }

        /// <summary>
        /// Sets a convar to the specified value, can be literal object or string representing
        /// </summary>
        /// <param name="convar"></param>
        /// <param name="value"></param>
        public static void Set(string convar, object value)
        {
            if (!Convars.ContainsKey(convar)) return;

            // get the convar
            var property = Convars[convar].Value;

            object set;
            // Convert.ChangeType doesn't work with enums (unless passed as enum)
            // so when trying to set an enum convar by string use Enum.Parse(...)
            if (property.PropertyType.IsEnum && value is string)
            {
                set = Enum.Parse(property.PropertyType, (string)value);
            }
            else
            {
                set = Convert.ChangeType(value, property.PropertyType);
            }

            property.SetValue(null, set);
        }
    }
}
