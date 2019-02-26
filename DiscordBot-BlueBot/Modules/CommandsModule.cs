using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using System.Text.RegularExpressions;
using DiscordBot.BlueBot.Core;
using DiscordBot_BlueBot;

namespace DiscordBot.BlueBot.Modules
{
    public class CommandsModule : ModuleBase<SocketCommandContext>
    {
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

        [Command("testdb")]
        public async Task TestDb()
        {
            var newUser = new UserAccount();
            newUser.DiscordId = Int64.Parse(Context.User.Id.ToString());
            newUser.Username = Context.User.Username;
            newUser.JoinDate = Context.Message.Timestamp.DateTime.ToLocalTime();

            var db = new DBase();
            db.CreateUserTable();

            string msg = "Users in database: " + Environment.NewLine;
            db.AddUser(newUser);
            foreach (var user in db.GetAllUsers())
            {
                msg += $"#{user.Id} - {user.JoinDate} - {user.DiscordId} - {user.Username}" + Environment.NewLine;
            }
            await Context.Channel.SendMessageAsync(msg);
        }

        [Command("users")]
        public async Task UsersInDatabase()
        {
            var db = new DBase();

            string msg = "Users in database: " + Environment.NewLine;
            foreach (var user in db.GetAllUsers())
            {
                msg += $"#{user.Id} - [{user.JoinDate}] - {user.DiscordId} - {user.Username}" + Environment.NewLine;
            }
            await Context.Channel.SendMessageAsync(msg);
        }

