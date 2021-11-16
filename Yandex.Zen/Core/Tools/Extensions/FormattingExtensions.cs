namespace Yandex.Zen.Core.Tools.Extensions
{
    public static class FormattingExtensions
    {
        public static string XPathToStandardView(this string[] xpathHeAndNameHe) =>
                $"[{xpathHeAndNameHe[0]}]\t[{xpathHeAndNameHe[1]}]\tНе найден элемент по заданному пути...";

    }
}