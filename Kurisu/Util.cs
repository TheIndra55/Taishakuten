namespace Kurisu
{
    class Util
    {
        // based on https://github.com/discordjs/discord.js/blob/44ac5fe6dfbab21bb4c16ef580d1101167fd15fd/src/util/Util.js#L547
        /// <summary>
        /// Breaks user, role and everyone/here mentions by adding a zero width space after every @ character
        /// </summary>
        /// <param name="str">The string to sanitize</param>
        public static string RemoveMentions(string str)
        {
            return str.Replace("@", "@\u200b");
        }
    }
}
