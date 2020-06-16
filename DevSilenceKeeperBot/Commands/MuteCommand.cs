using DevSilenceKeeperBot.Extensions;
using DevSilenceKeeperBot.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DevSilenceKeeperBot.Commands
{
    public sealed class MuteCommand : Command
    {
        private readonly TimeSpan _defaultMuteDuration = TimeSpan.FromDays(1);
        private readonly IChatService _chatService;
        public override string[] Triggers => new string[] { "/mute" };

        public MuteCommand(IChatService chatService)
        {
            _chatService = chatService;
        }

        public override async Task Execute(Message message, TelegramBotClient botClient)
        {
            if (message.ReplyToMessage.From.Id == botClient.BotId)
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Я не дурак, что бы мутить самого себя ಠ_ಠ...",
                    replyToMessageId: message.MessageId).ConfigureAwait(false);
                return;
            }

            var promotedMembers = _chatService.GetPromotedMembers(message.Chat.Id);
            bool isAdmin = await message.From.IsAdmin(message.Chat.Id, botClient).ConfigureAwait(false);
            bool isPromotedChatMember = promotedMembers?.Any(member => member.UserId == message.From.Id) == true;
            if (!(isAdmin || isPromotedChatMember))
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Мутить могут только модераторы и участники чата с привилегиями!",
                    replyToMessageId: message.MessageId).ConfigureAwait(false);
                return;
            }

            isAdmin = await message.ReplyToMessage.From.IsAdmin(message.Chat.Id, botClient).ConfigureAwait(false);
            if (isAdmin)
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Ну тут наши полномочия всё. Админа замутить не имею права...",
                    replyToMessageId: message.MessageId).ConfigureAwait(false);
                return;
            }
            TimeSpan muteDuration = _defaultMuteDuration;
            try
            {
                muteDuration = GetMuteDuration(message.Text);
            }
            catch (ArgumentException ex)
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: ex.Message,
                    replyToMessageId: message.MessageId).ConfigureAwait(false);
                return;
            }
            DateTime muteUntilDate = DateTime.Now + muteDuration;
            Task muteChatMember = botClient.RestrictChatMemberAsync(
                chatId: message.Chat.Id,
                userId: message.ReplyToMessage.From.Id,
                permissions: new ChatPermissions { CanSendMessages = false },
                untilDate: muteUntilDate);
            Task reportMute = botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: $"{message.ReplyToMessage.From} замучен до {muteUntilDate:dd.MM.yyyy HH:mm:ss} (UTC+02:00)",
                replyToMessageId: message.MessageId);

            Task.WaitAll(new Task[] { muteChatMember, reportMute });
            await botClient.DeleteMessageAsync(
                chatId: message.Chat.Id,
                messageId: message.ReplyToMessage.MessageId).ConfigureAwait(false);
        }

        private TimeSpan GetMuteDuration(string commandArgs)
        {
            var muteDurationString = commandArgs
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Skip(1).FirstOrDefault();

            if (string.IsNullOrWhiteSpace(muteDurationString))
            {
                return _defaultMuteDuration;
            }

            bool isParsed = TimeSpan.TryParse(muteDurationString, out TimeSpan muteDuration);
            if (isParsed)
            {
                if (muteDuration > TimeSpan.Zero)
                {
                    return muteDuration;
                }
                throw new ArgumentException("Вот давай без мута в прошлое.");
            }
            throw new ArgumentException("Дай TimeSpan в формате [d.]HH:mm[:ss[.ff]]! Я же робот, а не человек (╯°□°）╯︵ ┻━┻");
        }
    }
}