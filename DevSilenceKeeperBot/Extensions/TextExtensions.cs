using System.Text.RegularExpressions;

namespace DevSilenceKeeperBot.Extensions
{
    public static class TextExtensions
    {
        public static string RemoveSpecialCharacters(this string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            var pattern = new Regex("[-:!@#$%^&*()}{|\":?><\\[\\]\\;'/.,~]");
            return pattern.Replace(text, string.Empty).ToLower();
        }
    }
}