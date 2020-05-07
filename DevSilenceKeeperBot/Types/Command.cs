namespace DevSilenceKeeperBot.Types
{
    public struct Command
    {
        public long ChatId;
        public string Label;
        public string[] Arguments;
        public bool IsInvokerHasAdminRights;

        public Command(long chatId, string label, string[] args = null)
        {
            ChatId = chatId;
            Label = label;
            Arguments = args;
            IsInvokerHasAdminRights = false;
        }
    }
}