﻿using ZennoLab.InterfacesLibrary.Enums.Log;
using Yandex.Zen.Core.Interfaces;
using Yandex.Zen.Core.Toolkit;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;

namespace Yandex.Zen.Core.Services.ChannelManagerService
{
    public class ChannelManager : IService
    {
        public DataManager DataManager { get; set; }
        public IAuthorizationModule Authorization { get; set; }

        public ChannelManager(DataManager manager)
        {
            DataManager = manager;
        }

        public void Start()
        {
            Logger.Write($"Тест сервиса: {nameof(ChannelManager)}", LoggerType.Info, false, false, true, LogColor.Green);
        }
    }
}
