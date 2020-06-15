using System;
using System.IO;

namespace DevSilenceKeeperBot.Exceptions
{
    public sealed class MissingAppSettingsFileException : FileNotFoundException
    {
        public MissingAppSettingsFileException()
        {
        }

        public MissingAppSettingsFileException(string message)
            : base(message)
        {
        }

        public MissingAppSettingsFileException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public MissingAppSettingsFileException(string message, string fileName)
            : base(message, fileName)
        {
        }

        public MissingAppSettingsFileException(string message, string fileName, Exception innerException)
            : base(message, fileName, innerException)
        {
        }
    }
}