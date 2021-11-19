using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZennoLab.CommandCenter;

namespace Yandex.Zen.Core.Toolkit.Macros
{
    public class TabMacros
    {
        /// <summary>
        /// Проверка наличия элемента на странице (true - элемент присутствует, false - элемент отсутствует).
        /// </summary>
        /// <param name="tab"></param>
        /// <param name="xpath"></param>
        /// <param name="matchNumber"></param>
        /// <param name="numberOfAttempts"></param>
        /// <param name="timeoutBetweenAttempts"></param>
        /// <returns></returns>
        public static bool CheckExistenceHtmlElementOnPage(Tab tab, string xpath, int matchNumber = 0, int numberOfAttempts = 10, int timeoutBetweenAttempts = 500)
        {
            var counterWaitElement = 0;

            while (true)
            {
                if (++counterWaitElement > numberOfAttempts) return false;

                if (tab.IsBusy) tab.WaitDownloading();

                var searchInput = tab.FindElementByXPath(xpath, matchNumber);

                if (!searchInput.IsVoid && !searchInput.IsNull) return true;

                Thread.Sleep(timeoutBetweenAttempts);
            }
        }

        /// <summary>
        /// Получить HtmlElement со страницы.
        /// </summary>
        /// <param name="tab"></param>
        /// <param name="xpath"></param>
        /// <param name="matchNumber"></param>
        /// <param name="numberOfAttempts"></param>
        /// <param name="timeoutBetweenAttempts"></param>
        /// <returns></returns>
        public static HtmlElement GetHtmlElement(Tab tab, string xpath, int matchNumber = 0, int numberOfAttempts = 10, int timeoutBetweenAttempts = 500)
        {
            var counterWaitElement = 0;

            while (true)
            {
                if (++counterWaitElement > numberOfAttempts) return null;

                if (tab.IsBusy) tab.WaitDownloading();

                var htmlElement = tab.FindElementByXPath(xpath, matchNumber);

                if (!htmlElement.IsVoid && !htmlElement.IsNull) return htmlElement;

                Thread.Sleep(timeoutBetweenAttempts);
            }
        }
    }
}
