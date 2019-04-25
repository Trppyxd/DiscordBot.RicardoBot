using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using System.Text.RegularExpressions;
using System.Threading;
using DiscordBot.BlueBot.Core;
using DiscordBot_BlueBot;
using SQLite;

namespace DiscordBot.BlueBot.Modules
{
    public class CommandsModule : ModuleBase<SocketCommandContext>
    {

        #region Variables

        private DBase db;

        public ulong lastPollId; // unused
        public enum DeleteType
        {
            Self = 1,
            Bot = 2,
            All = 3
        }

        #endregion

        #region Helpers


        #region IsUserRole

        private bool IsUserRole(SocketGuildUser user, string role)
        {
            if (!user.Guild.Roles.Any()) return false;
            string targetRoleName = role.ToLower();
            try
            {
                var result = (user.Guild.Roles).First(x => x.Name.ToLower() == targetRoleName).Id;

                var targetRole = user.Guild.GetRole(result);
                return user.Roles.Contains(targetRole);
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region RemoveMessageIfNotInBotChannel

        public async Task RemoveMessageIfNotInBotChannel()
        {
            if (Context.Channel.Id != Config.bot.botChannelId)
            {
                await Context.Channel.DeleteMessageAsync(Context.Message.Id);
            }
        }

        #endregion

        #region ReturnIdListFromMentionString

        public List<ulong> ReturnIdListFromMentionString(string mentionString)
        {
            //Regex chars = new Regex("[<>@#!]");
            var usersList = Utilities.CleanId(mentionString);
            var userMentions = new List<ulong>();
            foreach (var user in usersList)
            {
                userMentions.Add(Convert.ToUInt64(user));
            }

            return userMentions;
        }

        #endregion

        #region DBUpdateData

        /// <summary>
        /// <see cref="DBase" /> Must be initialized for this to work.
        /// </summary>
        public void dbUpdateData()
        {
            foreach (var user in db.GetAllUsers())
            {
                // If user is not in guild but in database
                if (Context.Guild.GetUser(Convert.ToUInt64(user.DiscordId)) == null)
                {
                    if (user.IsMember == 1)
                    {
                        db.EditUser(Convert.ToUInt64(user.DiscordId), "IsMember", "0");
                    }
                }
                // If user is in the guild
                else if (Context.Guild.GetUser(Convert.ToUInt64(user.DiscordId)) != null)
                {
                    if (user.IsMember == 0)
                    {
                        db.EditUser(Convert.ToUInt64(user.DiscordId), "IsMember", "1");
                    }
                }
            }
        }

        #endregion


        #endregion

        #region Commands


        #region Moderation - Category


        #region Move Command

        [RequireBotPermission(GuildPermission.MoveMembers)]
        [RequireUserPermission(GuildPermission.MoveMembers)]
        [Command("move")]
        public async Task MoveUser(string voiceChannel, [Remainder] string user)
        {
            RemoveMessageIfNotInBotChannel();
            var users = Utilities.CleanId(user);
            var vChannel = Context.Guild.VoiceChannels.First(
                x => x.Name.ToLower().Contains(voiceChannel.ToLower())).Id;

            foreach (var u in users)
            {

                await Context.Guild.GetUser(u).ModifyAsync(x =>
                {
                    x.ChannelId = vChannel;

                });
            }
        }

        #endregion

        #region Purge Command

        [RequireBotPermission(ChannelPermission.SendMessages)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageMessages, ErrorMessage = "User doesn't have permission to manage messages.")]
        [RequireOwner]
        [Command("purge"), Alias("delete", "del", "prune")]
        [Summary("Deletes x amount of messages from a text channel.")]
        public async Task PurgeChat(
            [Summary("The channel you wish to delete the messages from.")]string channelName,
            [Summary("The amount of messages to delete, DEFAULT: 10, MAX: 100")]int amount = 10,
            [Summary("The type of message to delete: Self, Bot or All")]DeleteType deleteType = DeleteType.Self)
        {
            if (string.IsNullOrWhiteSpace(channelName)) return;
            if (amount > 100) amount = 100;

            var channelId = Utilities.CleanId(channelName)[0];
            var channel = Context.Client.GetChannel(channelId);
            if (channel == null) return;


            var channelInst = channel as SocketTextChannel;
            var messages = await channelInst.GetMessagesAsync(amount).FlattenAsync();

            var delete = messages.Where(m => m.Timestamp.LocalDateTime > DateTime.Now.ToLocalTime().AddDays(-14));

            if (deleteType == DeleteType.Self) delete = delete.Where(m => m.Author.Id == Context.Message.Author.Id);
            else if (deleteType == DeleteType.Bot) delete = delete.Where(m => m.Author.IsBot);
            else if (deleteType != DeleteType.All) return;

            await channelInst.DeleteMessagesAsync(delete);

            int delCount = delete.Count();
            await Context.Message.Channel.SendMessageAsync($"<@{Context.User.Id}> > Deleted {delCount} messages of type \"{deleteType}\" in \"{Context.Guild.GetChannel(channelId).Name}\" channel.");
        }

        [RequireBotPermission(ChannelPermission.SendMessages)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageMessages, ErrorMessage = "User doesn't have permission to manage messages.")]
        [Command("purge"), Alias("delete", "del", "prune")]
        [Summary("Deletes x amount of messages from the current text channel.")]
        public async Task PurgeChatOverload(
            [Summary("The amount of messages to delete; default 10; max 100")]int amount = 10,
            [Summary("The type of message to delete: Self, Bot or All")]DeleteType deleteType = DeleteType.Self)
        {
            var channel = Context.Message.Channel as SocketTextChannel;

            if (amount > 100) amount = 100;
            if (amount != 100) amount += 1; // To add the current command to the delete list.

            var messages = await channel.GetMessagesAsync(amount).FlattenAsync();

            var delete = messages.Where(m => m.Timestamp.LocalDateTime > DateTime.Now.ToLocalTime().AddDays(-14));

            if (deleteType == DeleteType.Self) delete = delete.Where(m => m.Author.Id == Context.Message.Author.Id);
            else if (deleteType == DeleteType.Bot) delete = delete.Where(m => m.Author.IsBot);
            else if (deleteType != DeleteType.All) return;

            await channel.DeleteMessagesAsync(delete);

            int delCount = delete.Count();
            await channel.SendMessageAsync($"<@{Context.User.Id}> : Deleted {delCount} messages of {deleteType}.");
        }

