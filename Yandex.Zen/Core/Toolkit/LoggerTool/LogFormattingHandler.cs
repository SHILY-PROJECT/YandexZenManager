using System;
using Yandex.Zen.Core.Toolkit.BrowserCustomizer;

namespace Yandex.Zen.Core.Toolkit.LoggerTool
{
    public static class LogFormattingHandler
    {
        public static string FormatException(this Exception ex, string additionalMessage = "")
            => $"[Exception message:{ex.Message}]{Environment.NewLine}" +
            $"[{ex.StackTrace}]{Environment.NewLine}{additionalMessage}";

        public static string FormatXPathForLog(this string[] xpathHeAndNameHe) =>
           $"[{xpathHeAndNameHe[0]}]\t[{xpathHeAndNameHe[1]}]\tНе найден элемент по заданному пути...";

        public static string FormatXPathForLog(this HE htmlElement) =>
           $"[{htmlElement.XPath}]\t[{htmlElement.Description}]\tНе найден элемент по заданному пути...";
    }
}
