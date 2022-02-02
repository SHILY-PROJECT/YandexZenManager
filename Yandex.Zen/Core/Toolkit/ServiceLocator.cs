using System;
using System.Collections.Generic;
using Yandex.Zen.Core.Services.AccounRegisterService;
using Yandex.Zen.Core.Services.ActivityManagerService;
using Yandex.Zen.Core.Services.BrowserAccountManagerService;
using Yandex.Zen.Core.Services.ChannelManagerService;
using Yandex.Zen.Core.Services.PublicationManagerService;
using Yandex.Zen.Core.Services.WalkerOnZenService;

namespace Yandex.Zen.Core.Toolkit
{
    public class ServiceLocator
    {
        private readonly DataManager _manager;

        public ServiceLocator(DataManager manager)
        {
            _manager = manager;
        }

        public static Dictionary<string, Type> TypesOfServices => new Dictionary<string, Type>
        {
            { "Управление каналом",             typeof(ChannelManager) },
            { "Постинг",                        typeof(PublicationManager) },
            { "Ручное управление в браузере",   typeof(BrowserAccountManager) },
            { "Нагуливание по дзен",            typeof(WalkerOnZen) },
            { "Регистрация аккаунтов",          typeof(AccounRegister) },
            { "Накручивание активности",        typeof(ActivityManager) },
        };

        public Dictionary<Type, Action> InstancesOfServices => new Dictionary<Type, Action>
        {
            { typeof(ChannelManager),           new ChannelManager(_manager).Start },
            { typeof(PublicationManager),       new PublicationManager(_manager).Start },
            { typeof(BrowserAccountManager),    new BrowserAccountManager(_manager).Start },
            { typeof(WalkerOnZen),              new WalkerOnZen(_manager).Start },
            { typeof(AccounRegister),           new AccounRegister(_manager).Start },
            { typeof(ActivityManager),          new ActivityManager(_manager).Start },
        };

        public static Action GetStartOfService(Type serviceType, DataManager manager)
        {
            var locator = new ServiceLocator(manager);
            locator.InstancesOfServices.TryGetValue(serviceType, out var service);
            return service;
        }
    }
}