        [RequireBotPermission(ChannelPermission.SendMessages)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageMessages, ErrorMessage = "User doesn't have permission to manage messages.")]
        [Command("purgeu"), Alias("deleteu", "delu", "pruneu")]
        [Summary("Deletes x amount of messages from the current text channel.")]
        public async Task PurgeChatOverloadUser(
            [Summary("The amount of messages to delete; default 10; max 100")]int amount,
            [Summary("The type of message to delete: Self, Bot or All")]string user)
        {
            if (string.IsNullOrWhiteSpace(user)) return;

            var channel = Context.Message.Channel as SocketTextChannel;
            ulong userId = Utilities.CleanId(user)[0];

            if (amount > 100) amount = 100;
            if (amount != 100) amount += 1; // To add the current command to the delete list.

            var messages = await channel.GetMessagesAsync(amount).FlattenAsync();

            var delete = messages.Where(m => m.Timestamp.LocalDateTime > DateTime.Now.ToLocalTime().AddDays(-14));
            delete = delete.Where(m => m.Author.Id == userId);


            await channel.DeleteMessagesAsync(delete);

            int delCount = delete.Count();
            await channel.SendMessageAsync($"<@{Context.User.Id}> : Deleted {delCount} messages of user <@{userId}>.");
        }

        #endregion

        #region Kick Command

        [RequireUserPermission(GuildPermission.Administrator, ErrorMessage = "Admin only command.")]
        [Command("kick")]
        public async Task KickUser([Remainder]string userMention)
        {
            if (string.IsNullOrEmpty(userMention)) return;
            var userIds = ReturnIdListFromMentionString(userMention);

            //var userIdsList = userIds.ConvertAll(x => x.ToString());
            foreach (var id in userIds)
            {
                await Context.Guild.GetUser(id).KickAsync();
            }

            try
            {
                var outputChannel = Context.Guild.GetChannel(Config.bot.logChannelId) as SocketTextChannel;
                await outputChannel.SendMessageAsync($"[KICK]User(s): \"{userMention}\" have been kicked from the server.");
            }
            catch (Exception)
            {
                Utilities.LogConsole(Utilities.LogType.ERROR, "Failed to kick user/s.");
            }
        }

        #endregion

        #region Anti-Raid Command

        [RequireUserPermission(GuildPermission.Administrator, ErrorMessage = "Admin only command.")]
        [Command("antiraid")]
        public async Task AntiRaid(string option, string ban = "off")
        {
            if (option.ToLower() == "on")
                CommandHandler.antiRaidToggle = true;
            else if (option.ToLower() == "off")
                CommandHandler.antiRaidToggle = false;

            if (ban.ToLower() == "on")
                CommandHandler.optionalBan = true;
            else if (ban.ToLower() == "off")
                CommandHandler.optionalBan = false;
        }

