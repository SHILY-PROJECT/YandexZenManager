using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Toolkit.BrowserCustomizer.Enums;
using ZennoLab.CommandCenter;

namespace Yandex.Zen.Core.Toolkit.BrowserCustomizer.Models
{
    public class HE
    {
        public string XPath { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public HtmlElement Element { get; set; }
        public HtmlElementCollection Collection { get; set; }

        public HE(string xpath) => XPath = xpath;
        public HE(string xpath, string title) : this(xpath) => Title = title;
        public HE(string xpath, string title, HtmlElement htmlElement) : this(xpath, title) => Element = htmlElement;
        public HE(string xpath, string title, HtmlElementCollection htmlElementCollection) : this(xpath, title) => Collection = htmlElementCollection;

        private Instance Browser { get => ProjectComponents.Project.Browser; }

        public void SetValue(string value, LevelEmulation levelEmulation, int timeoutMillisecondsAfterAction,
            bool findElement = true, int attemptsFindElement = 3, bool throwExceptionIfElementNoFind = true, bool logEnException = true)
        {
            if (findElement)
                FindElement(attemptsFindElement, throwExceptionIfElementNoFind, logEnException);

            if (Element is null)
                throw new Exception("HtmlElement is null");

            Element.SetValue(Browser.ActiveTab, value, levelEmulation, timeoutMillisecondsAfterAction);
        }

        public void Click(int timeoutMillisecondsAfterAction = 0, bool ifBusyThenWait = true,
            bool findElement = true, int attemptsFindElement = 3, bool throwExceptionIfElementNoFind = true, bool logEnException = true)
        {
            if (findElement)
                FindElement(attemptsFindElement, throwExceptionIfElementNoFind, logEnException);

            Element.Click(Browser.ActiveTab, timeoutMillisecondsAfterAction, ifBusyThenWait);
        }

        public void FindElement(int attemptsFindElement = 3, bool throwExceptionIfElementNoFind = true, bool logEnException = true)
            => Element = Browser.FuncGetFirstHe(this, throwExceptionIfElementNoFind, logEnException, attemptsFindElement);
        

    }
}
