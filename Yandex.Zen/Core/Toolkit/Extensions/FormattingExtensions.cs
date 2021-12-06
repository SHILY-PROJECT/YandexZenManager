using Yandex.Zen.Core.Toolkit.BrowserCustomizer.Models;

namespace Yandex.Zen.Core.Toolkit.BrowserCustomizer
{
    public static class FormattingExtensions
    {
        public static string XPathToStandardView(this string[] xpathHeAndNameHe) =>
            $"[{xpathHeAndNameHe[0]}]\t[{xpathHeAndNameHe[1]}]\tНе найден элемент по заданному пути...";

    }
}