        #endregion

        #region GetMessages Command

        [RequireUserPermission(GuildPermission.Administrator, ErrorMessage = "Admin only command.")]
        [Command("getmessages"), Alias("getmsgs", "getpms", "getdms")]
        [Summary("Gets the messages in a private channel with the specified user.")]
        public async Task GetMessages(
            [Summary("Whose private messages to show.")]string user,
            [Summary("The amount of messages to show, DEFAULT: 10, MAX: 100")]int amount = 10)
        {
            if (amount > 100) amount = 100;

            if (String.IsNullOrWhiteSpace(user)) return;
            var newUser = Utilities.CleanId(user)[0];

            var dmChannel = await Context.Client.GetUser(newUser).GetOrCreateDMChannelAsync();
            var messages = await dmChannel.GetMessagesAsync(amount).FlattenAsync();

            var msgList = new List<string>();

            foreach (var msg in messages)
            {
                var msgContent = msg.Content;
                if (msg.Content.Contains("||")) msgContent = msg.Content.Replace("||", "<SPOILER>"); // Scuffed fix for spoiler tag sanitization. Waiting for hotfix.
                msgList.Add($"[{msg.Timestamp.ToLocalTime():dd/MM/yyyy hh:mm:ss}] {msg.Author.Username} - \"{msgContent}\"");
            }
            msgList.Reverse();

            if (msgList.Count == 0) return;

            var userData = Context.Client.GetUser(newUser);
            var formatString = String.Join($"{Environment.NewLine}", msgList);

            //embed description doesn't support more than 22 lines
            var embed = new EmbedBuilder();
            embed.WithAuthor(userData.Username);
            embed.WithTitle($"Last {amount} private messages of <@{userData.Id}>");
            embed.WithDescription(formatString);

            await Context.Message.Channel.SendMessageAsync("", false, embed.Build());
        }

        #endregion

        #region Message via Bot Command

        [RequireUserPermission(GuildPermission.Administrator, ErrorMessage = "Admin only command.")]
        [Command("msg"), Alias("pm", "dm", "message")]
        [Summary("Messages an User in private channel with the provided message.")]
        public async Task PrivateMessage(string user = null, [Remainder]string message = "")
        {
            if (user == null) return;
            user = new string((from c in user
                               where char.IsNumber(c)
                               select c).ToArray());

            var dmChannel = await Context.Client.GetUser(UInt64.Parse(user)).GetOrCreateDMChannelAsync();
            await dmChannel.SendMessageAsync(message);
        }

        #endregion

        #region CreateRoleAssigmentMessage

        // TODO Finish
        [RequireUserPermission(GuildPermission.Administrator, ErrorMessage = "Admin only command.")]
        [Command("createroleassignmentmessage"), Alias("cram")]
        public async Task RollAssignmentMessage(string role, string title, [Remainder]string content)
        {
            if (!Context.Guild.Roles.Any(x => x.Id == Convert.ToUInt64(Utilities.CleanId(role)[0])))
            {
                await Context.Channel.SendMessageAsync("No such roles in the guild.");
                return;
            }

            EmbedBuilder builder = new EmbedBuilder()
            {
                Title = title,
                Description = content,
                Footer = new EmbedFooterBuilder
                {
                    Text = "Click ✅ to get the role, ❌ to remove it."
                }
            };
        }

        #endregion


        #endregion


        #region Fun - Category


