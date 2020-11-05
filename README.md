# DevSilenceKeeperBot
It's a simple chatbot moderator for telegram groups. 
This bot was written for @progeri_chat group in Telegram. Also, here I practiced to write clean code, used DI container, and tried TDD.

## What it can do?
- send "captcha" to new chat members ("click button to get off all restrictions")
- kick (ban for 31 seconds, due to new Telegram API rules) members if their sent message contains forbidden words/templates.
- send helpful image and URL on "meaningless hello messages", that popular among new chat members.
- send google url on "just google it" messages
- promote chat members giving them new privilegies
- mute and unmute chat members by promoted members or higher
- delete messages by promoted members or higher
- ban and unban chat members by promoted members or higher

### Available bot commands
- /help – show all commands.
- /words (or /templates) – show forbidden(lead to kick) words/templates.
- /add – (**admin only**) add forbidden word/template.
- /remove (/rm) – (**admin only**) remove forbidden word/template.
- /promote – (**promoted member or higher**) gives additional privilegies to chat member
- /unpromote — (**promoted member or higher**) removed additional privilegies from chat member
- /delete (/del) – (**promoted member or higher**) delete replied message
- /users – show chat promoted members
- /mute \[d.\]HH:mm\[:ss\] – (**promoted member or higher**) mute chat member
- /unmute – (**promoted member or higher**) unmute chat member
- /ban — (**promoted member or higher**) bans replied chat member
- /unban — (**promoted member of higher**) unbans replied chat member

### Try it!
You can just add the existing chat-bot instance [@devsilencekeeper_bot](https://t.me/devsilencekeeper_bot) to your group and give admin rights to it.

**OR**

## Host your own chat-bot instance

### Prerequisites
- .NET Core 3.1 Runtime
- Telegram chatbot's token (you can get it in [@BotFather](https://t.me/BotFather))
- Existing MySQL database connection string

### How to run your own bot
1. Configure your bot settings in **appSettings.json**.
```
{
  ...
  "ConnectionStrings": {
    "MySql": "server=localhost;port=3306;database=test_devsilencekeeper;user=test_devsilencekeeper;password=P@$$W0RD"
  },
  "BotToken": "123456789:AABBB_ccccddddeeeeffffggggg",
  "HelloWords": [
    "hello",
    "hi"
  ],
  "GoogleWords": [
	"what is",
	"how i can do"
  ]
}
```
2. Run the executable file.
```
dotnet run
```
3. Add chatbot as admin to the your group!



## Built with
- [Telegram.Bot](https://github.com/TelegramBots/telegram.bot) – C# Wrapper of Telegram Bot API. 
- [Entity Framework Core](https://github.com/dotnet/efcore) — EF Core is a modern object-database mapper for .NET. It supports LINQ queries, change tracking, updates, and schema migrations. 
- [Pomelo.EntityFrameworkCore.MySQL](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql) — Entity Framework Core provider for MySql built on top of mysql-net/MySqlConnector 
- [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json) — Json.NET is a popular high-performance JSON framework for .NET 
- [Serilog](https://github.com/serilog/serilog) — Simple .NET logging with fully-structured events 
- [XUnit](https://github.com/xunit/xunit) – .NET Testing Framework.
- [NePrivet.ru](https://neprivet.ru/) – image reply to "empty hello messages", that studies people how to correctly use chats.

## License
This project is licensed under the MIT License - see the [LICENSE.md](https://github.com/Jeidoz/DevSilenceKeeperBot/edit/master/LICENSE.md) file for details
