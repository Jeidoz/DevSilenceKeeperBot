# DevSilenceKeeperBot
It's a simple chatbot moderator for telegram groups. 
This bot was written for @progeri_chat group in Telegram. Also, here I practiced to write clean code, used DI container, and tried TDD.

## What it can do?
- kick (ban for 31 seconds, due to new Telegram API rules) members if their sent message contains forbidden words/templates.
- send helpful image and URL on "meaningless hello messages", that popular among new chat members.

## Prerequisites
- .NET Core 3.1 Runtime
- Telegram chatbot's token (you can get it in [@BotFather](https://t.me/BotFather))

## How to run
1. Configure your bot settings in **appSettings.json**.
```
{
  "botToken": "123456789:AABBB_ccccddddeeeeffffggggg",
  "helloWords": [
    "hello",
    "hi"
  ]
}
```
2. Make a chatbot an admin in the group (for ablity to kick members).
3. Run the executable file.
```
dotnet run
```

## Available bot commands
- /help – show all commands.
- /words (or /templates) – show forbidden(lead to kick) words/templates.
- /add – (**admin only**) add forbidden word/template.
- /remove (/rm, /delete or /del) – (**admin only**) remove forbidden word/template.

## Built with
- [Telegram.Bot](https://github.com/TelegramBots/telegram.bot) – C# Wrapper of Telegram Bot API. 
- [ScruptureMap](https://github.com/structuremap/structuremap) – DI Container.
- [LiteDb](https://github.com/mbdavid/LiteDB) – simple embedded NoSQL database.
- [XUnit](https://github.com/xunit/xunit) – .NET Testing Framework.
- [NePrivet.ru](https://neprivet.ru/) – image reply to "empty hello messages", that studies people how to correctly use chats.

## License
This project is licensed under the MIT License - see the [LICENSE.md](https://github.com/Jeidoz/DevSilenceKeeperBot/edit/master/LICENSE.md) file for details
