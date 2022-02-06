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
        delegate void ResourceObjectConfiguration(DataManager manager, IService service);

        private readonly IService _service;
        private readonly DataManager _manager;
        private readonly TemplateSettingsModel _templateSettings;
        private readonly CaptchaService _captchaService;
        private readonly SmsService _smsService;

        public ServiceConfigurator(IService service, DataManager manager, TemplateSettingsModel templateSettings, CaptchaService captchaService, SmsService smsService)
        {
            _service = service;
            _manager = manager;
            _templateSettings = templateSettings;
            _captchaService = captchaService;
            _smsService = smsService;
        }

        private Dictionary<Type, ResourceObjectConfiguration> _mapperConfiguration = new Dictionary<Type, ResourceObjectConfiguration>
        {
            { typeof(AccounRegister), AccounRegisterConfiguration.Configure },
            { typeof(ActivityManager), ActivityManagerConfiguration.Configure },
            { typeof(BrowserAccountManager), BrowserAccountManagerConfiguration.Configure },
            { typeof(ChannelManager), ChannelManagerConfiguration.Configure },
            { typeof(PublicationManager), PublicationManagerConfiguration.Configure },
            { typeof(WalkerOnZen), WalkerOnZenConfiguration.Configure }
        };

        public IResourceObject Configure()
        {
            _mapperConfiguration[Program.CurrentService].Invoke(_manager, _service);
            
            switch (_service)
            {
                case AccounRegister accounRegister:

                    break;

                case ActivityManager activityManager:

                    break;

                case BrowserAccountManager browserAccountManager:

                    break;

                case ChannelManager channelManager:

                    break;

                case PublicationManager publicationManager:

                    break;

                case WalkerOnZen walkerOnZen:

                    break;

                default: throw new InvalidOperationException();
            }

            //switch (serviceType)
            //{
            //    case Type _ when serviceType == typeof(AccounRegister):

            //        break;

            //    case Type _ when serviceType == typeof(ActivityManager):

            //        break;

            //    case Type _ when serviceType == typeof(BrowserAccountManager):

            //        break;

            //    case Type _ when serviceType == typeof(ChannelManager):

            //        break;

            //    case Type _ when serviceType == typeof(PublicationManager):

            //        break;

            //    case Type _ when serviceType == typeof(WalkerOnZen):

            //        break;
            //}

            throw new NotImplementedException();
        }
    }
}
