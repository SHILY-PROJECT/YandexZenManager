using System;
using System.IO;
using System.Collections.Generic;
using Yandex.Zen.Core.Enums;
using Yandex.Zen.Core.Models.ResourceModels;
using Yandex.Zen.Core.Services.Models;
using Yandex.Zen.Core.Toolkit;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using Yandex.Zen.Core.Toolkit.PhoneServiceTool;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.InterfacesLibrary.ProjectModel;
using Yandex.Zen.Core.Toolkit.PhoneServiceTool.Models;
using Yandex.Zen.Core.Services.PostingSecondWindService;
using Yandex.Zen.Core.Services.PostingSecondWindService.Enums;
using Yandex.Zen.Core.Services.PostingSecondWindService.Models;
using Yandex.Zen.Core.Services.InstanceAccountManagementService;

namespace Yandex.Zen
{
    public class ProjectDataStore
    {
        private static readonly object _locker = new object();

        #region=========================================================================
        [ThreadStatic] private static IZennoPosterProjectModel _zenno;
        [ThreadStatic] private static Instance _browser;
        [ThreadStatic] private static ResourceBaseModel _resourceBaseModel;
        [ThreadStatic] private static ProgramModeEnum _programMode;
        [ThreadStatic] private static TableModel _mainTable;
        [ThreadStatic] private static TableModel _modeTable;
        [ThreadStatic] private static PhoneServiceNew _phoneService;
        [ThreadStatic] private static CaptchaServiceNew _captchaService;
        [ThreadStatic] private static List<string> _resourcesCurrentThread = new List<string>();
        private static List<string> _resourcesAllThreadsInWork = new List<string>();
        private static string _instanceWindowSize;
        private static DirectoryInfo _profilesDirectory;
        private static DirectoryInfo _accountsDirectory;
        #endregion======================================================================

        /// <summary>
        /// Объект типа аккаунта или донора с соответствующими данными.
        /// </summary>
        public static ResourceBaseModel ResourceObject { get => _resourceBaseModel; }
        
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
        public static DirectoryInfo AccountsDirectory { get { if (!_accountsDirectory.Exists) _accountsDirectory.Create(); return _accountsDirectory; } }

        /// <summary>
        /// Общая директория со всеми профилями.
        /// </summary>
        public static DirectoryInfo ProfilesDirectory { get { if (!_profilesDirectory.Exists) _accountsDirectory.Create(); return _accountsDirectory; } }
        
        /// <summary>
        /// Режим работы скрипта (программы).
        /// </summary>
        public static ProgramModeEnum ProgramMode { get => _programMode; }

        /// <summary>
        /// SMS сервис (автоматическое заполнение свойств данных при конфигурации проекта).
        /// </summary>
        public static PhoneService PhoneService { get; private set; }

        /// <summary>
        /// SMS сервис (автоматическое заполнение свойств данных при конфигурации проекта).
        /// </summary>
        public static PhoneServiceNew PhoneServiceNew { get => _phoneService; }

        /*todo Снести CaptchaServiceDll после рефакторинга*/
        public static string CaptchaServiceDll { get; private set; }
        /// <summary>
        /// Капча сервис (данные dll).
        /// </summary>
        public static CaptchaServiceNew CaptchaService { get => _captchaService; }

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
                _zenno = zenno;
                InitializingProjectData();

                _browser = instance;
                SetBrowserSettings();
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
        private static void InitializingProjectData()
        {
            _profilesDirectory = new DirectoryInfo($@"{Zenno.Directory}\profiles");
            _accountsDirectory = new DirectoryInfo($@"{Zenno.Directory}\accounts");

            _instanceWindowSize = Zenno.Variables["cfgInstanceWindowSize"].Value;

            // режим работы шаблона
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

            // установка таблиц
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

            // установка сервисов капчи и телефонных номеров
            _captchaService = new CaptchaServiceNew(Zenno.Variables["cfgCaptchaServiceDll"]);
            _phoneService = new PhoneServiceNew(Zenno.Variables["cfgSmsServiceAndCountry"], new PhoneSettingsModel
            (
                Zenno.Variables["cfgNumbAttempsGetPhone"],
                Zenno.Variables["cfgNumbMinutesWaitSmsCode"],
                Zenno.Variables["cfgNumbAttemptsRequestSmsCode"]
            ));

            switch (ProgramMode)
            {
                case ProgramModeEnum.PostingSecondWind:
                    PostingSecondWind.CurrentMode = new Dictionary<string, PostingSecondWindModeEnum>
                    {
                        ["Авторизация и привязка номера"] = PostingSecondWindModeEnum.AuthorizationAndLinkPhone,
                        ["Постинг"] =                       PostingSecondWindModeEnum.Posting
                    }
                    [Zenno.Variables["cfgPostingSecondWindModeOfOperation"].Value];
                    break;

                default: throw new Exception($"'{ProgramMode}' - на текущий момент режим отключен");
            }


            var profile = new ProfileDataModel()
            {
                UseWalkedProfileFromSharedFolder = bool.Parse(Zenno.Variables["cfgUseWalkedProfileFromSharedFolder"].Value),
                MinProfileSizeToUse = int.Parse(Zenno.Variables["cfgMinSizeProfileUseInModes"].Value)
            };
            var settings = new ResourceSettingsFromZennoVariablesModel()
            {
                CreateFolderResourceIfNoExist = bool.Parse(Zenno.Variables["cfgIfFolderErrorThenCreateIt"].Value)
            };
            _resourceBaseModel = new ResourceBaseModel
            {
                SettingsFromZennoVariables = settings,
                Profile = profile
            };
            _resourceBaseModel.SetAccount();
        }

        /// <summary>
        /// Установка настроек браузера.
        /// </summary>
        private void SetBrowserSettings()
        {
            _browser.SetWindowSize
            (
                int.Parse(_instanceWindowSize.Split('x')[0]),
                int.Parse(_instanceWindowSize.Split('x')[1])
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
