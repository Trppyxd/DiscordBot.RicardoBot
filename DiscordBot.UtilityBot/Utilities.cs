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
// ReSharper disable All

namespace DiscordBot.BlueBot
{
    class Utilities
    {
        // ReSharper disable once NotAccessedField.Local
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

        public static void LogConsole(LogType type, string message)
        {
            switch (type)
            {
                case LogType.DEBUG:
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("[LOG]" + message);
                        break;
                    }
                case LogType.ERROR:
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("[ERROR]" + message);
                        break;
                    }
                case LogType.WARNING:
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("[WARNING]" + message);
                        break;
                    }
                case LogType.DATABASE:
                    {
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        Console.WriteLine("[DB]" + message);
                        break;
                    }
                case LogType.DATABASE_ERROR:
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("[DB-ERROR]" + message);
                    break;
                    }
                default:
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("[DEFAULT_LOGTYPE]" + message);
                    break;
            }
            Console.ResetColor();
        }

        /// <summary>
        /// Used for <see cref="Utilities.LogConsole"/>
        /// </summary>
        public enum LogType
        {
            DEBUG = 0,
            ERROR = 1,
            WARNING = 2,
            DATABASE = 3,
            DATABASE_ERROR = 4
        }

        /// <summary>
        /// Trims characters from the start and the end of the id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Trimmed id consisting only of numbers.</returns>
        public static string CleanId(string id)
        {
            Regex regex = new Regex(@"\d+");
            var result = regex.Matches(id).Cast<Match>().Select(x => x.Value).ToArray();
            return String.Join(" ", result);
        }
    }
}
