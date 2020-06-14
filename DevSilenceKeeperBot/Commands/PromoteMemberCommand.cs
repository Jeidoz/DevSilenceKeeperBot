using DevSilenceKeeperBot.Exceptions;
using DevSilenceKeeperBot.Extensions;
using DevSilenceKeeperBot.Logging;
using DevSilenceKeeperBot.Services;
using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DevSilenceKeeperBot.Commands
{
    public sealed class PromoteMemberCommand : Command
    {
        private readonly IChatService _chatService;
        private readonly ILogger _logger;
        public override string[] Triggers => new string[] { "/promote" };

        public PromoteMemberCommand(IChatService chatService, ILogger logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        public override async Task Execute(Message message, TelegramBotClient botClient)
        {
            if (await message.From.IsAdmin(message.Chat.Id, botClient) == false)
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Надавать привилегии могут только модераторы!",
                    replyToMessageId: message.MessageId);
                return;
            }

            if(string.IsNullOrEmpty(message.ReplyToMessage.Text))
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Для повышения привилегий, нужно делать Reply на сообщение учасника чата.",
                    replyToMessageId: message.MessageId);
                return;
            }

            if(message.ReplyToMessage.From == message.From)
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Повышать самого себя любимого как-то неправильно...",
                    replyToMessageId: message.MessageId);
                return;
            }

            string response;
            string usernameMarkup = $"[@{message.ReplyToMessage.From.Username}](tg://user?id={message.ReplyToMessage.From.Id})";
            try
            {
                _chatService.AddPromotedMember(message.Chat.Id, message.ReplyToMessage.From);
                response = $"{usernameMarkup}, поздравляю из повышением\\!";
            }
            catch(AddingDublicateRecord)
            {
                response = $"{usernameMarkup}, ты уже привилегирован ಠ\\_ಠ";
                _logger.Warning($"[{nameof(AddingDublicateRecord)}]: Chat: {message.Chat}; Invoker: {message.From}; Text: \"{message.Text}\"");
            }
            catch(Exception ex)
            {
                response = $"{usernameMarkup}, извини, я сломался\\. Спамь создателю\\.";
                _logger.Error($"[{nameof(ex)}]: {ex.Message}\n{ex.StackTrace}");
            }

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: response,
                replyToMessageId: message.MessageId,
                parseMode: ParseMode.MarkdownV2);
        }
    }
}