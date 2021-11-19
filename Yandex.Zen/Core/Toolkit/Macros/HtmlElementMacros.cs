using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZennoLab.CommandCenter;
using Yandex.Zen.Core.Toolkit.Extensions;
using System.Threading;

namespace Yandex.Zen.Core.Toolkit.Macros
{
    public class HtmlElementMacros
    {
        ///// <summary>
        ///// Получение первого HtmlElement.
        ///// </summary>
        ///// <param name="xpath"></param>
        ///// <param name="name"></param>
        ///// <param name="throwException"></param>
        ///// <param name="logger"></param>
        ///// <param name="numberSecondsWaitElement"></param>
        ///// <returns></returns>
        //public static HtmlElement instance.FuncGetFirstHe(string xpath, string name = "", bool throwException = true, bool logger = true, int numberSecondsWaitElement = 3)
        //{
        //    var counter = 0;

        //    while (true)
        //    {
        //        var he = instance.ActiveTab.FindElementByXPath(xpath, 0);

        //        if (he.IsNullOrVoid())
        //        {
        //            if (++counter < numberSecondsWaitElement)
        //            {
        //                Thread.Sleep(1000);

        //                if (instance.ActiveTab.IsBusy)
        //                    instance.ActiveTab.WaitDownloading();

        //                continue;
        //            }

        //            name = name != "" ? $"[{name}]\t" : "";
        //            var textLog = $"[XPath: {xpath}]\t{name}Элемент не найден";

        //            if (logger) Logger.LoggerWrite(textLog, LoggerType.Warning, true, false, false);
        //            if (throwException) throw new Exception(textLog);

        //            return null;
        //        }
        //        else return he;
        //    }
        //}

        ///// <summary>
        ///// Получение первого HtmlElement.
        ///// </summary>
        ///// <param name="xpathAndName"></param>
        ///// <param name="throwException"></param>
        ///// <param name="logger"></param>
        ///// <param name="numberSecondsWaitElement"></param>
        ///// <returns></returns>
        //public static HtmlElement instance.FuncGetFirstHe(string[] xpathAndName, bool throwException = true, bool logger = true, int numberSecondsWaitElement = 3) =>
        //    instance.FuncGetFirstHe(xpathAndName[0], xpathAndName[1], throwException, logger, numberSecondsWaitElement);

        ///// <summary>
        ///// Получение HtmlElement.
        ///// </summary>
        ///// <param name="xpath"></param>
        ///// <param name="name"></param>
        ///// <param name="throwException"></param>
        ///// <param name="logger"></param>
        ///// <param name="numberSecondsWaitElement"></param>
        ///// <returns></returns>
        //public static HtmlElement FuncGetHeByNumb(string xpath, int numbElement, string name = "", bool throwException = true, bool logger = true, int numberSecondsWaitElement = 3)
        //{
        //    var counter = 0;

        //    while (true)
        //    {
        //        var he = instance.ActiveTab.FindElementByXPath(xpath, numbElement);

        //        if (he.IsNullOrVoid())
        //        {
        //            if (++counter < numberSecondsWaitElement)
        //            {
        //                Thread.Sleep(1000);

        //                if (instance.ActiveTab.IsBusy)
        //                    instance.ActiveTab.WaitDownloading();

        //                continue;
        //            }

        //            name = name != "" ? $"[{name}]\t" : "";
        //            var textLog = $"[XPath: {xpath}]\t{name}Элемент не найден";

        //            if (logger) Logger.LoggerWrite(textLog, LoggerType.Warning, true, false, false);
        //            if (throwException) throw new Exception(textLog);

        //            return null;
        //        }
        //        else return he;
        //    }
        //}

        ///// <summary>
        ///// Получение HtmlElementCollection.
        ///// </summary>
        ///// <param name="xpathAndName"></param>
        ///// <param name="throwException"></param>
        ///// <param name="logger"></param>
        ///// <param name="numberSecondsWaitElement"></param>
        ///// <returns></returns>
        //public static HtmlElementCollection FuncGetHeCollection(string[] xpathAndName, bool throwException = true, bool logger = true, int numberSecondsWaitElement = 3) =>
        //    FuncGetHeCollection(xpathAndName[0], xpathAndName[1], throwException, logger, numberSecondsWaitElement);

        ///// <summary>
        ///// Получение HtmlElementCollection.
        ///// </summary>
        ///// <param name="xpath"></param>
        ///// <param name="name"></param>
        ///// <param name="throwException"></param>
        ///// <param name="logger"></param>
        ///// <param name="numberSecondsWaitElement"></param>
        ///// <returns></returns>
        //public static HtmlElementCollection FuncGetHeCollection(string xpath, string name = "", bool throwException = true, bool logger = true, int numberSecondsWaitElement = 3)
        //{
        //    var counter = 0;

        //    while (true)
        //    {
        //        var heCollection = instance.ActiveTab.FindElementsByXPath(xpath);

        //        if (heCollection.Count == 0)
        //        {
        //            if (++counter < numberSecondsWaitElement)
        //            {
        //                Thread.Sleep(1000);

        //                if (instance.ActiveTab.IsBusy) instance.ActiveTab.WaitDownloading();

        //                continue;
        //            }

        //            name = name != "" ? $"[{name}]\t" : "";
        //            var textLog = $"[XPath: {xpath}]\t{name}Не найдено ни одного элемента";

        //            if (logger) Logger.LoggerWrite(textLog, LoggerType.Warning, true, false, false);
        //            if (throwException) throw new Exception(textLog);

        //            return null;
        //        }
        //        else return heCollection;
        //    }
        //}

    }
}
