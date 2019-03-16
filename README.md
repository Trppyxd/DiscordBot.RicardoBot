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

By Default the bot is set to delete all commands calling the Bot, except the bot channel that's set in `config.json`

## Commands
All commands are preceded by a prefix, or the tag of the bot (eg. `@bot#1111`). The prefix can be set in `config.json`.

Commands|Aliases|Parameter(s)
-|-|-
echo |say| **string** Message
choose |pick| **string** 1\|2\|3
purge |delete, del, prune| **int** amount = 10
messageme | NA | **null**
msg |pm, dm| **string** UserTag, **string** message
getmessages|getmsgs, getpms, getdms|**string** UserTag, **int** amount = 10

## Todo
- [x] purge command, fix error if the collection of messages contains one or more message that was created more than **14 days** ago, the command won't work.
- [ ] Add database (`Sqlite`) support.
- [ ] Add Guild text channel logging.
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
