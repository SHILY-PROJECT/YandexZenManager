using System;
using System.IO;
using System.Collections.Generic;
using ZennoLab.CommandCenter;
using Yandex.Zen.Core.Enums;
using Yandex.Zen.Core.Models.ResourceModels;
using Yandex.Zen.Core.Services.Models;
using Yandex.Zen.Core.Toolkit;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using Yandex.Zen.Core.Toolkit.SmsServiceTool;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.InterfacesLibrary.ProjectModel;
using Yandex.Zen.Core.Toolkit.SmsServiceTool.Models;
using Yandex.Zen.Core.Services.PostingSecondWindService;
using Yandex.Zen.Core.Services.PostingSecondWindService.Enums;
using Yandex.Zen.Core.Toolkit.Extensions;

namespace Yandex.Zen
{
    public class ProjectKeeper
    {
        [ThreadStatic] private static IZennoPosterProjectModel _zenno;
        [ThreadStatic] private static Instance _browser;
        [ThreadStatic] private static ResourceBaseModel _resourceBaseModel;
        [ThreadStatic] private static ProgramModeEnum _programMode;
        [ThreadStatic] private static TableModel _mainTable;
        [ThreadStatic] private static TableModel _modeTable;
        [ThreadStatic] private static List<string> _resourcesCurrentThread = new List<string>();

        private static readonly List<string> _resourcesAllThreadsInWork = new List<string>();
        private static DirectoryInfo _profilesDirectory;
        private static DirectoryInfo _accountsDirectory;

        /// <summary>
        /// SMS сервис (автоматическое заполнение свойств данных при конфигурации проекта).
        /// </summary>
        [Obsolete] public static PhoneService PhoneService { get; private set; }

        /// <summary>
        /// Объект типа аккаунта или донора с соответствующими данными.
        /// </summary>
        public static ResourceBaseModel Resource { get => _resourceBaseModel; }
        
        /// <summary>
        /// Общая таблица с аккаунтами.
        /// </summary>
        public static TableModel MainTable { get => _mainTable; }

        /// <summary>
        /// Таблица текущего режима.
        /// </summary>
        public static TableModel ModeTable { get => _modeTable; }

        /// <summary>
        /// Общая директория со всеми аккаунтами.
        /// </summary>
        public static DirectoryInfo AccountsDirectory
        {
            get
            {
                if (_accountsDirectory.Exists is false)
                    _accountsDirectory.Create();
                return _accountsDirectory;
            }
        }

        /// <summary>
        /// Общая директория со всеми профилями.
        /// </summary>
        public static DirectoryInfo ProfilesDirectory
        { 
            get
            { 
                if (_profilesDirectory.Exists is false)
                    _accountsDirectory.Create();
                return _accountsDirectory;
            }
        }
        
        /// <summary>
        /// Режим работы скрипта (программы).
        /// </summary>
        public static ProgramModeEnum CurrentProgramMode { get => _programMode; }

        /// <summary>
        /// Текущие объекты потока.
        /// </summary>
        public static List<string> ResourcesCurrentThread { get => _resourcesCurrentThread; }

        /// <summary>
        /// Все объекты всех потоков, которые сейчас в работе.
        /// </summary>
        public static List<string> ResourcesAllThreadsInWork { get => _resourcesAllThreadsInWork; }

        /// <summary>
        /// Объект зенно постера (project).
        /// </summary>
        public static IZennoPosterProjectModel Zenno { get => _zenno; }

        /// <summary>
        /// Объект зенно постера (браузер).
        /// </summary>
        public static Instance Browser { get => _browser; }

        /// <summary>
        /// Настройка проекта перед работой.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="zenno"></param>
        public void ConfigureProject(Instance instance, IZennoPosterProjectModel zenno, out bool configurationStatus)
        {
            configurationStatus = true;

            try
            {
                _browser = instance;
                _zenno = zenno;
                InitializingProjectData();
            }
            catch (Exception ex)
            {
                Logger.Write($"[{nameof(ex.Message)}:{ex.Message}]{Environment.NewLine}{nameof(ex.StackTrace)}:{ex.StackTrace}{Environment.NewLine}", LoggerType.Error, false, false, true, LogColor.Red);
                configurationStatus = false;
            }
        }

