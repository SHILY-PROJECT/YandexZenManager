using System;
using System.Collections.Generic;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.InterfacesLibrary.ProjectModel;
using Yandex.Zen.Core.Models;
using Yandex.Zen.Core.Interfaces;
using Yandex.Zen.Core.Toolkit;
using Yandex.Zen.Core.Toolkit.Extensions;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using Yandex.Zen.Core.Toolkit.SmsServiceTool;
using Yandex.Zen.Core.Toolkit.SmsServiceTool.Models;
using Yandex.Zen.Core.Toolkit.BrowserCustomizer;
using Yandex.Zen.Core.Toolkit.ResourceObject.Interfaces;
using Yandex.Zen.Core.Toolkit.ResourceObject.Models;
using Yandex.Zen.Core.Services.ChannelManagerService;
using Yandex.Zen.Core.Services.PublicationManagerService;
using Yandex.Zen.Core.Services.BrowserAccountManagerService;
using Yandex.Zen.Core.Services.WalkerOnZenService;
using Yandex.Zen.Core.Services.AccounRegisterService;
using Yandex.Zen.Core.Services.ActivityManagerService;

namespace Yandex.Zen
{
    public class DataManager : IDataManager
    {
        private static readonly Dictionary<string, Type> _serviceMapper = new Dictionary<string, Type>
        {
            { "Управление каналом",             typeof(ChannelManager) },
            { "Постинг",                        typeof(PublicationManager) },
            { "Ручное управление в браузере",   typeof(BrowserAccountManager) },
            { "Нагуливание по дзен",            typeof(WalkerOnZen) },
            { "Регистрация аккаунтов",          typeof(AccounRegister) },
            { "Накручивание активности",        typeof(ActivityManager) },
        };

        public DataManager(Instance instance, IZennoPosterProjectModel zenno)
        {
            Zenno = zenno;
            Browser = instance;
        }

        public IService Service { get; set; }
        public IZennoPosterProjectModel Zenno { get; private set; }
        public IResourceObject CurrentResourceObject { get; set; }
        public Type ServiceType { get; private set; }
        public Instance Browser { get; private set; }
        public TableData TableData { get; private set; }

        /// <summary>
        /// Конфигурация проекта.
        /// </summary>
        public bool TryConfigureProjectSettings()
        {
            ConfigureProjectSettings(out var configurationStatus);
            return configurationStatus;
        }

        /// <summary>
        /// Конфигурация проекта.
        /// </summary>
        public void ConfigureProjectSettings(out bool configurationStatus)
        {
            try
            {
                Configure();
                configurationStatus = true;
            }
            catch (Exception ex)
            {
                Logger.Write(ex.FormatException(), LoggerType.Error, false, false, true, LogColor.Red);
                configurationStatus = false;
            }
        }

        /// <summary>
        /// Конфигурация объекта для работы.
        /// </summary>
        private void Configure()
        {
            this.TableData = new TableData(Zenno, "AccountsShared", Zenno.Variables["cfgPathFileAccounts"]);
            SetService(Zenno.Variables["cfgTemplateMode"].Value);

            Logger.ConfigureGlobalLog(this);
            HE.ConfigureGlobalBrowse(this);

            this.SetBrowserSettings(Zenno.Variables["cfgInstanceWindowSize"].Value);
            this.Service.Configuration.Configure();

            // CaptchaService
            this.Service.ResourceObject.CaptchaService = new CaptchaService
            {
                ServiceDll = Zenno.Variables["cfgCaptchaServiceDll"].Value
            };

            // SmsService
            this.Service.ResourceObject.SmsService = new SmsService
            {
                ServiceParams = new SmsServiceParamsDataModel(Zenno.Variables["cfgSmsServiceAndCountry"].Value),
                WaitPhoneTimeOfSeconds = Zenno.Variables["cfgNumbAttempsGetPhone"].Value.ExtractNumber(),
                WaitSmsCodeOfMinutes = Zenno.Variables["cfgNumbMinutesWaitSmsCode"].Value.Split(' ')[0].ExtractNumber(),
                AttemptsReSendSmsCode = Zenno.Variables["cfgNumbAttemptsRequestSmsCode"].Value.Split(' ')[0].ExtractNumber()
            };

            // TemplateSettingsModel
            this.Service.ResourceObject.TemplateSettings = new TemplateSettingsModel
            {
                CreateFoldersAndFiles = bool.Parse(Zenno.Variables["cfgIfFolderErrorThenCreateIt"].Value),
                UseWalkedProfileFromSharedFolder = bool.Parse(Zenno.Variables["cfgUseWalkedProfileFromSharedFolder"].Value),
                MinProfileSizeToUse = int.Parse(Zenno.Variables["cfgMinSizeProfileUseInModes"].Value)
            };
        }

        private void SetService(string serviceKey)
        {
            if (_serviceMapper.TryGetValue(serviceKey, out var serviceType))
            {
                ServiceType = serviceType;
                Service = (IService)serviceType.GetConstructor(new[] { typeof(IDataManager) })?.Invoke(new[] { this });
            }
            else throw new InvalidOperationException("Service not defined.");
        }

        /// <summary>
        /// Установка настроек браузера.
        /// </summary>
        private void SetBrowserSettings(string instanceWindowSize)
        {
            Browser.SetWindowSize
            (
                int.Parse(instanceWindowSize.Split('x')[0]),
                int.Parse(instanceWindowSize.Split('x')[1])
            );

            Browser.ClearCache();
            Browser.ClearCookie();

            Browser.IgnoreAdditionalRequests = false;
            Browser.IgnoreAjaxRequests = false;
            Browser.IgnoreFrameRequests = false;
            Browser.IgnoreFlashRequests = true;
        }
    }
}
