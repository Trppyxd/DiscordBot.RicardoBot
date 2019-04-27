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
        public static bool ValidateTextFileExistance(string file)
        {
            if (!File.Exists(file))
            {
                File.WriteAllText(file, "");
                return false;
            }
            return true;
        }

        public static bool ValidateDirectoryExistance(string directoryPath)
        {
            List<string> dirlist = new List<string>(directoryPath.Split('\\'));
            string path = "";

            for (int i = 0; i < dirlist.Count; i++)
            {
                path += dirlist[i] + '\\';
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }

            return true;
        }

        public static void WriteToLog(string msg, bool sortByDay = true)
        {
            string path = "";
            if (sortByDay == true)
                path = $@"{AppDomain.CurrentDomain.BaseDirectory}Core\logs-{DateTime.UtcNow:dd-MM-yyyy}.txt";
            else
                path = $@"{AppDomain.CurrentDomain.BaseDirectory}Core\logs.txt";

            ValidateTextFileExistance(path);
            using (var sw = new StreamWriter(path, true))
            {
                sw.WriteLine(msg);
            }
        }

        public static void LogConsole(LogType type, string message)
        {
            string resultString;
            switch (type)
            {
                case LogType.DEBUG:
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        resultString = $"[LOG]{DateTime.UtcNow:dd/MM/yy hh:mm:ss} > " + message;
                        break;
                    }
                case LogType.ERROR:
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        resultString = $"[ERROR]{DateTime.UtcNow:dd/MM/yy hh:mm:ss} > " + message;
                        break;
                    }
                case LogType.WARNING:
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        resultString = $"[WARNING]{DateTime.UtcNow:dd/MM/yy hh:mm:ss} > " + message;
                        break;
                    }
                case LogType.DATABASE:
                    {
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        resultString = $"[DB]{DateTime.UtcNow:dd/MM/yy hh:mm:ss} > " + message;
                        break;
                    }
                case LogType.DATABASE_ERROR:
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        resultString = $"[DB-ERROR]{DateTime.UtcNow:dd/MM/yy hh:mm:ss} > " + message;
                        break;
                    }
                case LogType.USER_LEFT:
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        resultString = $"[LEFT]{DateTime.UtcNow:dd/MM/yy hh:mm:ss} > " + message;
                        break;
                    }
                case LogType.USER_JOINED:
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        resultString = $"[JOINED]{DateTime.UtcNow:dd/MM/yy hh:mm:ss} > " + message;
                        break;
                    }
                default:
                    Console.ForegroundColor = ConsoleColor.White;
                    resultString = $"[DEFAULT_LOGTYPE]{DateTime.UtcNow:dd/MM/yy hh:mm:ss K} > " + message;
                    break;
            }
            Console.WriteLine(resultString);
            Utilities.WriteToLog(resultString);
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
            DATABASE_ERROR = 4,
            USER_JOINED = 5,
            USER_LEFT = 6
        }

        /// <summary>
        /// Trims characters from the start and the end of the id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Trimmed id consisting only of numbers.</returns>
        public static ulong[] CleanId(string id)
        {
            Regex regex = new Regex(@"\d+");
            var matches = regex.Matches(id).Cast<Match>();
            ulong[] idArray = new ulong[matches.Count()];

            int i = 0;
            foreach (var match in matches)
            {
                idArray[i] = Convert.ToUInt64(match.Value);
                i++;
            }

            return idArray;
        }
    }
}
