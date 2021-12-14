using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Toolkit.BrowserCustomizer.Models;

namespace Yandex.Zen.Core.Toolkit
{
    public static class FormattingHandler
    {
        public static string FormatExceptionForLog(this Exception ex, string additionalMessage = "")
            => $"[Exception message:{ex.Message}]{Environment.NewLine}" +
            $"[{ex.StackTrace}]{Environment.NewLine}{additionalMessage}";

        public static string FormatXPathForLog(this string[] xpathHeAndNameHe) =>
           $"[{xpathHeAndNameHe[0]}]\t[{xpathHeAndNameHe[1]}]\tНе найден элемент по заданному пути...";

        public static string FormatXPathForLog(this HE htmlElement) =>
           $"[{htmlElement.XPath}]\t[{htmlElement.Description}]\tНе найден элемент по заданному пути...";
    }
}
