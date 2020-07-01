using System;
using System.Linq;
using System.Threading.Tasks;
using DevSilenceKeeperBot.Extensions;
using DevSilenceKeeperBot.Services;
using Telegram.Bot.Types;

namespace DevSilenceKeeperBot.Commands
{
    public class BanCommand : Command
    {
        private readonly IChatService _chatService;

        public BanCommand(IChatService chatService)
        {
            _chatService = chatService;
        }
        
        public override string[] Triggers => new[] {"/ban"};
        public override async Task Execute(Message message)
        {
            if (message.ReplyToMessage is null)
            {
                await SendTextResponse("Давать бан без цели это признак дурачины ಠ_ಠ...", message);
                return;
            }

            if (IsItAnAttemptToBanBotByYourself(message))
            {
                await SendTextResponse("Я не дурак, что бы банить самого себя ಠ_ಠ...", message);
                return;
            }

            if (IsItAnAttemptToBanChatMemberByYourself(message))
            {
                await SendTextResponse("Банить самого-себя как-то неправильно...", message);
                return;
            }

            if (await IsChatMemberHaveRightsToBan(message) == false)
            {
                await SendTextResponse("Банить могут только модераторы и участники чата с привилегиями!", message);
                return;
            }

            if (await message.ReplyToMessage.From.IsAdmin(message.Chat.Id))
            {
                await SendTextResponse("Ну тут наши полномочия всё. Админа забанить не имею права...", message);
                return;
            }
            
            await DevSilenceKeeper.BotClient.KickChatMemberAsync(
                message.Chat.Id,
                message.ReplyToMessage.From.Id);
            await DevSilenceKeeper.BotClient.SendTextMessageAsync(
                message.Chat.Id,
                $"{message.ReplyToMessage.From} забанен на веки-вечные",
                replyToMessageId: message.MessageId);
        }

        private bool IsItAnAttemptToBanBotByYourself(Message message)
        {
            return message.ReplyToMessage.From.Id == DevSilenceKeeper.BotClient.BotId;
        }

        private bool IsItAnAttemptToBanChatMemberByYourself(Message message)
        {
            return message.ReplyToMessage.From.Id == message.From.Id;
        }

        private async Task<bool> IsChatMemberHaveRightsToBan(Message message)
        {
            var promotedMembers = _chatService.GetPromotedMembers(message.Chat.Id);
            bool isAdmin = await message.From.IsAdmin(message.Chat.Id);
            bool isPromotedChatMember = promotedMembers?.Any(member => member.UserId == message.From.Id) == true;
            return isAdmin || isPromotedChatMember;
        }

        private static async Task SendTextResponse(string text, Message replyToMessage)
        {
            await DevSilenceKeeper.BotClient.SendTextMessageAsync(
                    replyToMessage.Chat.Id,
                    text,
                    replyToMessageId: replyToMessage.MessageId);
        }
    }
}