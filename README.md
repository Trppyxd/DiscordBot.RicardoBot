# UtilityBot
## Description
UtilityBot is a fun side project that I started with the primary focuses on learning more about the Discord.Net API and creating something that would increase the quality of life thus making it easier to administrate a discord server, aswell as to have a tool that is fun to mess around with.
## Setup
**Config.json**<br/>
Upon running the program/solution a `./Resources/config.json` file will be generated.
**Configurable fields**

Variable | Requirement | Description
-|-|-
**token** | Critical | The token of your bot, without it the bot won't run.
**cmdPrefix** | High | Prefix that is preceding a command. If left empty then u can only call commands only via the bot's tag.
**botCommandChannel** | Optional | Name of the channel where you're going to call the bot commands from. By Default the bot is set to delete all messages calling the Bot, except the `botCommandChannel`.

~~By Default the bot is set to delete all commands calling the Bot, except the bot channel that's set in `config.json`~~

## Commands
All commands are preceded by a prefix, or the mention tag of the bot (eg. `@bot#1111`). The prefix can be set in `config.json`.

### Fun
Commands|Parameters|Description
-|-|-
echo[say] |**string** Message|Repeat the provided message.
choose[pick] |**string** 1\|2\|3|Picks one random option from the provided collection.
poll|**string** pollMessage|Creates a poll that you can vote on by clicking the linked emotes.

### Moderation
Commands|Parameters|Description
-|-|-
purge[delete,del,prune] |_**int** amount = 10_|Deletes `amount` of messages from a text channel.
move|**string** VoiceChannel, **string** user| Mention a destination `voice channel` and user/s to be moved to that channel(users must already be in a voice channel for it to work)
kick|**string** userMention|Kicks user/s mentioned.
kicklast|**int** time, **char** timeFormat `s/m/h`, _**string** reason = null_| Kicks users that joined the guild within the provided timeframe.
antiraid|**string** option, _**string** ban = "off"_|Toggle antiraid mode in the current guild which kicks/bans any user that joins while it's active, can optionally provide a reason.
getmessages[getmsgs, getpms, getdms]|**string** UserTag, **int** amount = 10|Gets the messages in a private channel with the specified user.
msg[pm,dm] |**string** UserTag, **string** message|Messages an User in private channel with the provided message.

### Miscellaneous
Commands|Parameters|Description
-|-|-
bm|**string** serverSearch|**UNSTABLE\!** Searches and scrapes the battlemetric's website for a server with the name of `serverSearch` and displays the server info (player count, queue etc).
whois|**string** dUser|Displays information about the provided `dUser`.
who|**string** botName|Displays information about the owner of the bot.
convertid|**string** id|Converts the provided mention into an id and returns it.

### Configuration
Commands|Parameters|Description
-|-|-
config|**string** option, **ulong** value|Changes the bot's config options.

### Database
Commands|Parameters|Description
-|-|-
users|**null**|Returns all the users that are in the database for the current guild.
dbedit|**ulong** discordId, **string** dbProperty, **int** value|Changes the `dbProperty`'s `value` of the user `discordId` in the database.

## Todo
- [x] purge command, fix error if the collection of messages contains one or more message that was created more than **14 days** ago, the command won't work.
- [x] Add database (`Sqlite`) support.
- [x] Add Guild text channel logging.
- [ ] Log user's online time in an explicit `voice channel` and `total time on all channels`.<br/>
      Can check online time periods for `Day, Total, From A to B`.<br/>
      Logs kept upto 7 days.
## Used Documentation
- Discord.Net API
  - [docs.stillu.cc](https://docs.stillu.cc/api/index.html)
  - [discord.foxbot.me](https://discord.foxbot.me/docs/api/index.html)
  - [Discord API Guild](https://discordapp.com/invite/discord-api) `#dotnet_discord-net`
- Newtonsoft.json - JSON parsing/serialization
  - [newtonsoft.com](https://www.newtonsoft.com/json/help/html/Introduction.htm)