        /// <summary>
        /// Инициализация свойств проекта.
        /// </summary>
        /// <returns></returns>
        private void InitializingProjectData()
        {
            SetBrowserSettings(Zenno.Variables["cfgInstanceWindowSize"].Value);

            _profilesDirectory = new DirectoryInfo($@"{Zenno.Directory}\profiles");
            _accountsDirectory = new DirectoryInfo($@"{Zenno.Directory}\accounts");

            // Режим работы шаблона
            _programMode = new Dictionary<string, ProgramModeEnum>()
            {
                ["Ручное управление аккаунтом в инстансе"] =        ProgramModeEnum.InstanceAccountManagement,
                ["Нагуливание профилей"] =                          ProgramModeEnum.WalkingProfile,
                ["Нагуливание аккаунтов/доноров по zen.yandex"] =   ProgramModeEnum.WalkingOnZen,
                ["Регистрация аккаунтов yandex"] =                  ProgramModeEnum.YandexAccountRegistration,
                ["Создание и оформление канала zen.yandex"] =       ProgramModeEnum.ZenChannelCreationAndDesign,
                ["Публикация статей на канале zen.yandex"] =        ProgramModeEnum.ZenArticlePublication,
                ["Накрутка активности"] =                           ProgramModeEnum.CheatActivity,
                ["Posting - second wind (new theme)"] =             ProgramModeEnum.PostingSecondWind
            }
            [Zenno.Variables["cfgScriptServices"].Value];

            // Установка таблиц
            _mainTable = new TableModel("AccountsShared", Zenno.Variables["cfgPathFileAccounts"]);
            _modeTable = new Dictionary<ProgramModeEnum, TableModel>
            {
                [ProgramModeEnum.InstanceAccountManagement] =   new TableModel("AccountsShared", Zenno.Variables["cfgPathFileAccounts"]),
                [ProgramModeEnum.YandexAccountRegistration] =   new TableModel("DonorsForRegistration", Zenno.Variables["cfgPathFileDonorsForRegistration"]),
                [ProgramModeEnum.ZenChannelCreationAndDesign] = new TableModel("AccountsForCreateZenChannel", Zenno.Variables["cfgPathFileAccountsForCreateZenChannel"]),
                [ProgramModeEnum.ZenArticlePublication] =       new TableModel("AccountsForPosting", Zenno.Variables["cfgPathFileAccountsForPosting"]),
                [ProgramModeEnum.CheatActivity] =               new TableModel("AccountsForCheatActivity", Zenno.Variables["cfgAccountsForCheatActivity"]),
                [ProgramModeEnum.PostingSecondWind] =           new TableModel("AccountsPostingSecondWind", Zenno.Variables["cfgPathFileAccountsPostingSecondWind"])
            }
            [_programMode];

            // Настройка режимов работы сервисов
            switch (CurrentProgramMode)
            {
                case ProgramModeEnum.PostingSecondWind:
                    PostingSecondWind.CurrentMode = new Dictionary<string, PostingSecondWindModeEnum>
                    {
                        ["Авторизация и привязка номера"] = PostingSecondWindModeEnum.AuthorizationAndLinkPhone,
                        ["Постинг"] =                       PostingSecondWindModeEnum.Posting
                    }
                    [Zenno.Variables["cfgPostingSecondWindModeOfOperation"].Value];
                    break;

                default: throw new Exception($"'{CurrentProgramMode}' - на текущий момент режим отключен");
            }

            // Установка сервисов капчи и телефонных номеров
            var captchaService = new CaptchaService
            {
                ServiceDll = Zenno.Variables["cfgCaptchaServiceDll"].Value
            };
            var smsService = new SmsService(
                new SmsServiceSettingsModel(
                    Zenno.Variables["cfgNumbAttempsGetPhone"].Value.Split(' ')[0].ExtractNumber(),
                    Zenno.Variables["cfgNumbMinutesWaitSmsCode"].Value.ExtractNumber(),
                    Zenno.Variables["cfgNumbAttemptsRequestSmsCode"].Value.Split(' ')[0].ExtractNumber()),
                new SmsServiceParamsDataModel(Zenno.Variables["cfgSmsServiceAndCountry"].Value));
            var profile = new ProfileDataModel()
            {
                UseWalkedProfileFromSharedFolder = bool.Parse(Zenno.Variables["cfgUseWalkedProfileFromSharedFolder"].Value),
                MinProfileSizeToUse = int.Parse(Zenno.Variables["cfgMinSizeProfileUseInModes"].Value)
            };

            var resourceSettings = new ResourceSettingsModel()
            {
                CreateFolderResourceIfNoExist = bool.Parse(Zenno.Variables["cfgIfFolderErrorThenCreateIt"].Value)
            };
            _resourceBaseModel = new ResourceBaseModel
            {
                CaptchaService = captchaService,
                SmsService = smsService,
                ActionSettings  = resourceSettings,
                ProfileData = profile
            };
            _resourceBaseModel.SetAccount();
        }

        /// <summary>
        /// Установка настроек браузера.
        /// </summary>
        private void SetBrowserSettings(string instanceWindowSize)
        {
            _browser.SetWindowSize
            (
                int.Parse(instanceWindowSize.Split('x')[0]),
                int.Parse(instanceWindowSize.Split('x')[1])
            );

            _browser.ClearCache();
            _browser.ClearCookie();

            _browser.IgnoreAdditionalRequests = false;
            _browser.IgnoreAjaxRequests = false;
            _browser.IgnoreFrameRequests = false;
            _browser.IgnoreFlashRequests = true;
        }

    }
}
