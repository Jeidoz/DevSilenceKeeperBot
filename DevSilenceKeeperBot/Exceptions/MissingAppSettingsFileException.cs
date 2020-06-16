using System.IO;

namespace DevSilenceKeeperBot.Exceptions
{
    public sealed class MissingAppSettingsFileException : FileNotFoundException
    {
        public MissingAppSettingsFileException(string message)
            : base(message) { }
    }
}