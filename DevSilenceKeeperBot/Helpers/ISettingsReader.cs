using DevSilenceKeeperBot.Types.Settings;

namespace DevSilenceKeeperBot.Helpers
{
    public interface IAppSettingsReader
    {
        /// <summary>
        ///     Reads appSettings.json and returns it's content
        /// </summary>
        /// <returns>Returns deserialized appSettings</returns>
        AppSettings Read();
    }
}