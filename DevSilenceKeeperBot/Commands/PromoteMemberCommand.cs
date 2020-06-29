﻿using System;
using System.Linq;
using System.Threading.Tasks;
using DevSilenceKeeperBot.Exceptions;
using DevSilenceKeeperBot.Extensions;
using DevSilenceKeeperBot.Logging;
using DevSilenceKeeperBot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DevSilenceKeeperBot.Commands
{
    public sealed class PromoteMemberCommand : Command
    {
        private readonly IChatService _chatService;
        private readonly ILogger _logger;

        public PromoteMemberCommand(IChatService chatService, ILogger logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        public override string[] Triggers => new[] {"/promote"};

        public override async Task Execute(Message message, TelegramBotClient botClient)
        {
            var promotedMembers = _chatService.GetPromotedMembers(message.Chat.Id);
            bool isAdmin = await message.From.IsAdmin(message.Chat.Id, botClient).ConfigureAwait(false);
            bool isPromotedChatMember = promotedMembers?.Any(member => member.UserId == message.From.Id) == true;
            if (!(isAdmin || isPromotedChatMember))
            {
                await botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    text: "Надавать привилегии могут только модераторы!",
                    replyToMessageId: message.MessageId).ConfigureAwait(false);
                return;
            }

            if (string.IsNullOrEmpty(message.ReplyToMessage.Text))
            {
                await botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    text: "Для повышения привилегий, нужно делать Reply на сообщение участника чата.",
                    replyToMessageId: message.MessageId).ConfigureAwait(false);
                return;
            }

            if (message.ReplyToMessage.From == message.From)
            {
                await botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    text: "Повышать самого себя любимого как-то неправильно...",
                    replyToMessageId: message.MessageId).ConfigureAwait(false);
                return;
            }

            string response;
            string usernameMarkup =
                $"[@{message.ReplyToMessage.From.Username}](tg://user?id={message.ReplyToMessage.From.Id})";
            try
            {
                _chatService.AddPromotedMember(message.Chat.Id, message.ReplyToMessage.From);
                response = $"{usernameMarkup}, поздравляю из повышением\\!";
            }
            catch (AddingDuplicateRecord)
            {
                response = $"{usernameMarkup}, ты уже привилегирован ಠ\\_ಠ";
                _logger.Warning(
                    $"[{nameof(AddingDuplicateRecord)}]: Chat: {message.Chat}; Invoker: {message.From}; Text: \"{message.Text}\"");
            }
            catch (Exception ex)
            {
                response = $"{usernameMarkup}, извини, я сломался\\. Пиши создателю\\.";
                _logger.Error($"[{nameof(ex)}]: {ex.Message}\n{ex.StackTrace}");
            }

            await botClient.SendTextMessageAsync(
                message.Chat.Id,
                response,
                replyToMessageId: message.MessageId,
                parseMode: ParseMode.Markdown).ConfigureAwait(false);
        }
    }
}