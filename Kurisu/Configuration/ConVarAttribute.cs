using System;

namespace Kurisu.Configuration
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ConVarAttribute : Attribute
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
        public ConVarAttribute(string name)
        {
            Name = name;
        }
    }
}
