using DevSilenceKeeperBot.Extensions;
using DevSilenceKeeperBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

            var task = _chatService.GetForbiddenWordsAsync(message.Chat.Id);
            task.Wait();
            var result = task.Result;

            if (result == null)
            {
                return false;
            }

            var chatForbiddenWords = result.ToArray();

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

        public override async Task Execute(Message message)
        {
            var tasks = new List<Task>();
            if (await message.From.IsAdmin(message.Chat.Id))
            {
                Task replyToAdminTask = DevSilenceKeeper.BotClient.SendTextMessageAsync(
                    message.Chat.Id,
                    $"Модератор {message.From} нарушает правила чата!",
                    replyToMessageId: message.MessageId);
                tasks.Add(replyToAdminTask);
            }
            else
            {
                var until = DateTime.Now.AddSeconds(31);
                var kickChatMemberTask = DevSilenceKeeper.BotClient.KickChatMemberAsync(
                    message.Chat.Id,
                    message.From.Id,
                    until);

                Task reportAboutKickTask = DevSilenceKeeper.BotClient.SendTextMessageAsync(
                    message.Chat.Id,
                    $"Пользователь {message.From} нарушил правила чата!",
                    replyToMessageId: message.MessageId);

                tasks.AddRange(new List<Task> { kickChatMemberTask, reportAboutKickTask });
            }

            Task.WaitAll(tasks.ToArray());
            await DevSilenceKeeper.BotClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
        }
    }
}