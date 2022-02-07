using ZennoLab.InterfacesLibrary.Enums.Log;
using Yandex.Zen.Core.Interfaces;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using Yandex.Zen.Core.ServicesComponents.ResourceObject.Interfaces;

namespace Yandex.Zen.Core.Services.ChannelManagerService
{
    public class ChannelManager : IService
    {
        public ChannelManager(IDataManager manager)
        {
            DataManager = manager;
        }

        public IDataManager DataManager { get; set; }
        public IAuthorizationModule Authorization { get; set; }
        public IAccount Account { get; set; }

        public void Start()
        {
            Logger.Write($"Тест сервиса: {nameof(ChannelManager)}", LoggerType.Info, false, false, true, LogColor.Green);
        }
    }
}
