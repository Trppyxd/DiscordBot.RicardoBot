using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace DiscordBot.BlueBot
{
    class Program
    {
        private DiscordSocketClient _client;
        private CommandHandler _handler;

        static void Main(string[] args)
            => new Program().StartAsync().GetAwaiter().GetResult();


        public async Task StartAsync()
        {
            if (string.IsNullOrEmpty(Config.bot.token))
            {
                Utilities.LogConsole(Utilities.LogType.ERROR, "Token not set.");
                Console.ReadLine();
                return;
            }
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Debug,
                AlwaysDownloadUsers = true,
                MessageCacheSize = 300,
            });
            _client.Log += Log;

            await _client.LoginAsync(TokenType.Bot, Config.bot.token);
            await _client.StartAsync();

            _handler = new CommandHandler();
            await _handler.InitializeAsync(_client);
            await Task.Delay(-1);
        }

        private async Task Log(LogMessage msg)
        {
            Utilities.LogConsole(Utilities.LogType.DEBUG, $"{DateTime.Now.ToLocalTime():dd/MM/yy hh:mm:ss} > {msg.Message}");
        }
    }
}
/* TODO Add streaming via youtube url
 *
 */