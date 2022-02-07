using System;
using System.Collections.Generic;
using Yandex.Zen.Core.Interfaces;
using Yandex.Zen.Core.Toolkit;
using Yandex.Zen.Core.Toolkit.SmsServiceTool;
using Yandex.Zen.Core.Services.AccounRegisterService;
using Yandex.Zen.Core.Services.ActivityManagerService;
using Yandex.Zen.Core.Services.BrowserAccountManagerService;
using Yandex.Zen.Core.Services.ChannelManagerService;
using Yandex.Zen.Core.Services.PublicationManagerService;
using Yandex.Zen.Core.Services.WalkerOnZenService;
using Yandex.Zen.Core.ServicesComponents.ResourceObject.Models;
using Yandex.Zen.Core.ServicesComponents.ResourceObject.Interfaces;
using Yandex.Zen.Core.ServicesComponents.Configurations;

namespace Yandex.Zen.Core.ServicesComponents
{
    public class ServiceConfigurator
    {
        private static readonly Dictionary<Type, Func<IDataManager, IResourceObject>> _mapperConfiguration = new Dictionary<Type, Func<IDataManager, IResourceObject>>
        {
            { typeof(AccounRegister), AccounRegisterConfiguration.Configure },
            { typeof(ActivityManager), ActivityManagerConfiguration.Configure },
            { typeof(BrowserAccountManager), BrowserAccountManagerConfiguration.Configure },
            { typeof(ChannelManager), ChannelManagerConfiguration.Configure },
            { typeof(PublicationManager), PublicationManagerConfiguration.Configure },
            { typeof(WalkerOnZen), WalkerOnZenConfiguration.Configure }
        };

        public static void Configure(IDataManager manager, TemplateSettingsModel templateSettings, CaptchaService captchaService, SmsService smsService)
        {
            var res = _mapperConfiguration[manager.CurrentServiceType]?.Invoke(manager);

            res.CaptchaService = captchaService;
            res.SmsService = smsService;
            

        }
    }
}
