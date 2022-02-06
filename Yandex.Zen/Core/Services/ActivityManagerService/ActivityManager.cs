using ZennoLab.InterfacesLibrary.Enums.Log;
using Yandex.Zen.Core.Interfaces;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;

namespace Yandex.Zen.Core.Services.ActivityManagerService
{
    public class ActivityManager : IService
    {
        public DataManager DataManager { get; set; }

        public ActivityManager(DataManager manager)
        {
            DataManager = manager;
        }

        public void Start()
        {
            Logger.Write($"Тест сервиса: {nameof(ActivityManager)}", LoggerType.Info, false, false, true, LogColor.Green);
        }
    }
}
