using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Core.Mapping;
using System.IO;
using System.Linq;
using System.Net.Cache;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Remoting.Contexts;
using System.Security.Cryptography.X509Certificates;
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

        #region Helpers

        private SocketTextChannel GetUserGuildTextChannelByName(SocketGuildUser user, string ChannelName)
        {
            return user.Guild.Channels.First(x => x.Name.ToLower().Contains(ChannelName)) as SocketTextChannel;
        }

        private SocketVoiceChannel GetUserGuildVoiceChannelByName(SocketGuildUser user, string ChannelName)
        {
            return user.Guild.Channels.First(x => x.Name.ToLower().Contains(ChannelName)) as SocketVoiceChannel;
        }

        #endregion

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
            //_client.MessageReceived += HandleMessageReceived;
            //_client.ReactionAdded += HandleReaction;
            _client.UserJoined += HandleUserJoin;
            _client.UserLeft += HandleUserLeft;
            _client.Ready += AddUsersToDb;
            // TODO Add currentuser updated to change username in Database if an user changes his discord username
        }

        private async Task HandleMessageReceived(SocketMessage arg)
        {
            //var channel = GetUserGuildTextChannelByName(arg.Author as SocketGuildUser, "chat-logs");
            if (!arg.Author.IsBot)
            {
                var originChannel = (SocketTextChannel)_client.GetChannel(arg.Channel.Id);

                var channel = _client.GetGuild(524561374232313857).GetTextChannel(572775387369570306);
                var logResult =
                    $"[MSG]{arg.CreatedAt.UtcDateTime:dd/MM/yy hh:mm:ss}UTC - {arg.Author.ToString()}|{arg.Author.Id} [{originChannel.Guild.Name}|{originChannel.Guild.Id}] ({arg.Channel.Name}|{arg.Channel.Id})\n" +
                    $"--------START-------\n" +
                    $"{arg.Content}\n" +
                    $"---------END--------";
                if (arg.Attachments.Count > 0)
                    logResult += $"\nAttachments: {arg.Attachments.Count}";

                //var guild = ((SocketTextChannel)_client.GetChannel(arg.Channel.Id)).Guild;
                var eb = new EmbedBuilder()
                {
                    Color = Color.Red,
                    Title = $"Chatlog from {originChannel.Guild.Name} | {originChannel.Guild.Id}",

                    Author = new EmbedAuthorBuilder()
                    {
                        IconUrl = arg.Author.GetAvatarUrl(),
                        Name = $"{arg.Author.ToString()} | {arg.Author.Id}"
                    },
                    Description = arg.Content,

                    Footer = new EmbedFooterBuilder()
                    {
                        Text =
                            $"{arg.Channel.Name} | {arg.Channel.Id} - {arg.CreatedAt.UtcDateTime:dd/MM/yy  hh:mm:ss} UTC"
                    }
                };
                Utilities.WriteToLog(Utilities.LogFileType.MESSAGE_PUBLIC, logResult);
                Console.WriteLine(logResult);

                await channel.SendMessageAsync(embed: eb.Build());
                if (arg.Attachments.Count > 0)
                {
                    using (var http = new HttpClient())
                    {
                        foreach (var item in arg.Attachments)
                        {
                            var extension = item.Filename.Split('.')[1];
                            var memStream = await http.GetStreamAsync(item.Url);

                            await channel.SendFileAsync(memStream, item.Filename,
                                $"**Attachment Format: __{extension}__ Size: __{(item.Size / 1000):n0}KB__**");
                        }
                    }
                }
            }
        }

        private async Task LogUserLeft(SocketGuildUser user)
        {
            var channel = GetUserGuildTextChannelByName(user, "logs");
            if (channel == null)
            {
                Utilities.LogConsole(Utilities.LogFormat.ERROR,
                    "LogChannel ID was not valid.");
            }
            else
            {
                await channel.SendMessageAsync(
                    $"User {user.Mention} - {user.ToString()} left the guild at {DateTimeOffset.UtcNow}.");
            }
            Utilities.LogConsole(Utilities.LogFormat.USER_LEFT,
                $"User {user.ToString()} - {user.Id} has left {user.Guild}");
            Utilities.WriteToLog(Utilities.LogFileType.USER_LEAVE,
                $"User {user.Mention} - {user.ToString()} left the guild at {DateTimeOffset.UtcNow}.");
        }

        private async Task DbUserOnLeaveEdit(SocketGuildUser user)
        {
            DBase db = new DBase(user.Guild);

            var dbUserIds = db.GetAllUsers().Select(x => Convert.ToUInt64(x.DiscordId)); // TODO remove database call on every user leave event?
            if (dbUserIds.Contains(user.Id))
            {
                db.EditUser(user.Id, Constants.UserAccount.IsMember, "0");
                db.EditUser(user.Id, Constants.UserAccount.LeaveDate, $"{DateTimeOffset.UtcNow}");
            }
        }

        private async Task HandleUserLeft(SocketGuildUser user)
        {
            await LogUserLeft(user);
            await DbUserOnLeaveEdit(user);
        }

        private async Task AddUsersToDb()
        {
            foreach (var g in _client.Guilds)
            {
                DBase db = new DBase(g);
                var gUsers = g.Users;
                if (gUsers.Count == 0)
                    return;

                db.CreateUserTable();
                var dbUsers = db.GetAllUsers();
                var dbUserIds = dbUsers.Select(x => Convert.ToUInt64(x.DiscordId)).ToList();
                var userIdsNotInDb = gUsers.Select(x => x.Id).Where(x => !dbUserIds.Contains(x)).ToList();

                if (!dbUserIds.Any())
                    userIdsNotInDb = gUsers.Select(x => x.Id).ToList();
                if (userIdsNotInDb.Count == 0)
                    return;

                var newUser = new UserAccount();

                foreach (var userId in userIdsNotInDb)
                {
                    var gUser = g.GetUser(userId);
                    newUser.DiscordId = (long)gUser.Id;
                    newUser.Username = gUser.ToString();
                    if (gUser.JoinedAt != null) newUser.JoinDate = (DateTimeOffset)gUser.JoinedAt;
                    newUser.IsMember = 1;
                    // TODO Add check if user rejoins the server again and already has a previous leave date(override it or make a collection of leavedates).

                    db.AddUser(newUser, g.GetUser(gUser.Id));
                }
            }
        }

        private async Task HandleUserJoin(SocketGuildUser user)
        {
            if (antiRaidToggle && optionalBan)
            {
                await user.BanAsync(1, $"Banned at:{DateTime.UtcNow:dd/MM/yyyy hh/mm/ss} | Banned by: Automated | Reason: Anti-Raid Ban");
                return;
            }
            if (antiRaidToggle)
            {
                await user.KickAsync("Reason: Automated Anti-Raid Kick");
                return;
            }

            var db = new DBase(user.Guild);
            db.CreateUserTable();

            var newUser = new UserAccount();

            newUser.DiscordId = (long)user.Id;
            newUser.Username = user.ToString();
            if (user.JoinedAt != null) newUser.JoinDate = (DateTimeOffset)user.JoinedAt;

            db.AddUser(newUser, user);
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
            //Console.WriteLine($"[DEBUG]optionalBan = {optionalBan}; antiRaidToggle = {antiRaidToggle}");
            // TODO Find a different way than subscribing to the heartbeat event to refresh activity type
            await _client.SetGameAsync(
                $"Running on {_client.Guilds.Count} guilds.");
            if (_client.Guilds.Count == 0) await _client.SetGameAsync("Waiting for heartbeat..."); // prereq. Must be in atleast 1 guild
        }

        //private async Task HandleReaction(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel channel, SocketReaction reaction)
        //{

        //}

        private async Task HandleCommandAsync(SocketMessage s)
        {
            await HandleMessageReceived(s);
            if (!(s is SocketUserMessage msg)) return;
            if (msg.Author.IsBot) return;

            var context = new SocketCommandContext(_client, msg);
            int argPos = 0;
            if (msg.HasStringPrefix(Config.bot.cmdPrefix, ref argPos)
                || msg.HasMentionPrefix(_client.GetUser(535740106187735050), ref argPos)) // Bot discord id is hardcoded
            {
                var result = await _service.ExecuteAsync(context, argPos, null);
                if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                {
                    Utilities.LogConsole(Utilities.LogFormat.ERROR, result.ErrorReason);
                }
            }
        }
    }
}
