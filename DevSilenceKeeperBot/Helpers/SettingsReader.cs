using DevSilenceKeeperBot.Exceptions;
using DevSilenceKeeperBot.Types.Settings;
using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace DevSilenceKeeperBot.Helpers
{
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
                throw new MissingAppSettingsFileException("appSettings.json file is missing in the app folder");
            }
            return JsonConvert.DeserializeObject<AppSettings>(json);
        }
    }
}