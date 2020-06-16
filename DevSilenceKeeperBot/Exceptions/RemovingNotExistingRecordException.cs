using System;

namespace DevSilenceKeeperBot.Exceptions
{
    public sealed class RemovingNotExistingRecordException : ArgumentException
    {
        public RemovingNotExistingRecordException(string message, string paramName)
            : base(message, paramName) { }
    }
}