using ZennoLab.InterfacesLibrary.Enums.Log;
using Yandex.Zen.Core.Interfaces;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;

namespace Yandex.Zen.Core.Services.BrowserAccountManagerService
{
    public class BrowserAccountManager : IService
    {
        public BrowserAccountManager(DataManager manager)
        {
            DataManager = manager;
        }

        public static bool IsInProcess { get; set; }

        public DataManager DataManager { get; set; }


        public void Start()
        {
            Logger.Write($"Тест сервиса: {nameof(BrowserAccountManager)}", LoggerType.Info, false, false, true, LogColor.Green);
        }
    }
}
