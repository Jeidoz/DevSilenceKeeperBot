using System;
using System.Linq;
using System.Threading.Tasks;
using DevSilenceKeeperBot.Extensions;
using DevSilenceKeeperBot.Services;
using Telegram.Bot.Types;

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

        public override async Task Execute(Message message)
        {
            if (message.ReplyToMessage is null)
            {
                await SendTextResponse("Вызывать мут без цели это признак дурачины ಠ_ಠ...", message);
                return;
            }

            if (IsItAnAttemptToMuteBotByYourself(message))
            {
                await SendTextResponse("Я не дурак, что бы мутить самого себя ಠ_ಠ...", message);
                return;
            }

            if (IsItAnAttemptToMuteChatMemberByYourself(message))
            {
                await SendTextResponse("Мутить самого-себя как-то неправильно...", message);
                return;
            }

            if (await IsChatMemberHaveRightsToMute(message) == false)
            {
                await SendTextResponse("Мутить могут только модераторы и участники чата с привилегиями!", message);
                return;
            }

            if (await message.ReplyToMessage.From.IsAdmin(message.Chat.Id))
            {
                await SendTextResponse("Ну тут наши полномочия всё. Админа замутить не имею права...", message);
                return;
            }

            TimeSpan muteDuration;
            try
            {
                muteDuration = GetMuteDuration(message.Text);
            }
            catch (ArgumentException ex)
            {
                await SendTextResponse(ex.Message, message);
                return;
            }

            var muteUntilDate = DateTime.Now + muteDuration;
            await DevSilenceKeeper.BotClient.RestrictChatMemberAsync(
                message.Chat.Id,
                message.ReplyToMessage.From.Id,
                new ChatPermissions {CanSendMessages = false},
                muteUntilDate);
            await DevSilenceKeeper.BotClient.SendTextMessageAsync(
                message.Chat.Id,
                $"{message.ReplyToMessage.From} замучен до {muteUntilDate:dd.MM.yyyy HH:mm:ss} (UTC+02:00)",
                replyToMessageId: message.MessageId);
        }

        private bool IsItAnAttemptToMuteBotByYourself(Message message)
        {
            return message.ReplyToMessage.From.Id == DevSilenceKeeper.BotClient.BotId;
        }

        private bool IsItAnAttemptToMuteChatMemberByYourself(Message message)
        {
            return message.ReplyToMessage.From.Id == message.From.Id;
        }

        private async Task<bool> IsChatMemberHaveRightsToMute(Message message)
        {
            var promotedMembers = await _chatService.GetPromotedMembersAsync(message.Chat.Id);
            bool isAdmin = await message.From.IsAdmin(message.Chat.Id);
            bool isPromotedChatMember = promotedMembers?.Any(member => member.UserId == message.From.Id) == true;
            return isAdmin || isPromotedChatMember;
        }

        private static async Task SendTextResponse(string text, Message replyToMessage)
        {
            await DevSilenceKeeper.BotClient.SendTextMessageAsync(
                    replyToMessage.Chat.Id,
                    text,
                    replyToMessageId: replyToMessage.MessageId)
                ;
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
                    "Дай TimeSpan в формате [d.]HH:mm[:ss[.ff]]! Я же робот, а не человек (╯°□°）╯︵ ┻━┻");
            }

            if (muteDuration > TimeSpan.Zero)
            {
                return muteDuration;
            }

            throw new ArgumentException("Вот давай без мута в прошлое.");
        }
    }
}