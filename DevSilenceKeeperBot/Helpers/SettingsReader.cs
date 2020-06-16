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
                json = File.ReadAllText(path: "appSettings.json", Encoding.UTF8);
            }
            catch (FileNotFoundException)
            {
                throw new MissingAppSettingsFileException(
                    message: "appSettings.json file is missing in the app folder");
            }

            return JsonConvert.DeserializeObject<AppSettings>(json);
        }
    }
}