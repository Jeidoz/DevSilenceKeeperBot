using System;
using System.Linq;
using System.Threading.Tasks;
using DevSilenceKeeperBot.Extensions;
using DevSilenceKeeperBot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DevSilenceKeeperBot.Commands
{
    public sealed class MuteCommand : Command
    {
        private readonly IChatService _chatService;
        private readonly TimeSpan _defaultMuteDuration = TimeSpan.FromDays(1);

        public MuteCommand(IChatService chatService)
        {
            _chatService = chatService;
        }

        public override string[] Triggers => new[] {"/mute"};

        public override async Task Execute(Message message, TelegramBotClient botClient)
        {
            if (message.ReplyToMessage.From.Id == botClient.BotId)
            {
                await botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    text: "Я не дурак, что бы мутить самого себя ಠ_ಠ...",
                    replyToMessageId: message.MessageId).ConfigureAwait(false);
                return;
            }

            if (message.ReplyToMessage.From.Id == message.From.Id)
            {
                await botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    text: "Мутить самого-себя как-то неправильно...",
                    replyToMessageId: message.MessageId)
                    .ConfigureAwait(false);
                return;
            }

            var promotedMembers = _chatService.GetPromotedMembers(message.Chat.Id);
            bool isAdmin = await message.From.IsAdmin(message.Chat.Id, botClient).ConfigureAwait(false);
            bool isPromotedChatMember = promotedMembers?.Any(member => member.UserId == message.From.Id) == true;
            if (!(isAdmin || isPromotedChatMember))
            {
                await botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    text: "Мутить могут только модераторы и участники чата с привилегиями!",
                    replyToMessageId: message.MessageId).ConfigureAwait(false);
                return;
            }

            isAdmin = await message.ReplyToMessage.From.IsAdmin(message.Chat.Id, botClient).ConfigureAwait(false);
            if (isAdmin)
            {
                await botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    text: "Ну тут наши полномочия всё. Админа замутить не имею права...",
                    replyToMessageId: message.MessageId).ConfigureAwait(false);
                return;
            }

            TimeSpan muteDuration;
            try
            {
                muteDuration = GetMuteDuration(message.Text);
            }
            catch (ArgumentException ex)
            {
                await botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    ex.Message,
                    replyToMessageId: message.MessageId).ConfigureAwait(false);
                return;
            }

            var muteUntilDate = DateTime.Now + muteDuration;
            var muteChatMember = botClient.RestrictChatMemberAsync(
                message.Chat.Id,
                message.ReplyToMessage.From.Id,
                new ChatPermissions {CanSendMessages = false},
                muteUntilDate);
            Task reportMute = botClient.SendTextMessageAsync(
                message.Chat.Id,
                $"{message.ReplyToMessage.From} замучен до {muteUntilDate:dd.MM.yyyy HH:mm:ss} (UTC+02:00)",
                replyToMessageId: message.MessageId);

            Task.WaitAll(muteChatMember, reportMute);
            await botClient.DeleteMessageAsync(
                message.Chat.Id,
                message.ReplyToMessage.MessageId).ConfigureAwait(false);
        }

        private TimeSpan GetMuteDuration(string commandArgs)
        {
            string muteDurationString = commandArgs
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Skip(1).FirstOrDefault();

            if (string.IsNullOrWhiteSpace(muteDurationString))
            {
                return _defaultMuteDuration;
            }

            bool isParsed = TimeSpan.TryParse(muteDurationString, out var muteDuration);
            if (!isParsed)
            {
                throw new ArgumentException(
                    message: "Дай TimeSpan в формате [d.]HH:mm[:ss[.ff]]! Я же робот, а не человек (╯°□°）╯︵ ┻━┻");
            }

            if (muteDuration > TimeSpan.Zero)
            {
                return muteDuration;
            }

            throw new ArgumentException(message: "Вот давай без мута в прошлое.");
        }
    }
}