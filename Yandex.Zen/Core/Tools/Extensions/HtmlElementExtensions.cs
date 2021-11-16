using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZennoLab.CommandCenter;
using Yandex.Zen.Core.Enums.Extensions;

namespace Yandex.Zen.Core.Tools.Extensions
{
    public static class HtmlElementExtensions
    {
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
        /// <param name="timeoutAfterAction">Таймаут после действия.</param>
        /// <param name="useSelectedItems">true, если нужно использовать стандартные поля "select" с автоматическим заполнением; в противном случае - false.</param>
        /// <param name="append">true, если вам нужно добавить значение к существующему содержимому; в противном случае - false.</param>
        private static void SetValue(this HtmlElement he, Tab tab, string value, string emulationLevel, int timeoutMillisecondsAfterSetValue = 0, bool useSelectedItems = false, bool append = false, bool useKeyEventEnterAfterSetValue = false, int timeoutMillisecondsAfterKeyEventEnter = 0)
        {
            if (he.IsNullOrVoid()) return;

            // Установка значения в поле
            he.SetValue(value, emulationLevel, useSelectedItems, append);

            if (tab.IsBusy) tab.WaitDownloading();

            if (timeoutMillisecondsAfterSetValue != 0)
            {
                Thread.Sleep(timeoutMillisecondsAfterSetValue);

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
