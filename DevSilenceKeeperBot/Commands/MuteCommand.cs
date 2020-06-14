using DevSilenceKeeperBot.Extensions;
using DevSilenceKeeperBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DevSilenceKeeperBot.Commands
{
    public sealed class MuteCommand : Command
    {
        private readonly TimeSpan DefaultMuteDuration = TimeSpan.FromDays(1);
        private readonly IChatService _chatService;
        public override string[] Triggers => new string[] { "/mute" };

        public MuteCommand(IChatService chatService)
        {
            _chatService = chatService;
        }

        public override async Task Execute(Message message, TelegramBotClient botClient)
        {
            var promotedMembers = _chatService.GetPromotedMembers(message.Chat.Id);
            bool isAdmin = await message.From.IsAdmin(message.Chat.Id, botClient);
            bool isPromotedChatMember = promotedMembers?.Any(member => member.UserId == message.From.Id) == true;
            if (!(isAdmin || isPromotedChatMember))
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Мутить могут только модераторы и учасники чата с привилегиями!",
                    replyToMessageId: message.MessageId);
            }

            isAdmin = await message.ReplyToMessage.From.IsAdmin(message.Chat.Id, botClient);
            if (isAdmin)
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Ну тут наши полномочия всё. Админа замутить не имеею права...",
                    replyToMessageId: message.MessageId);
            }
            TimeSpan muteDuration = DefaultMuteDuration;
            try
            {
                muteDuration = GetMuteDuration(message.Text);
            }
            catch(ArgumentException ex)
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: ex.Message,
                    replyToMessageId: message.MessageId);
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
                text: $"{message.ReplyToMessage.From} замучен до {muteUntilDate:dd.MM.yyyy hh:mm:ss}",
                replyToMessageId: message.MessageId);

            Task.WaitAll(new Task[] { muteChatMember, reportMute });
            await botClient.DeleteMessageAsync(
                chatId: message.Chat.Id,
                messageId: message.ReplyToMessage.MessageId);
        }
        private TimeSpan GetMuteDuration(string commandArgs)
        {
            var muteDurationString = commandArgs
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Skip(1).FirstOrDefault();

            if (string.IsNullOrWhiteSpace(muteDurationString))
            {
                return DefaultMuteDuration;
            }

            bool isParsed = TimeSpan.TryParse(muteDurationString, out TimeSpan muteDuration);
            if(isParsed)
            {
                if (muteDuration > TimeSpan.Zero)
                {
                    return muteDuration;
                }
                throw new ArgumentException("Вот давай без мута в прошлое.");
            }
            throw new ArgumentException("Дай TimeSpan в формате [d.]hh:mm[:ss[.ff]]! Я же робот, а не человек (╯°□°）╯︵ ┻━┻");
        }
    }
}
