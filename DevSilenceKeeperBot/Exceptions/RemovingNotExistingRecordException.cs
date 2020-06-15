using System;

namespace DevSilenceKeeperBot.Exceptions
{
    public sealed class RemovingNotExistingRecordException : ArgumentException
    {
        public RemovingNotExistingRecordException()
        {
        }

        public RemovingNotExistingRecordException(string message)
            : base(message)
        {
        }

        public RemovingNotExistingRecordException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public RemovingNotExistingRecordException(string message, string paramName)
            : base(message, paramName)
        {
        }

        public RemovingNotExistingRecordException(string message, string paramName, Exception innerException)
            : base(message, paramName, innerException)
        {
        }
    }
}