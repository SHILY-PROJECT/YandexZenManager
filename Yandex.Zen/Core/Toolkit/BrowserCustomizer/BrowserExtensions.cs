using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Yandex.Zen.Core.Toolkit.Extensions.Enums;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using ZennoLab.CommandCenter;
using Yandex.Zen.Core.Toolkit.BrowserCustomizer.Models;
using Yandex.Zen.Core.Toolkit.BrowserCustomizer.Enums;

namespace Yandex.Zen.Core.Toolkit.BrowserCustomizer
{
    public static class BrowserExtensions
    {
        /// <summary>
        /// Получение первого HtmlElement.
        /// </summary>
        /// <param name="xpath"></param>
        /// <param name="name"></param>
        /// <param name="throwException"></param>
        /// <param name="logger"></param>
        /// <param name="attemptsFindElement"></param>
        /// <returns></returns>
        public static HtmlElement FuncGetFirstHe(this Instance instance, string xpath, string name = "", bool throwException = true, bool logger = true, int attemptsFindElement = 3)
        {
            var counter = 0;

            while (true)
            {
                var he = instance.ActiveTab.FindElementByXPath(xpath, 0);

                if (he.IsNullOrVoid())
                {
                    if (++counter < attemptsFindElement)
                    {
                        Thread.Sleep(1000);

                        if (instance.ActiveTab.IsBusy)
                            instance.ActiveTab.WaitDownloading();

                        continue;
                    }

                    name = !string.IsNullOrWhiteSpace(name) ? $"[{name}]\t" : "";
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
        /// <param name="attemptsFindElement"></param>
        /// <returns></returns>
        public static HtmlElement FuncGetFirstHe(this Instance instance, string[] xpathAndName, bool throwException = true, bool logger = true, int attemptsFindElement = 3) =>
            instance.FuncGetFirstHe(xpathAndName[0], xpathAndName[1], throwException, logger, attemptsFindElement);

        /// <summary>
        /// Получение первого HtmlElement.
        /// </summary>
        public static HtmlElement FuncGetFirstHe(this Instance instance, HE htmlElement, bool throwException = true, bool logger = true, int attemptsFindElement = 3)
            => instance.FuncGetFirstHe(htmlElement.XPath, htmlElement.Title, throwException, logger, attemptsFindElement);

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
            instance.FuncGetHeCollection(xpathAndName[0], xpathAndName[1], throwException, logger, numberSecondsWaitElement);
        public static HtmlElementCollection FuncGetHeCollection(this Instance instance, HE htmlElement, bool throwException = true, bool logger = true, int numberSecondsWaitElement = 3) =>
            instance.FuncGetHeCollection(htmlElement.XPath, htmlElement.Title, throwException, logger, numberSecondsWaitElement);

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

        /// <summary>
        /// Устанавливает значение элемента html.
        /// </summary>
        /// <param name="he">Объект типа HtmlElement.</param>
        /// <param name="tab">Активная вкладка Tab (instance.ActiveTab).</param>
        /// <param name="value">Новое значение для этого элемента html.</param>
        /// <param name="levelEmulation">Уровень эмуляции. Может быть: "None", "Middle", "Full" или "SuperEmulation".</param>
        /// <param name="timeoutAfterAction">Таймаут после действия.</param>
        /// <param name="useSelectedItems">true, если нужно использовать стандартные поля "select" с автоматическим заполнением; в противном случае - false.</param>
        /// <param name="append">true, если вам нужно добавить значение к существующему содержимому; в противном случае - false.</param>
        public static void SetValue(this HtmlElement he, Tab tab, string value, LevelEmulation levelEmulation, int timeoutMillisecondsAfterAction = 0, bool useSelectedItems = false, bool append = false, bool useKeyEventEnterAfterSetValue = false, int timeoutMillisecondsAfterKeyEventEnter = 0) =>
            he.SetValue(tab, value, levelEmulation.ToString(), timeoutMillisecondsAfterAction, useSelectedItems, append, useKeyEventEnterAfterSetValue, timeoutMillisecondsAfterKeyEventEnter);

        /// <summary>
        /// Устанавливает значение элемента html.
        /// </summary>
        /// <param name="he">Объект типа HtmlElement.</param>
        /// <param name="tab">Активная вкладка Tab (instance.ActiveTab).</param>
        /// <param name="value">Новое значение для этого элемента html.</param>
        /// <param name="emulationLevel">Уровень эмуляции. Может быть: "None", "Middle", "Full" или "SuperEmulation".</param>
        /// <param name="timeoutMillisecondsAfterAction">Таймаут после действия.</param>
        /// <param name="useSelectedItems">true, если нужно использовать стандартные поля "select" с автоматическим заполнением; в противном случае - false.</param>
        /// <param name="append">true, если вам нужно добавить значение к существующему содержимому; в противном случае - false.</param>
        private static void SetValue(this HtmlElement he, Tab tab, string value, string emulationLevel, int timeoutMillisecondsAfterAction = 0, bool useSelectedItems = false, bool append = false, bool useKeyEventEnterAfterSetValue = false, int timeoutMillisecondsAfterKeyEventEnter = 0)
        {
            if (he.IsNullOrVoid()) return;

            // Установка значения в поле
            he.SetValue(value, emulationLevel, useSelectedItems, append);

            if (tab.IsBusy) tab.WaitDownloading();

            if (timeoutMillisecondsAfterAction != 0)
            {
                Thread.Sleep(timeoutMillisecondsAfterAction);
                if (tab.IsBusy) tab.WaitDownloading();
            }

            // Вызов события Enter
            if (useKeyEventEnterAfterSetValue)
            {
                tab.KeyEvent("Enter", "press", "");

                if (tab.IsBusy) tab.WaitDownloading();

                if (timeoutMillisecondsAfterKeyEventEnter != 0)
                {
                    Thread.Sleep(timeoutMillisecondsAfterKeyEventEnter);

                    if (tab.IsBusy) tab.WaitDownloading();
                }
            }
        }

        /// <summary>
        /// Выполняет событие щелчка на элементе html.
        /// </summary>
        /// <param name="he">Объект типа HtmlElement.</param>
        /// <param name="tab">Активная вкладка Tab (instance.ActiveTab).</param>
        /// <param name="timeoutMillisecondsAfterAction">Таймаут после действия.</param>
        public static void Click(this HtmlElement he, Tab tab, int timeoutMillisecondsAfterAction = 0, bool ifBusyThenWait = true)
        {
            if (he.IsNullOrVoid()) return;

            he.Click();

            if (ifBusyThenWait && tab.IsBusy) tab.WaitDownloading();

            if (timeoutMillisecondsAfterAction != 0)
            {
                Thread.Sleep(timeoutMillisecondsAfterAction);

                if (ifBusyThenWait && tab.IsBusy) tab.WaitDownloading();
            }
        }

        /// <summary>
        /// Проверяет элемент на null или void.
        /// </summary>
        /// <param name="he"></param>
        /// <returns></returns>
        public static bool IsNullOrVoid(this HtmlElement he) => he == null || he.IsNull || he.IsVoid;

    }
}
