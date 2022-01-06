using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Interfaces.Services;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using ZennoLab.InterfacesLibrary.Enums.Log;

namespace Yandex.Zen.Core.Services.BrowserAccountManagerService
{
    public class MainBrowserAccountManager : IBrowserAccountManagerService
    {
        public DataManager DataManager { get; set; }

        public MainBrowserAccountManager(DataManager manager)
        {
            DataManager = manager;
        }

        public void Start()
        {
            Logger.Write("Тест сервиса: MainBrowserAccountManager_new", LoggerType.Info, false, false, true, LogColor.Green);
        }
    }
}
