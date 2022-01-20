using ZennoLab.InterfacesLibrary.Enums.Log;
using Yandex.Zen.Core.Interfaces;
using Yandex.Zen.Core.Toolkit;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;

namespace Yandex.Zen.Core.Services.BrowserAccountManagerService
{
    public class BrowserAccountManager : IService
    {
        public DataManager DataManager { get; set; }

        public BrowserAccountManager(DataManager manager)
        {
            DataManager = manager;
        }

        public void Start()
        {
            Logger.Write("Тест сервиса: MainBrowserAccountManager_new", LoggerType.Info, false, false, true, LogColor.Green);
        }
    }
}
