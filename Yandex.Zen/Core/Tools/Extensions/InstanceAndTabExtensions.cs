using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Yandex.Zen.Core.Enums.Extensions;
using Yandex.Zen.Core.Tools.LoggerTool;
using Yandex.Zen.Core.Tools.LoggerTool.Enums;
using ZennoLab.CommandCenter;

namespace Yandex.Zen.Core.Tools.Extensions
{
    public static class InstanceAndTabExtensions
    {
        /// <summary>
        /// Получение первого HtmlElement.
        /// </summary>
        /// <param name="xpath"></param>
        /// <param name="name"></param>
        /// <param name="throwException"></param>
        /// <param name="logger"></param>
        /// <param name="numberSecondsWaitElement"></param>
        /// <returns></returns>
        public static HtmlElement FuncGetFirstHe(this Instance instance, string xpath, string name = "", bool throwException = true, bool logger = true, int numberSecondsWaitElement = 3)
        {
            var counter = 0;

            while (true)
            {
                var he = instance.ActiveTab.FindElementByXPath(xpath, 0);

                if (he.IsNullOrVoid())
                {
                    if (++counter < numberSecondsWaitElement)
                    {
                        Thread.Sleep(1000);

                        if (instance.ActiveTab.IsBusy)
                            instance.ActiveTab.WaitDownloading();

                        continue;
                    }

                    name = name != "" ? $"[{name}]\t" : "";
                    var textLog = $"[XPath: {xpath}]\t{name}Элемент не найден";

                    if (logger) Logger.Write(textLog, LoggerType.Warning, true, false, false);
                    if (throwException) throw new Exception(textLog);

                    return null;
                }
                else return he;
            }
        }

        /// <summary>
        /// Получение первого HtmlElement.
        /// </summary>
        /// <param name="xpathAndName"></param>
        /// <param name="throwException"></param>
        /// <param name="logger"></param>
        /// <param name="numberSecondsWaitElement"></param>
        /// <returns></returns>
        public static HtmlElement FuncGetFirstHe(this Instance instance, string[] xpathAndName, bool throwException = true, bool logger = true, int numberSecondsWaitElement = 3) =>
            FuncGetFirstHe(instance, xpathAndName[0], xpathAndName[1], throwException, logger, numberSecondsWaitElement);

        /// <summary>
        /// Получение HtmlElement.
        /// </summary>
        /// <param name="xpath"></param>
        /// <param name="name"></param>
        /// <param name="throwException"></param>
        /// <param name="logger"></param>
        /// <param name="numberSecondsWaitElement"></param>
        /// <returns></returns>
        public static HtmlElement FuncGetHeByNumb(this Instance instance, string xpath, int numbElement, string name = "", bool throwException = true, bool logger = true, int numberSecondsWaitElement = 3)
        {
            var counter = 0;

            while (true)
            {
                var he = instance.ActiveTab.FindElementByXPath(xpath, numbElement);

                if (he.IsNullOrVoid())
                {
                    if (++counter < numberSecondsWaitElement)
                    {
                        Thread.Sleep(1000);

                        if (instance.ActiveTab.IsBusy)
                            instance.ActiveTab.WaitDownloading();

                        continue;
                    }

                    name = name != "" ? $"[{name}]\t" : "";
                    var textLog = $"[XPath: {xpath}]\t{name}Элемент не найден";

                    if (logger) Logger.Write(textLog, LoggerType.Warning, true, false, false);
                    if (throwException) throw new Exception(textLog);

                    return null;
                }
                else return he;
            }
        }

        /// <summary>
        /// Получение HtmlElementCollection.
        /// </summary>
        /// <param name="xpathAndName"></param>
        /// <param name="throwException"></param>
        /// <param name="logger"></param>
        /// <param name="numberSecondsWaitElement"></param>
        /// <returns></returns>
        public static HtmlElementCollection FuncGetHeCollection(this Instance instance, string[] xpathAndName, bool throwException = true, bool logger = true, int numberSecondsWaitElement = 3) =>
            FuncGetHeCollection(instance, xpathAndName[0], xpathAndName[1], throwException, logger, numberSecondsWaitElement);

