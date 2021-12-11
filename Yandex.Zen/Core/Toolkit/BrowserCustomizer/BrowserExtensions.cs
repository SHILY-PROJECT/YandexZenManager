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
        /// <param name="attemptsFind"></param>
        /// <returns></returns>
        public static HtmlElement FindFirstElement(this Instance instance, string xpath, string name = "",
            bool throwException = true, bool logger = true, int attemptsFind = 3)
        {
            var counter = 0;

            while (true)
            {
                var he = instance.ActiveTab.FindElementByXPath(xpath, 0);

                if (he.IsNullOrVoid())
                {
                    if (++counter < attemptsFind)
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
        /// <param name="attemptsFind"></param>
        /// <returns></returns>
        public static HtmlElement FindFirstElement(this Instance instance,
            string[] xpathAndName, bool throwException = true, bool logger = true, int attemptsFind = 3) =>
            instance.FindFirstElement(xpathAndName[0], xpathAndName[1], throwException, logger, attemptsFind);

        /// <summary>
        /// Получение первого HtmlElement.
        /// </summary>
        public static HtmlElement FindFirstElement(this Instance instance, HE htmlElement,
            bool throwException = true, bool logger = true, int attemptsFind = 3)
            => instance.FindFirstElement(htmlElement.XPath, htmlElement.Info, throwException, logger, attemptsFind);

        /// <summary>
        /// Получение HtmlElement.
        /// </summary>
        /// <param name="xpath"></param>
        /// <param name="name"></param>
        /// <param name="throwException"></param>
        /// <param name="logger"></param>
        /// <param name="secWaitElement"></param>
        /// <returns></returns>
        public static HtmlElement FindElementByNumber(this Instance instance, string xpath,
            int numbElement, string name = "", bool throwException = true, bool logger = true, int secWaitElement = 3)
        {
            var counter = 0;

            while (true)
            {
                var he = instance.ActiveTab.FindElementByXPath(xpath, numbElement);

                if (he.IsNullOrVoid())
                {
                    if (++counter < secWaitElement)
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
        /// <param name="secWaitElement"></param>
        /// <returns></returns>
        public static HtmlElementCollection FindElements(this Instance instance, string[] xpathAndName,
            bool throwException = true, bool logger = true, int secWaitElement = 3) =>
            instance.FindElements(xpathAndName[0], xpathAndName[1], throwException, logger, secWaitElement);

        /// <summary>
        /// Получение HtmlElementCollection.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="htmlElement"></param>
        /// <param name="throwException"></param>
        /// <param name="logger"></param>
        /// <param name="secWaitElement"></param>
        /// <returns></returns>
        public static HtmlElementCollection FindElements(this Instance instance, HE htmlElement,
            bool throwException = true, bool logger = true, int secWaitElement = 3) =>
            instance.FindElements(htmlElement.XPath, htmlElement.Info, throwException, logger, secWaitElement);

        /// <summary>
        /// Получение HtmlElementCollection.
        /// </summary>
        /// <param name="xpath"></param>
        /// <param name="name"></param>
        /// <param name="throwException"></param>
        /// <param name="logger"></param>
        /// <param name="secWaitElement"></param>
        /// <returns></returns>
        public static HtmlElementCollection FindElements(this Instance instance, string xpath,
            string name = "", bool throwException = true, bool logger = true, int secWaitElement = 3)
        {
            var counter = 0;

            while (true)
            {
                var heCollection = instance.ActiveTab.FindElementsByXPath(xpath);

                if (heCollection.Count == 0)
                {
                    if (++counter < secWaitElement)
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
        /// <param name="waitPageLoad"></param>
        /// <param name="msTimeoutAfterAction"></param>
        public static void KeyEvent(this Tab tab, string key, string keyEvent, string keyModifer, bool waitPageLoad = true, int msTimeoutAfterAction = 0)
        {
            tab.KeyEvent(key, keyEvent, keyModifer);

            if (waitPageLoad && tab.IsBusy) tab.WaitDownloading();

            if (msTimeoutAfterAction != 0)
            {
                Thread.Sleep(msTimeoutAfterAction);
                if (waitPageLoad && tab.IsBusy) tab.WaitDownloading();
            }
        }

        /// <summary>
        /// Перезагрузка страницы.
        /// </summary>
        /// <param name="tab"></param>
        /// <param name="typeRefresh"></param>
        public static void Refresh(this Tab tab, RefreshTypeEnum typeRefresh)
        {
            switch (typeRefresh)
            {
                case RefreshTypeEnum.JavaScript:
                    tab.MainDocument.EvaluateScript("location.reload(true)");
                    tab.MainDocument.EvaluateScript("history.go(0)");
                    break;
                case RefreshTypeEnum.Navigate:
                    tab.Navigate(tab.URL);
                    break;
            }

            if (tab.IsBusy) tab.WaitDownloading();
        }

        /// <summary>
        /// Переход по указанной ссылке.
        /// </summary>
        /// <param name="tab"></param>
        /// <param name="url"></param>
        /// <param name="referrer"></param>
        /// <param name="waitPageLoad"></param>
        public static void Navigate(this Tab tab, string url, string referrer, bool waitPageLoad = true)
        {
            tab.Navigate(url, referrer);
            if (waitPageLoad && tab.IsBusy) tab.WaitDownloading();
        }

        /// <summary>
        /// Переход по указанной ссылке.
        /// </summary>
        public static void Navigate(this Tab tab, string url, bool waitPageLoad)
        {
            tab.Navigate(url);
            if (waitPageLoad && tab.IsBusy) tab.WaitDownloading();
        }

        /// <summary>
        /// Эмуляция ведения мышки по элементу.
        /// </summary>
        /// <param name="tab"></param>
        /// <param name="he"></param>
        /// <param name="sizeOfType"></param>
        /// <param name="waitPageLoad"></param>
        public static void FullEmulationMouseMoveAboveHtmlElement(this Tab tab, HtmlElement he, int sizeOfType, bool waitPageLoad = true)
        {
            tab.FullEmulationMouseMoveAboveHtmlElement(he, sizeOfType);
            if (waitPageLoad && tab.IsBusy) tab.WaitDownloading();
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
        public static void SetValue(this HtmlElement he, Tab tab, string value, LevelEmulation levelEmulation,
            int msTimeoutAfterAction = 0, bool append = false,
            bool useKeyEventEnter = false, int msTimeoutAfterKeyEvent = 0)
            => he.SetValue(tab, value, levelEmulation.ToString(), msTimeoutAfterAction, append, useKeyEventEnter, msTimeoutAfterKeyEvent);

        /// <summary>
        /// Устанавливает значение элемента html.
        /// </summary>
        /// <param name="he">Объект типа HtmlElement.</param>
        /// <param name="tab">Активная вкладка Tab (instance.ActiveTab).</param>
        /// <param name="value">Новое значение для этого элемента html.</param>
        /// <param name="emulationLevel">Уровень эмуляции. Может быть: "None", "Middle", "Full" или "SuperEmulation".</param>
        /// <param name="msTimeoutAfterAction">Таймаут после действия.</param>
        /// <param name="useSelectedItems">true, если нужно использовать стандартные поля "select" с автоматическим заполнением; в противном случае - false.</param>
        /// <param name="append">true, если вам нужно добавить значение к существующему содержимому; в противном случае - false.</param>
        private static void SetValue(this HtmlElement he, Tab tab, string value, string emulationLevel,
            int msTimeoutAfterAction = 0, bool append = false,
            bool useKeyEventEnter = false, int msTimeoutAfterKeyEvent = 0)
        {
            if (he.IsNullOrVoid()) return;

            // Установка значения в поле
            he.SetValue(value, emulationLevel, false, append);

            if (tab.IsBusy) tab.WaitDownloading();

            if (msTimeoutAfterAction != 0)
            {
                Thread.Sleep(msTimeoutAfterAction);
                if (tab.IsBusy) tab.WaitDownloading();
            }

            // Вызов события Enter
            if (useKeyEventEnter)
            {
                tab.KeyEvent("Enter", "press", "");

                if (tab.IsBusy) tab.WaitDownloading();

                if (msTimeoutAfterKeyEvent != 0)
                {
                    Thread.Sleep(msTimeoutAfterKeyEvent);

                    if (tab.IsBusy) tab.WaitDownloading();
                }
            }
        }

        /// <summary>
        /// Выполняет событие щелчка на элементе html.
        /// </summary>
        /// <param name="he">Объект типа HtmlElement.</param>
        /// <param name="tab">Активная вкладка Tab (instance.ActiveTab).</param>
        /// <param name="msTimeoutAfterAction">Таймаут после действия.</param>
        public static void Click(this HtmlElement he, Tab tab, int msTimeoutAfterAction = 0, bool waitPageLoad = true)
        {
            if (he.IsNullOrVoid()) return;

            he.Click();

            if (waitPageLoad && tab.IsBusy) tab.WaitDownloading();

            if (msTimeoutAfterAction != 0)
            {
                Thread.Sleep(msTimeoutAfterAction);

                if (waitPageLoad && tab.IsBusy) tab.WaitDownloading();
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
