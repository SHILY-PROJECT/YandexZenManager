﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Interfaces.Services;
using Yandex.Zen.Core.Toolkit;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using ZennoLab.InterfacesLibrary.Enums.Log;

namespace Yandex.Zen.Core.Services.WalkerOnZenService
{
    public class MainWalkerOnZen : IWalkerOnZenService
    {
        public DataManager DataManager { get; set; }

        public MainWalkerOnZen(DataManager manager)
        {
            DataManager = manager;
        }

        public void Start()
        {
            Logger.Write("Тест сервиса: MainWalkerOnZen_new", LoggerType.Info, false, false, true, LogColor.Green);
        }
    }
}
