using System;

namespace DevSilenceKeeperBot.Exceptions
{
    public sealed class AddingDuplicateRecord : ArgumentException
    {
        public AddingDuplicateRecord(string message, string paramName)
            : base(message, paramName) { }
    }
}