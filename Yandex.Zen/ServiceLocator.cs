using System;
using System.Linq;
using System.Collections.Generic;
using Yandex.Zen.Core.Interfaces;
using Yandex.Zen.Core.Services.AccounRegisterService;
using Yandex.Zen.Core.Services.ActivityManagerService;
using Yandex.Zen.Core.Services.BrowserAccountManagerService;
using Yandex.Zen.Core.Services.ChannelManagerService;
using Yandex.Zen.Core.Services.PublicationManagerService;
using Yandex.Zen.Core.Services.WalkerOnZenService;
using Yandex.Zen.Core.ServicesConfigurations;

namespace Yandex.Zen
{
    public class ServiceLocator
    {
        private readonly IDataManager _manager;

        public ServiceLocator(IDataManager manager)
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
            { typeof(AccounRegister),           new AccounRegister(_manager, new AccounRegisterServiceConfiguration(_manager)).Start },
            { typeof(ActivityManager),          new ActivityManager(_manager, new ActivityManagerServiceConfiguration(_manager)).Start },
            { typeof(BrowserAccountManager),    new BrowserAccountManager(_manager, new BrowserAccountManagerServiceConfiguration(_manager)).Start },
            { typeof(ChannelManager),           new ChannelManager(_manager, new ChannelManagerServiceConfiguration(_manager)).Start },
            { typeof(PublicationManager),       new PublicationManager(_manager, new PublicationManagerServiceConfiguration(_manager)).Start },
            { typeof(WalkerOnZen),              new WalkerOnZen(_manager, new WalkerOnZenServiceConfiguration(_manager)).Start },
        };

        public static Action GetStartOfService(Type serviceType, IDataManager manager)
        {
            var loc = new ServiceLocator(manager);
            loc.InstancesOfServices.TryGetValue(serviceType, out var service);
            manager.Service = service.GetInvocationList().First().Target as IService;
            return service;
        }
    }
}
