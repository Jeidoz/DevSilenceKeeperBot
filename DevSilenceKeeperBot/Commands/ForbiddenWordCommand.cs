using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevSilenceKeeperBot.Extensions;
using DevSilenceKeeperBot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DevSilenceKeeperBot.Commands
{
    public sealed class ForbiddenWordCommand : Command
    {
        private readonly IChatService _chatService;

        public ForbiddenWordCommand(IChatService chatService)
        {
            _chatService = chatService;
            Triggers = null;
        }

        public override string[] Triggers { get; }

        public override bool Contains(Message message)
        {
            if (string.IsNullOrEmpty(message.Text))
            {
                return false;
            }

            var temp = _chatService
                .GetChatForbiddenWords(message.Chat.Id);
            if (temp == null)
            {
                return false;
            }

            var chatForbiddenWords = temp.ToArray();

            if (!chatForbiddenWords.Any())
            {
                return false;
            }

            if (message.Entities == null)
            {
                return chatForbiddenWords.Any(word => message.Text.Contains(word));
            }

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
                    message.Chat.Id,
                    $"Модератор {message.From} нарушает правила чата!",
                    replyToMessageId: message.MessageId);
                tasks.Add(replyToAdminTask);
            }
            else
            {
                var until = DateTime.Now.AddSeconds(31);
                var kickChatMemberTask = botClient.KickChatMemberAsync(
                    message.Chat.Id,
                    message.From.Id,
                    until);

                Task reportAboutKickTask = botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    $"Пользователь {message.From} нарушил правила чата!",
                    replyToMessageId: message.MessageId);

                tasks.AddRange(new List<Task> {kickChatMemberTask, reportAboutKickTask});
            }

            Task.WaitAll(tasks.ToArray());
            await botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId).ConfigureAwait(false);
        }
    }
}