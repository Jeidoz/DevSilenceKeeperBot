using System;
using System.IO;

namespace DevSilenceKeeperBot.Exceptions
{
    public sealed class MissingAppSettingsFileException : FileNotFoundException
    {
        public MissingAppSettingsFileException(string message)
            : base(message)
        {
        }

        public MissingAppSettingsFileException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
