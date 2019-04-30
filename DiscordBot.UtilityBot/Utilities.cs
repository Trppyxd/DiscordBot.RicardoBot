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

        public static void WriteToLog(LogFileType lfType, string msg, bool sortByDay = true)
        {
            string path = "";
            switch (lfType)
            {

                case LogFileType.ALL:
                    path =
                        sortByDay == true ?
                            $@"{AppDomain.CurrentDomain.BaseDirectory}Core\CONSOLE_Logs-{DateTime.UtcNow:dd-MM-yyyy}UTC.txt" :
                            $@"{AppDomain.CurrentDomain.BaseDirectory}Core\CONSOLE_Logs.txt";
                    break;
                case LogFileType.MESSAGE_PRIVATE:
                    path =
                        sortByDay == true ?
                            $@"{AppDomain.CurrentDomain.BaseDirectory}Core\MESSAGE_PRIVATE_Logs-{DateTime.UtcNow:dd-MM-yyyy}UTC.txt" :
                            $@"{AppDomain.CurrentDomain.BaseDirectory}Core\MESSAGE_PRIVATE_Logs.txt";
                    break;
                case LogFileType.MESSAGE_PUBLIC:
                    path =
                        sortByDay == true ?
                            $@"{AppDomain.CurrentDomain.BaseDirectory}Core\MESSAGE_PUBLIC_Logs-{DateTime.UtcNow:dd-MM-yyyy}UTC.txt" :
                            $@"{AppDomain.CurrentDomain.BaseDirectory}Core\MESSAGE_PUBLIC_Logs.txt";
                    break;
                case LogFileType.USER_JOIN:
                    path =
                        sortByDay == true ?
                            $@"{AppDomain.CurrentDomain.BaseDirectory}Core\USER_JOIN_Logs-{DateTime.UtcNow:dd-MM-yyyy}UTC.txt" :
                            $@"{AppDomain.CurrentDomain.BaseDirectory}Core\USER_JOIN_Logs.txt";
                    break;
                case LogFileType.USER_LEAVE:
                    path =
                        sortByDay == true ?
                            $@"{AppDomain.CurrentDomain.BaseDirectory}Core\USER_LEAVE_Logs-{DateTime.UtcNow:dd-MM-yyyy}UTC.txt" :
                            $@"{AppDomain.CurrentDomain.BaseDirectory}Core\USER_LEAVE_Logs.txt";
                    break;
                case LogFileType.USER_KICKED:
                    path =
                        sortByDay == true ?
                            $@"{AppDomain.CurrentDomain.BaseDirectory}Core\USER_KICKED_Logs-{DateTime.UtcNow:dd-MM-yyyy}UTC.txt" :
                            $@"{AppDomain.CurrentDomain.BaseDirectory}Core\USER_KICKED_Logs.txt";
                    break;
                case LogFileType.USER_BANNED:
                    path =
                        sortByDay == true ?
                            $@"{AppDomain.CurrentDomain.BaseDirectory}Core\USER_BANNED_Logs-{DateTime.UtcNow:dd-MM-yyyy}UTC.txt" :
                            $@"{AppDomain.CurrentDomain.BaseDirectory}Core\USER_BANNED_Logs.txt";
                    break;
                case LogFileType.DATABASE:
                    path =
                        sortByDay == true ?
                            $@"{AppDomain.CurrentDomain.BaseDirectory}Core\DATABASE_Logs-{DateTime.UtcNow:dd-MM-yyyy}UTC.txt" :
                            $@"{AppDomain.CurrentDomain.BaseDirectory}Core\DATABASE_Logs.txt";
                    break;
                case LogFileType.ERROR:
                    path =
                        sortByDay == true ?
                            $@"{AppDomain.CurrentDomain.BaseDirectory}Core\ERROR_Logs-{DateTime.UtcNow:dd-MM-yyyy}UTC.txt" :
                            $@"{AppDomain.CurrentDomain.BaseDirectory}Core\ERROR_Logs.txt";
                    break;
                default:
                    path =
                        sortByDay == true ?
                            $@"{AppDomain.CurrentDomain.BaseDirectory}Core\LOGTYPEDEFAULT_Logs-{DateTime.UtcNow:dd-MM-yyyy}UTC.txt" :
                            $@"{AppDomain.CurrentDomain.BaseDirectory}Core\LOGTYPEDEFAULT_Logs.txt";
                    break;
            }

            ValidateDirectoryExistance($@"{AppDomain.CurrentDomain.BaseDirectory}Core\");
            ValidateTextFileExistance(path);
            using (var sw = new StreamWriter(path, true))
            {
                sw.WriteLine(msg);
            }
        }

        public enum LogFileType
        {
            ALL = 0,
            MESSAGE_PUBLIC = 1,
            MESSAGE_PRIVATE = 2,
            USER_LEAVE = 3,
            USER_JOIN = 4,
            USER_KICKED = 5,
            USER_BANNED = 6,
            DATABASE = 7,
            ERROR = 8,
        }

        public static void LogConsole(LogFormat format, string message)
        {
            string resultString;
            switch (format)
            {
                case LogFormat.DEBUG:
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        resultString = $"[LOG]{DateTime.UtcNow:dd/MM/yy hh:mm:ss}UTC > " + message;
                        break;
                    }
                case LogFormat.NOFORMAT:
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        resultString = "message";
                        break;
                    }
                case LogFormat.ERROR:
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        resultString = $"[ERROR]{DateTime.UtcNow:dd/MM/yy hh:mm:ss}UTC > " + message;
                        Utilities.WriteToLog(LogFileType.ERROR, resultString);
                        break;
                    }
                case LogFormat.WARNING:
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        resultString = $"[WARNING]{DateTime.UtcNow:dd/MM/yy hh:mm:ss}UTC > " + message;
                        Utilities.WriteToLog(LogFileType.ERROR, resultString);
                        break;
                    }
                case LogFormat.DATABASE:
                    {
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        resultString = $"[DB]{DateTime.UtcNow:dd/MM/yy hh:mm:ss}UTC > " + message;
                        Utilities.WriteToLog(LogFileType.DATABASE, resultString);
                        break;
                    }
                case LogFormat.DATABASE_ERROR:
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        resultString = $"[DB-ERROR]{DateTime.UtcNow:dd/MM/yy hh:mm:ss}UTC > " + message;
                        Utilities.WriteToLog(LogFileType.DATABASE, resultString);

                        break;
                    }
                case LogFormat.USER_LEFT:
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        resultString = $"[LEFT]{DateTime.UtcNow:dd/MM/yy hh:mm:ss}UTC > " + message;
                        Utilities.WriteToLog(LogFileType.USER_LEAVE, resultString);
                        break;
                    }
                case LogFormat.USER_JOINED:
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        resultString = $"[JOINED]{DateTime.UtcNow:dd/MM/yy hh:mm:ss}UTC > " + message;
                        Utilities.WriteToLog(LogFileType.USER_JOIN, resultString);
                        break;
                    }
                default:
                    Console.ForegroundColor = ConsoleColor.White;
                    resultString = $"[DEFAULT_LOGTYPE]{DateTime.UtcNow:dd/MM/yy hh:mm:ss}UTC > " + message;
                    break;
            }
            Console.WriteLine(resultString);
            Utilities.WriteToLog(LogFileType.ALL, resultString);
            Console.ResetColor();
        }

        /// <summary>
        /// Used for <see cref="Utilities.LogConsole"/>
        /// </summary>
        public enum LogFormat
        {
            DEBUG = 0,
            ERROR = 1,
            WARNING = 2,
            DATABASE = 3,
            DATABASE_ERROR = 4,
            USER_JOINED = 5,
            USER_LEFT = 6,
            NOFORMAT = 7
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
