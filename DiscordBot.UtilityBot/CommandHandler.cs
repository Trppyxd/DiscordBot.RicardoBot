using System;
using System.CodeDom;
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
using DiscordBot.BlueBot.Modules;
using DiscordBot_BlueBot;
using Newtonsoft.Json;

namespace DiscordBot.BlueBot
{
    public class CommandHandler
    {
        private DiscordSocketClient _client;
        private CommandService _service;
        public static bool antiRaidToggle;
        public static bool optionalBan;

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
            _client.UserJoined += HandleUserJoin;
            _client.Ready += AddUsersToDb;
        }

        private async Task AddUsersToDb()
        {
            if (string.IsNullOrEmpty(Config.bot.guildId))
            {
            Console.WriteLine($"[ERROR] Config.bot.guildId is null or empty.");
            return;
            }

            var guild = _client.GetGuild(Convert.ToUInt64(Config.bot.guildId));
            var gUsers = guild.Users;

            var db = new DBase();
            db.CreateUserTable();
            var dbUserIds = db.GetAllUsers().Select(x => Convert.ToUInt64(x.DiscordId));
            var userIdsNotInDb = gUsers.Select(x => x.Id).Where(x => !dbUserIds.Contains(x));
            //if (userIdsNotInDb.Any()) return;

            var newUser = new UserAccount();

            SocketGuildUser gUser = null;
            foreach (var userId in userIdsNotInDb)
            {
                gUser = guild.GetUser(userId);
                newUser.DiscordId = (long)gUser.Id;
                newUser.Username = gUser.Username;
                if (gUser.JoinedAt != null) newUser.JoinDate = (DateTimeOffset)gUser.JoinedAt;

                db.AddUser(newUser);
            }
        }

        private async Task HandleUserJoin(SocketGuildUser user)
        {
            if (antiRaidToggle && optionalBan)
            {
                await user.BanAsync();
                return;
            }
            if(antiRaidToggle)
            {
                await user.KickAsync();
                return;
            }


            var db = new DBase();
            db.CreateUserTable();

            var newUser = new UserAccount();

            newUser.DiscordId = (long)user.Id;
            newUser.Username = user.Username;
            if (user.JoinedAt != null) newUser.JoinDate = (DateTimeOffset)user.JoinedAt;

            db.AddUser(newUser);
        }
    
        //private async Task _client_GuildUnavailable(SocketGuild arg)
        //{
        //}

        //private async Task _client_GuildAvailable(SocketGuild arg)
        //{
        //}

        //private async Task ClientOnReady()
        //{
        //}

        private async Task HandleHeartbeat(int arg1, int arg2)
        {
            Console.WriteLine($"optionalBan = {optionalBan}; antiRaidToggle = {antiRaidToggle}"); // TODO remove Debug line
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
