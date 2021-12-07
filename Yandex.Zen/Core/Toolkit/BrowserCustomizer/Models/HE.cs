using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Toolkit.BrowserCustomizer.Enums;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using ZennoLab.CommandCenter;

namespace Yandex.Zen.Core.Toolkit.BrowserCustomizer.Models
{
    public class HE
    {
        #region====================================================================
        private Instance Browser { get => ProjectComponents.Project.Browser; }
        #endregion=================================================================

        private List<HtmlElement> _collection;

        public string XPath { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public HtmlElement Element { get => _collection.FirstOrDefault(); set { _collection = new List<HtmlElement>{ value }; } }
        public List<HtmlElement> Collection { get => _collection; set { _collection = value; } }
        public string InformationLog => $"'{nameof(XPath)}:{XPath} | {Title}' - Не найден элемент по заданному пути...";

        public HE(string xpath) => XPath = xpath;
        public HE(string xpath, string title) : this(xpath) => Title = title;
        public HE(string xpath, string title, HtmlElement htmlElement) : this(xpath, title) => Element = htmlElement;
        public HE(string xpath, string title, List<HtmlElement> htmlElementCollection) : this(xpath, title) => Collection = htmlElementCollection;


        /// <summary>
        /// Установить значение.
        /// </summary>
        public void SetValue(string value, LevelEmulation levelEmulation, int timeoutMillisecondsAfterAction,
            bool findElement = true, int attemptsFindElement = 3, bool throwExceptionIfElementNoFind = true, bool logger = true)
        {
            if (findElement)
                Find(attemptsFindElement, throwExceptionIfElementNoFind, logger);
            Element.SetValue(Browser.ActiveTab, value, levelEmulation, timeoutMillisecondsAfterAction);
        }

        /// <summary>
        /// Клик по элементу.
        /// </summary>
        public void Click(int timeoutMillisecondsAfterAction = 0, bool ifBusyThenWait = true,
            bool findElement = true, int attemptsFindElement = 3, bool throwExceptionIfElementNoFind = true, bool logger = true)
        {
            if (findElement)
                Find(attemptsFindElement, throwExceptionIfElementNoFind, logger);
            Element.Click(Browser.ActiveTab, timeoutMillisecondsAfterAction, ifBusyThenWait);
        }

        /// <summary>
        /// Поиск элемента.
        /// </summary>
        /// <param name="attemptsFindElement"></param>
        /// <param name="throwExceptionIfElementNoFind"></param>
        /// <param name="logger"></param>
        public void Find(int attemptsFindElement = 3, bool throwExceptionIfElementNoFind = true, bool logger = true)
        {
            //Element = Browser.FuncGetFirstHe(this, false, false, attemptsFindElement);
            Collection = Browser.FuncGetHeCollection(this, false, false, attemptsFindElement).ToList();

            if (Collection is null || !Collection.Any())
            {
                if (logger) Logger.Write(InformationLog, LoggerType.Warning, true, false, false);
                if (throwExceptionIfElementNoFind) throw new Exception(InformationLog);
            }
        }
    }
}
