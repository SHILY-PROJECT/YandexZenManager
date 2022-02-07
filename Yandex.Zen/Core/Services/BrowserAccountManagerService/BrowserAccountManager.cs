using ZennoLab.InterfacesLibrary.Enums.Log;
using Yandex.Zen.Core.Interfaces;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using Yandex.Zen.Core.ServicesComponents.ResourceObject.Interfaces;

namespace Yandex.Zen.Core.Services.BrowserAccountManagerService
{
    public class BrowserAccountManager : IService
    {
        public BrowserAccountManager(IDataManager manager)
        {
            DataManager = manager;
        }

        public IDataManager DataManager { get; set; }
        public IAccount Account { get; set; }
        public static bool IsInProcess { get; set; }

        public void Start()
        {
            Logger.Write($"Тест сервиса: {nameof(BrowserAccountManager)}", LoggerType.Info, false, false, true, LogColor.Green);
        }
    }
}