        [Command("who")]
        public async Task Who(string botName)
        {
            if (botName.ToLower() != "ricardobot") return;

            var embed = new EmbedBuilder();
            embed.WithTitle("Info");
            embed.WithDescription("This bot is owned by Ken.\n" +
                                  "[Code](https://github.com/Trppyxd/DiscordBot.RicardoBot/blob/master/README.md)");
            embed.WithColor(new Color(0, 255, 0));

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

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


        [RequireUserPermission(ChannelPermission.ManageMessages, ErrorMessage = "User doesn't have permission to manage messages.")]
        [Command("purge"), Alias("delete", "del", "prune")]
        [Summary("Deletes x amount of messages from a text channel.")]
        public async Task PurgeChat(
            [Summary("The channel you wish to delete the messages from.")]string channelName,
            [Summary("The amount of messages to delete, DEFAULT: 10, MAX: 100")]int amount = 10,
            [Summary("The type of message to delete: Self, Bot or All")]DeleteType deleteType = DeleteType.Self)
        {
            if (amount > 100) amount = 100;

            if (!string.IsNullOrEmpty(channelName)) channelName = Utilities.TrimId(channelName);
            var channel = Context.Client.GetChannel(UInt64.Parse(channelName));

            var channelInst = channel as SocketTextChannel;
            var messages = await channelInst.GetMessagesAsync(amount).FlattenAsync();

            var delete = messages.Where(m => m.Timestamp.LocalDateTime > DateTime.Now.ToLocalTime().AddDays(-14));

            if (deleteType == DeleteType.Self) delete = delete.Where(m => m.Author.Id == Context.Message.Author.Id);
            else if (deleteType == DeleteType.Bot) delete = delete.Where(m => m.Author.IsBot);
            else if (deleteType != DeleteType.All) return;

            await channelInst.DeleteMessagesAsync(delete);
        }


        [Command("purge"), Alias("delete", "del", "prune")]
        [Summary("Deletes x amount of messages from the current text channel.")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        public async Task PurgeChatOverride(
            [Summary("The amount of messages to delete; default 10; max 100")]int amount = 10,
            [Summary("The type of message to delete: Self, Bot or All")]DeleteType deleteType = DeleteType.Self)
        {
            var channel = Context.Message.Channel as SocketTextChannel;

            if (amount > 100) amount = 100;
            if (amount != 100) amount += 1; // To add the current command to the delete list.

            var messages = await channel.GetMessagesAsync(amount).FlattenAsync();

            var delete = messages.Where(m => m.Timestamp.LocalDateTime > DateTime.Now.ToLocalTime().AddDays(-14));

            if (deleteType == DeleteType.Self) delete = delete.Where(m => m.Author.Id == Context.Message.Author.Id);
            else if (deleteType == DeleteType.Bot)delete = delete.Where(m => m.Author.IsBot);
            else if (deleteType != DeleteType.All) return;

            int delCount = delete.Count();

            await channel.DeleteMessagesAsync(delete);

            await channel.SendMessageAsync($"<@{Context.User.Id}> Deleted {delCount} messages of {deleteType}.");
        }

        public enum DeleteType
        {
            Self = 1,
            Bot = 2,
            All = 3
        }

        [Command("messageme")]
        [Summary("The bot sends a message via a private text channel.")]
        public async Task MessageMe([Remainder]string message = "")
        {
            if (!IsUserRole(Context.User as SocketGuildUser, "member")) return;
            var dmChannel = await Context.User.GetOrCreateDMChannelAsync();
            await dmChannel.SendMessageAsync(Utilities.GetAlert("PRIVATE_MESSAGE"));

            await RemoveMessageIfNotInBotChannel();
        }

        [RequireUserPermission(GuildPermission.Administrator, ErrorMessage = "Admin only command.")]
        [Command("msg"), Alias("pm", "dm", "message")]
        [Summary("Messages an User in private channel with a provided message.")]
        public async Task PrivateMessage(string user = null, [Remainder]string message = "")
        {
            if (user == null) return;
            user = new string((from c in user
                where char.IsNumber(c)
                select c).ToArray());
            
            var dmChannel = await Context.Client.GetUser(UInt64.Parse(user)).GetOrCreateDMChannelAsync();
            await dmChannel.SendMessageAsync(message);

        }

        [Command("getmessages"), Alias("getmsgs", "getpms", "getdms")]
        [Summary("Gets the messages in a private channel with the specified user.")]
        public async Task GetMessages(
            [Summary("Whose private messages to show.")]string user, 
            [Summary("The amount of messages to show, DEFAULT: 10, MAX: 100")]int amount = 10)
        {
            if (amount > 100) amount = 100;

            if (user == null) return;
            user = Utilities.TrimId(user);

            var dmChannel = await Context.Client.GetUser(UInt64.Parse(user)).GetOrCreateDMChannelAsync();
            var messages = await dmChannel.GetMessagesAsync(amount).FlattenAsync();

            var msgList = new List<string>();
            
            foreach (var msg in messages)
            {
                var msgContent = msg.Content;
                if (msg.Content.Contains("||")) msgContent = msg.Content.Replace("||", "<SPOILER>");;
                msgList.Add($"[{msg.Timestamp.ToLocalTime():dd/MM/yyyy hh:mm:ss}] {msg.Author.Username} - \"{msgContent}\"");
            }
            msgList.Reverse();

            if (msgList.Count == 0) return;

            var userData = Context.Client.GetUser(UInt64.Parse(user));
            var formatString = String.Join($"{Environment.NewLine}", msgList);

            //embed description doesn't support more than 22 lines
            var embed = new EmbedBuilder();
            embed.WithAuthor(userData.Username);
            embed.WithTitle($"Last {amount} private messages of <@{userData.Id}>");
            embed.WithDescription(formatString);

            await Context.Message.Channel.SendMessageAsync("", false, embed.Build());
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("kick")]
        public async Task UserMute([Remainder]string userMention)
        {
            if(string.IsNullOrEmpty(userMention)) return;
            //if (!userMention.Contains('<')) return;

            Regex chars = new Regex("[<>@#]");
            var usersList = userMention.Replace(' ', ',');
            var userMentions = chars.Replace(userMention, "").Split(' ');
            
            var userIds = userMentions.Select(UInt64.Parse).ToList();

            //var userIdsList = userIds.ConvertAll(x => x.ToString());
            foreach (var id in userIds)
            {
                await Context.Guild.GetUser(id).KickAsync();
            }

            var outputChannel = Context.Guild.Channels.Single(x => x.Name.Contains(Config.bot.botCommandChannel)) as SocketTextChannel;
            // TODO Add try catch for exception if Context.Guild.Channels.Single throws an exception.
            await outputChannel.SendMessageAsync($"Users: \"{usersList}\" have been kicked from the server.");
        }

        private bool IsUserRole(SocketGuildUser user, string role)
        {
            if (!user.Guild.Roles.Any()) return false;
            string targetRoleName = role.ToLower();
            var result = (user.Guild.Roles).Where(x => x.Name.ToLower() == targetRoleName).Select(x => x.Id);
            //var result = from r in user.Guild.Roles
            //    where r.Name.ToLower() == targetRoleName
            //    select r.Id;
            if (!result.Any()) return false;
            ulong roleID = result.FirstOrDefault();
            if (roleID == 0) return false; // ?
            var targetRole = user.Guild.GetRole(roleID);
            return user.Roles.Contains(targetRole);
        }

        public async Task RemoveMessageIfNotInBotChannel()
        {
            if (Context.Channel.Name.Contains(Config.bot.botCommandChannel) == false)
            {
                await Context.Channel.DeleteMessageAsync(Context.Message.Id);
            }
        }

        [Command("data")]
        public async Task GetData()
        {
            await Context.Channel.SendMessageAsync("Data has " + DataStorage.GetPairsCount() + " pairs.");
        }

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

            IEmote[] reactions = {new Emoji("✅"), new Emoji("❌") };

            var sendMsg = await Context.Channel.SendMessageAsync(embed: builder.Build());
            await Context.Message.DeleteAsync();
            await sendMsg.AddReactionsAsync(reactions);


        }
    }
}