        #region Echo Command

        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Command("echo")]
        [Alias("say")]
        public async Task Echo([Remainder]string message)
        {
            var embed = new EmbedBuilder();
            embed.WithTitle("Echoed message");
            embed.WithDescription(message);
            embed.WithColor(new Color(0, 255, 0));

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        #endregion

        #region Choose Command

        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Command("choose"), Alias("pick")]
        [Summary("Picks one random option from a provided list.")]
        public async Task ChooseOne([Remainder]string message)
        {
            string[] options = message.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            Random r = new Random();
            string selection = options[r.Next(0, options.Length)];

            var userThumbnailUrl = Context.User.GetAvatarUrl();
            var embed = new EmbedBuilder();
            embed.WithTitle("Message by " + Context.User.Username);
            embed.WithDescription(selection);
            embed.WithColor(new Color(0, 255, 0));
            embed.WithThumbnailUrl(userThumbnailUrl);

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        #endregion

        #region Poll Command

        [Command("poll")]
        public async Task StartPoll([Remainder] string PollMessage)
        {
            EmbedBuilder builder = new EmbedBuilder();
            builder.Title = $"**A poll started by {Context.Message.Author.Username}**";
            builder.Description = $"{PollMessage}";
            builder.Footer = new EmbedFooterBuilder
            {
                Text = "Vote by 'clicking' on the emotes below.",
            };

            IEmote[] reactions = { new Emoji("✅"), new Emoji("❌") };

            var sendMsg = await Context.Channel.SendMessageAsync(embed: builder.Build());

            await Context.Message.DeleteAsync();
            await sendMsg.AddReactionsAsync(reactions);
        }

        #endregion


        #endregion


        #region Miscellaneous - Category


        #region WhoIs Command

        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Command("whois")]
        public async Task WhoIs(string dUser)
        {
            db = new DBase(Context.Guild);

            var user = Context.Guild.GetUser(Convert.ToUInt64(Utilities.CleanId(dUser)[0]));
            var joinedAt = db.GetUserByDiscordId(user.Id).JoinDate;
            await Context.Channel.SendMessageAsync(
                $"WhoIs:\nName:{user.Username} - ID:{user.Id}\nJoined at:{joinedAt:dd/MM/yy hh:mm:ss}\nCreated At:{user.CreatedAt}\nIs Bot:{user.IsBot}");
        }

        #endregion

        #region Who Command

        [RequireBotPermission(ChannelPermission.SendMessages)]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("who")]
        public async Task Who(string botName)
        {
            if (botName.ToLower() != "utilitybot") return;

            var embed = new EmbedBuilder();
            embed.WithTitle("Info");
            embed.WithDescription("This bot is owned by Trppy.\n" +
                                  "[Code](https://github.com/Trppyxd/DiscordBot.RicardoBot/blob/master/README.md)");
            embed.WithColor(new Color(0, 255, 0));

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        #endregion

        #region ConvertID Command

        [Command("convertid")]
        public async Task ConvertId(string Id)
        {
            string oldId = Id;
            var cleanId = Utilities.CleanId(Id)[0];

            await Context.Channel.SendMessageAsync($"User: <@{cleanId}>\nBefore: \\{oldId}\nAfter: {cleanId}");
        }

        #endregion


        #endregion


        #region Configuration - Category


        #region Config Command

        [RequireOwner]
        [Command("config")]
        public async Task ConfigSet(string option, ulong value)
        {
            if (option.Equals("guildId", StringComparison.OrdinalIgnoreCase))
            {
                Config.bot.guildId = value;
                Config.Save();
            }

            if (option.Equals("logChannelId", StringComparison.OrdinalIgnoreCase))
            {
                Config.bot.logChannelId = value;
                Config.Save();
            }

            if (option.Equals("botChannelId", StringComparison.OrdinalIgnoreCase))
            {
                Config.bot.botChannelId = value;
                Config.Save();
            }
        }

        #endregion


        #endregion


        #region Database - Category


        #region Users Command

        [RequireOwner]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Command("users")]
        public async Task dbGetUsers()
        {
            db = new DBase(Context.Guild);

            dbUpdateData();

            string msg = "Users in database: " + Environment.NewLine;
            foreach (var user in db.GetAllUsers())
            {
                msg += $"#{user.Id} [{user.JoinDate:dd/MM/yy hh:mm:ss}] - {user.DiscordId} - {user.Username} [{user.IsMember}]" + Environment.NewLine;
            }
            await Context.Channel.SendMessageAsync(msg);
        }

        #endregion

        #region DbEdit Command

        [RequireOwner]
        [Command("dbedit")]
        public async Task dbEditUser(ulong discordId, string dbProperty, int value)
        {

            using (SQLiteConnection dbCon = new SQLiteConnection(DBase.dbPath))
            {
                SQLiteCommand cmd = new SQLiteCommand(dbCon);
                cmd.CommandText = $@"Update UserAccount Set {dbProperty} = {value} Where DiscordId = {discordId}";

                int result = cmd.ExecuteNonQuery();
                if (result == 1)
                {
                    Utilities.LogConsole(Utilities.LogType.DATABASE, $"Edit Successful > User {discordId}, property {dbProperty}, new value {value}");
                    await Context.Channel.SendMessageAsync(
                        $"Edit Successful > User <@{discordId}>, property {dbProperty}, new value {value}");
                }
                else { Utilities.LogConsole(Utilities.LogType.ERROR, $"Couldn't change property > User {discordId}, property {dbProperty}, new value {value}"); }

            }
        }

        #endregion


        #endregion


        #endregion
    }
}
