using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Mapping;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.BlueBot.Core;
using DiscordBot_BlueBot;
using Newtonsoft.Json;

namespace DiscordBot.BlueBot
{
    class CommandHandler
    {
        private DiscordSocketClient _client;
        private CommandService _service;
        //private int guildCount = 0;

        public async Task InitializeAsync(DiscordSocketClient client)
        {
            _client = client;
            _service = new CommandService(new CommandServiceConfig
            {
                CaseSensitiveCommands = false
            });
            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), null);

            //_client.GuildUnavailable += _client_GuildUnavailable;
            //_client.GuildAvailable += _client_GuildAvailable;
            _client.LatencyUpdated += HandleHeartbeat;
            _client.MessageReceived += HandleCommandAsync;
            _client.ReactionAdded += HandleReaction;
        }

        //private async Task _client_GuildUnavailable(SocketGuild arg)
        //{
        //    --guildCount;
        //    await _client.SetGameAsync($"Running on {guildCount} guilds.");
        //}

        //private async Task _client_GuildAvailable(SocketGuild arg)
        //{
        //    ++guildCount;
        //    await _client.SetGameAsync($"Running on {guildCount} guilds.");
        //}

        //private async Task ClientOnReady()
        //{
        //    await _client.SetGameAsync($"Running on {_client.Guilds.Count} guilds.");
        //}

        private async Task HandleHeartbeat(int arg1, int arg2)
        {
            await _client.SetGameAsync($"Running on {_client.Guilds.Count} guilds.");
            if (_client.Guilds.Count == 0) await _client.SetGameAsync("Waiting for heartbeat..."); // prereq. Must be in atleast 1 guild
        }

        private async Task HandleReaction(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel channel, SocketReaction reaction)
        {
            
        }

        private async Task HandleCommandAsync(SocketMessage s)
        {
            var msg = s as SocketUserMessage;
            if (msg == null) return;
            var context = new SocketCommandContext(_client, msg);
            int argPos = 0;
            if (msg.HasStringPrefix(Config.bot.cmdPrefix, ref argPos)
                || msg.HasMentionPrefix(_client.GetUser(535740106187735050), ref argPos))
            {
                var result = await _service.ExecuteAsync(context, argPos, null);
                if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                {
                    Console.WriteLine(result.ErrorReason);
                }
            }          
        }
    }
}
