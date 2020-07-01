using System.IO;
using System.Text;
using DevSilenceKeeperBot.Exceptions;
using DevSilenceKeeperBot.Types.Settings;
using Newtonsoft.Json;

namespace DevSilenceKeeperBot.Helpers
{
    // ReSharper disable once UnusedType.Global
    public sealed class AppSettingsReader : IAppSettingsReader
    {
        public AppSettings Read()
        {
            string json;
            try
            {
                json = File.ReadAllText("appSettings.json", Encoding.UTF8);
            }
            catch (FileNotFoundException)
            {
                throw new MissingAppSettingsFileException(
                    "appSettings.json file is missing in the app folder");
            }

            return JsonConvert.DeserializeObject<AppSettings>(json);
        }
    }
}