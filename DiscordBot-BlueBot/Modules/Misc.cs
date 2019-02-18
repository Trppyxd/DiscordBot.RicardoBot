using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace DiscordBot.BlueBot.Modules
{
    public class Misc : ModuleBase<SocketCommandContext>
    {
        [Command("echo"), Alias("say")]
        public async Task Echo([Remainder]string message)
        {
            var embed = new EmbedBuilder();
            embed.WithTitle("Echoed message");
            embed.WithDescription(message);
            embed.WithColor(new Color(0, 255, 0));

            await Context.Channel.SendMessageAsync("", false, embed.Build());
            
        }


        [Command("who")]
        public async Task who(string botName)
        {
            if (botName.ToLower() != "ricardobot") return;

            var embed = new EmbedBuilder();
            embed.WithTitle("Info");
            embed.WithDescription("This bot is owned by Ken.\n[Code](https://github.com/Trppyxd/DiscordBot.RicardoBot/blob/master/README.md)");
            embed.WithColor(new Color(0, 255, 0));

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("choose"), Alias("pick"), 
         Summary("Picks one random option from a provided list.")]
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

        [Command("purge"), Alias("delete", "del", "prune"), 
         Summary("Deletes x amount of messages from a text channel.")]
        public async Task PurgeChat(
            [Summary("The amount of messages to delete, DEFAULT: 10, MAX: 100")]
            int amount = 10, 
            [Summary("The channel you wish to delete the messages from, DEFAULT: cmd origin")]
            string channel = "")
        {
            if (amount > 100)
                amount = 100;

            if (channel != "")
            {
                channel = new string((
                    from c in channel
                    where char.IsNumber(c)
                    select c).ToArray());
            }

            var channelInstance = Context.Client.GetChannel(UInt64.Parse(channel));
            

            if (channel == "")
            {
                channelInstance = Context.Channel as SocketTextChannel;
                if (amount != 100) amount += 1;
            }

            var channelA = channelInstance as SocketTextChannel;
            var messages = await channelA.GetMessagesAsync(amount).FlattenAsync();

            var delete = 
                from m in messages
                where m.Timestamp.LocalDateTime > DateTime.Now.ToLocalTime().AddDays(-14)
                select m;
            
            
            await channelA.DeleteMessagesAsync(delete);
        }

        [Command("purge"), Alias("delete", "del", "prune"),
         Summary("Deletes x amount of messages from a text channel.")]
        public async Task PurgeChatOverride(

            [Summary("The channel you wish to delete the messages from, DEFAULT: cmd origin")]
            string channel = "")
        {
             int amount = 10;


            channel = new string((
                from c in channel
                where char.IsNumber(c)
                select c).ToArray());

            Console.WriteLine(channel);
            var channelInstance = Context.Client.GetChannel(UInt64.Parse(channel));


            if (channel == "")
            {
                channelInstance = Context.Channel as SocketTextChannel;
                if (amount != 100) amount += 1;
            }

            var channelFinal = channelInstance as SocketTextChannel;

            var messages = await channelFinal.GetMessagesAsync(amount).FlattenAsync();

            var delete =
                from m in messages
                where m.Timestamp.LocalDateTime > DateTime.Now.ToLocalTime().AddDays(-14)
                select m;


            await channelFinal.DeleteMessagesAsync(delete);
        }

        [Command("messageme"), 
         Summary("The bot sends a message via a private text channel.")]
        public async Task MessageMe([Remainder]string message = "")
        {
            if (!IsUserMember(Context.User as SocketGuildUser)) return;
            var dmChannel = await Context.User.GetOrCreateDMChannelAsync();
            await dmChannel.SendMessageAsync(Utilities.GetAlert("PRIVATE_MESSAGE"));

            await RemoveMessageIfNotInBotChannel();
        }

        [Command("msg"), Alias("pm", "dm", "message"), 
         Summary("Messages an User in private channel with a provided message.")]
        public async Task PrivateMessage(string user = null, [Remainder]string message = "")
        {
            if (user == null) return;
            user = new string((from c in user
                where char.IsNumber(c)
                select c).ToArray());
            
            var dmChannel = await Context.Client.GetUser(UInt64.Parse(user)).GetOrCreateDMChannelAsync();
            await dmChannel.SendMessageAsync(message);

        }

        [Command("getmessages"), Alias("getmsgs", "getpms", "getdms"),
         Summary("Gets the messages in a private channel with the specified user.")]
        public async Task GetMessages(
            [Summary("Whose private messages to show.")]string user, 
            [Summary("The amount of messages to show, DEFAULT: 10, MAX: 100")]int amount = 10)
        {
            if (amount > 100) amount = 100;

            if (user == null) return;
            user = new string((
                from c in user
                where char.IsNumber(c)
                select c).ToArray());

            var dmChannel = await Context.Client.GetUser(UInt64.Parse(user)).GetOrCreateDMChannelAsync();
            var messages = await dmChannel.GetMessagesAsync(amount).FlattenAsync();

            var msgList = new List<string>();
            
            foreach (var msg in messages)
            {
                msgList.Add($"[{msg.Timestamp.ToLocalTime().ToString("dd/MM/yyyy hh:mm:ss")}] {msg.Author.Username} - \"{msg.Content}\"");
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



        private bool IsUserMember(SocketGuildUser user)
        {
            string targetRoleName = "Member"; // TODO Remove hardcoded rolename
            var result = from r in user.Guild.Roles
                where r.Name == targetRoleName
                select r.Id;
            ulong roleID = result.FirstOrDefault();
            if (roleID == 0) return false;
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
    }
}
