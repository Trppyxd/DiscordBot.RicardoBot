using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace DiscordBot.BlueBot.Modules
{
    public class Misc : ModuleBase<SocketCommandContext>
    {
        private DiscordSocketClient _client;

        [Command("echo")]
        public async Task Echo([Remainder]string message)
        {
            var embed = new EmbedBuilder();
            embed.WithTitle("Echoed message");
            embed.WithDescription(message);
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

        [Command("purge"), 
         Summary("Deletes x amount of messages from a text channel.")]
        public async Task PurgeChat([Summary("The amount of messages to delete, DEFAULT is 10.")]int amount = 10, SocketTextChannel channel = null)
        {
            if (amount > 100)
                amount = 100;
            //else if(amount > Context.Channel.)

            if (channel == null)
                channel = Context.Channel as SocketTextChannel;

            var messages = await Context.Channel.GetMessagesAsync(amount).FlattenAsync();


            await channel.DeleteMessagesAsync(messages);
            //if ()

            // TODO FINISH   
        }

        [Command("messageme")]
        public async Task MessageMe([Remainder]string message = "")
        {
            if (!IsUserMember(Context.User as SocketGuildUser)) return;
            var dmChannel = await Context.User.GetOrCreateDMChannelAsync();
            await dmChannel.SendMessageAsync(Utilities.GetAlert("PRIVATE_MESSAGE"));

            await RemoveMessageIfNotInBotChannel();
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
