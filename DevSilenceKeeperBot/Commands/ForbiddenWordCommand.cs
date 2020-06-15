using DevSilenceKeeperBot.Extensions;
using DevSilenceKeeperBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DevSilenceKeeperBot.Commands
{
    public sealed class ForbiddenWordCommand : Command
    {
        private readonly IChatService _chatService;
        public override string[] Triggers { get; }

        public ForbiddenWordCommand(IChatService chatService)
        {
            _chatService = chatService;
        }

        public override bool Contains(Message message)
        {
            if (string.IsNullOrEmpty(message.Text))
            {
                return false;
            }

            var chatForbiddenWords = _chatService.GetChatForbiddenWords(message.Chat.Id);

            if (chatForbiddenWords == null || !chatForbiddenWords.Any())
            {
                return false;
            }

            if (message.Entities != null)
            {
                if (message.Entities
                    .Any(entity => chatForbiddenWords
                        .Any(word => entity.Url?.Contains(word) == true)))
                {
                    return true;
                }
            }

            return chatForbiddenWords.Any(word => message.Text.Contains(word));
        }

        public override async Task Execute(Message message, TelegramBotClient botClient)
        {
            var tasks = new List<Task>();
            if (await message.From.IsAdmin(message.Chat.Id, botClient).ConfigureAwait(false))
            {
                Task replyToAdminTask = botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: $"Модератор {message.From} нарушает правила чата!",
                    replyToMessageId: message.MessageId);
                tasks.Add(replyToAdminTask);
            }
            else
            {
                DateTime until = DateTime.Now.AddSeconds(31);
                Task kickChatMemberTask = botClient.KickChatMemberAsync(
                    chatId: message.Chat.Id,
                    userId: message.From.Id,
                    untilDate: until);

                Task reportAboutKickTask = botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: $"Пользователь {message.From} нарушил правила чата!",
                    replyToMessageId: message.MessageId);

                tasks.AddRange(new List<Task> { kickChatMemberTask, reportAboutKickTask });
            }

            Task.WaitAll(tasks.ToArray());
            await botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId).ConfigureAwait(false);
        }
    }
}