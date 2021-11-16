using Global.ZennoExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Yandex.Zen.Core.Tools.Macros
{
    public class TextMacros
    {
        public static string GenerateString(int lengthStringForom, int lengthStringTo, string pattern, string customSymbols = "") =>
            GenerateString(new Random().Next(lengthStringForom < lengthStringTo ? lengthStringForom : lengthStringTo, lengthStringTo + 1), pattern, customSymbols);

        public static string GenerateString(int lengthString, string pattern, string customSymbols = "")
        {
            var rnd = new Random();
            var temp = new List<string>();
            var chars = new char[lengthString];

            if (Regex.IsMatch(pattern, "a.*?")) temp.Add("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
            if (Regex.IsMatch(pattern, "b.*?")) temp.Add("abcdefghijklmnopqrstuvwxyz");
            if (Regex.IsMatch(pattern, "c.*?")) temp.Add("0123456789");
            if (Regex.IsMatch(pattern, "d.*?")) temp.Add("$^%#*");
            if (!string.IsNullOrEmpty(customSymbols)) temp.Add(customSymbols);

            temp.Shuffle();

            var symbols = temp.ToArray();
            var line = string.Join("", symbols);

            for (int i = 0; i < chars.Length; i++) chars[i] = line[rnd.Next(line.Length)];

            if (chars.Length >= symbols.Length)
            {
                for (int i = 0; i < symbols.Length; i++)
                {
                    if (!string.IsNullOrEmpty(symbols[i])) chars[i] = Convert.ToChar(symbols[i].Substring(rnd.Next(symbols[i].Length), 1));
                }
            }

            return string.Join("", chars);
        }
    }
}
