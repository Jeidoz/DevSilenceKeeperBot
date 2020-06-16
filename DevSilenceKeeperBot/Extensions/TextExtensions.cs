using System.Text.RegularExpressions;

namespace DevSilenceKeeperBot.Extensions
{
    public static class TextExtensions
    {
        public static string RemoveSpecialCharacters(this string text)
        {
            var pattern = new Regex(pattern: "[-:!@#$%^&*()}{|\":?><\\[\\]\\;'/.,~]");
            return pattern.Replace(text, string.Empty).ToLower();
        }
    }
}