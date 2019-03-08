using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.WebSocket;
using DiscordBot.BlueBot.Core;
using DiscordBot_BlueBot;
using Newtonsoft.Json;

namespace DiscordBot.BlueBot
{
    class Utilities
    {
        private static Dictionary<string, string> alerts;

        static Utilities()
        {
            string json = File.ReadAllText("SystemLang/alerts.json");
            var data = JsonConvert.DeserializeObject<dynamic>(json);
            alerts = data.ToObject<Dictionary<string, string>>();
        }

        public static bool ValidateFileExistance(string file)
        {
            if (!File.Exists(file))
            {
                File.WriteAllText(file, "");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Trims characters from the start and the end of the id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Trimmed id consisting only of numbers.</returns>
        public static string CleanId(string id)
        {
            Regex regex = new Regex(@"\d+");
            return regex.Match(id).Value;
            //char[] charsToTrim = {'<', '>', '@', '#', '"', '\'', '/', '\\'};
            //return id.Trim(charsToTrim);
        }
    }
}
