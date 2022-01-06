using System;
using Yandex.Zen.Core.Interfaces;
using Yandex.Zen.Core.Interfaces.Services;
using Yandex.Zen.Core.Toolkit;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using ZennoLab.InterfacesLibrary.Enums.Log;

namespace Yandex.Zen.Core.Services.ChannelManagerService
{
    public class MainChannelManager : IChannelManagerService
    {
        public DataManager DataManager { get; set; }
        public IAuthorizationModule Authorization { get; set; }

        public MainChannelManager(DataManager manager)
        {
            DataManager = manager;
        }

        public void Start()
        {
            Logger.Write("Тест сервиса: MainChannelManager_new", LoggerType.Info, false, false, true, LogColor.Green);
        }
    }
}