        /// <summary>
        /// Получение HtmlElementCollection.
        /// </summary>
        /// <param name="xpath"></param>
        /// <param name="name"></param>
        /// <param name="throwException"></param>
        /// <param name="logger"></param>
        /// <param name="numberSecondsWaitElement"></param>
        /// <returns></returns>
        public static HtmlElementCollection FuncGetHeCollection(this Instance instance, string xpath, string name = "", bool throwException = true, bool logger = true, int numberSecondsWaitElement = 3)
        {
            var counter = 0;

            while (true)
            {
                var heCollection = instance.ActiveTab.FindElementsByXPath(xpath);

                if (heCollection.Count == 0)
                {
                    if (++counter < numberSecondsWaitElement)
                    {
                        Thread.Sleep(1000);

                        if (instance.ActiveTab.IsBusy) instance.ActiveTab.WaitDownloading();

                        continue;
                    }

                    name = name != "" ? $"[{name}]\t" : "";
                    var textLog = $"[XPath: {xpath}]\t{name}Не найдено ни одного элемента";

                    if (logger) Logger.Write(textLog, LoggerType.Warning, true, false, false);
                    if (throwException) throw new Exception(textLog);

                    return null;
                }
                else return heCollection;
            }
        }

        /// <summary>
        /// Установить файл для загрузки на сервер с возможностью SetFileUploadPolicyOk.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="fileInfo"></param>
        /// <param name="setFileUploadPolicyOk"></param>
        public static void SetFilesForUpload(this Instance instance, FileInfo fileInfo, bool setFileUploadPolicyOk = true)
        {
            if (setFileUploadPolicyOk)
                instance.SetFileUploadPolicy("ok", "");

            instance.SetFilesForUpload(fileInfo.FullName);
        }

        /// <summary>
        /// Открыть новую вкладку с возможностью использования проверки "IsBusy" и "WaitDownloading".
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="address"></param>
        /// <param name="ifBusyThenWait"></param>
        public static void NewTab(this Instance instance, string address, bool ifBusyThenWait)
        {
            instance.NewTab(address);

            if (ifBusyThenWait && instance.ActiveTab.IsBusy) instance.ActiveTab.WaitDownloading();
        }

        /// <summary>
        /// Открыть URL в новом Tab (в новой вкладке).
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="titleTab">Имя tab.</param>
        /// <param name="url"></param>
        /// <param name="refferer"></param>
        /// <param name="ifBusyThenWait"></param>
        public static void NavigateInNewTab(this Instance instance, string titleTab, string url, string refferer = "", bool ifBusyThenWait = true)
        {
            instance.NewTab(titleTab);

            if (ifBusyThenWait && instance.ActiveTab.IsBusy) instance.ActiveTab.WaitDownloading();

            instance.ActiveTab.Navigate(url, refferer);

            if (ifBusyThenWait && instance.ActiveTab.IsBusy) instance.ActiveTab.WaitDownloading();
        }

        /// <summary>
        /// Вызвать событие KeyEvent с возможностью использования проверки "IsBusy" и "WaitDownloading".
        /// </summary>
        /// <param name="tab"></param>
        /// <param name="key"></param>
        /// <param name="keyEvent"></param>
        /// <param name="keyModifer"></param>
        /// <param name="ifBusyAfterActionThenWait"></param>
        /// <param name="timeoutMillisecondsAfterAction"></param>
        public static void KeyEvent(this Tab tab, string key, string keyEvent, string keyModifer, bool ifBusyAfterActionThenWait = true, int timeoutMillisecondsAfterAction = 0)
        {
            tab.KeyEvent(key, keyEvent, keyModifer);

            if (ifBusyAfterActionThenWait && tab.IsBusy) tab.WaitDownloading();

            if (timeoutMillisecondsAfterAction != 0)
            {
                Thread.Sleep(timeoutMillisecondsAfterAction);

                if (ifBusyAfterActionThenWait && tab.IsBusy) tab.WaitDownloading();
            }
        }

        public static void Refresh(this Tab tab, TypeRefreshEnum typeRefresh)
        {
            switch (typeRefresh)
            {
                case TypeRefreshEnum.JavaScript:
                    tab.MainDocument.EvaluateScript("location.reload(true)");
                    tab.MainDocument.EvaluateScript("history.go(0)");
                    break;
                case TypeRefreshEnum.Navigate:
                    tab.Navigate(tab.URL);
                    break;
            }

            if (tab.IsBusy) tab.WaitDownloading();
        }

        public static void Navigate(this Tab tab, string url, string referrer, bool ifBusyThenWait = true)
        {
            tab.Navigate(url, referrer);

            if (ifBusyThenWait && tab.IsBusy) tab.WaitDownloading();
        }

        public static void Navigate(this Tab tab, string url, bool ifBusyThenWait)
        {
            tab.Navigate(url);

            if (ifBusyThenWait && tab.IsBusy) tab.WaitDownloading();
        }

        public static void FullEmulationMouseMoveAboveHtmlElement(this Tab tab, HtmlElement he, int sizeOfType, bool ifBusyThenWait = true)
        {
            tab.FullEmulationMouseMoveAboveHtmlElement(he, sizeOfType);

            if (ifBusyThenWait && tab.IsBusy) tab.WaitDownloading();
        }

    }
}
