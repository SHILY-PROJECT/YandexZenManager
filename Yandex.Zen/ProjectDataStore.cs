using System;
using System.IO;
using System.Collections.Generic;
using Yandex.Zen.Core.Enums;
using Yandex.Zen.Core.Models.AccountOrDonorModels;
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
        [ThreadStatic] private static AccountOrDonorBaseModel _accountOrDonorBaseModel;
        [ThreadStatic] private static ProgramModeEnum _programMode;
        [ThreadStatic] private static TableModel _mainTable;
        [ThreadStatic] private static TableModel _modeTable;
        [ThreadStatic] private static List<string> _currentObjectCache;
        [ThreadStatic] private static PhoneServiceNew _phoneService;
        [ThreadStatic] private static CaptchaServiceNew _captchaService;
        private static readonly DirectoryInfo _profilesDirectory = new DirectoryInfo($@"{Zenno.Directory}\profiles");
        private static readonly DirectoryInfo _accountsDirectory = new DirectoryInfo($@"{Zenno.Directory}\accounts");
        private static List<string> _objectsOfAllThreadsInWork;
        private static string _instanceWindowSize;
        #endregion======================================================================

        /// <summary>
        /// Объект типа аккаунта или донора с соответствующими данными.
        /// </summary>
        public static AccountOrDonorBaseModel ResourceObject { get => _accountOrDonorBaseModel; }
        
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
        public static List<string> CurrentObjectCache
        {
            get
            {
                if (_currentObjectCache is null)
                    lock (_locker) _currentObjectCache = _currentObjectCache is null ? _currentObjectCache = new List<string>() : _currentObjectCache;
                return _currentObjectCache;
            }
        }

        /// <summary>
        /// Все объекты всех потоков, которые сейчас в работе.
        /// </summary>
        public static List<string> CurrentObjectsOfAllThreadsInWork
        {
            get
            {
                if (_objectsOfAllThreadsInWork is null)
                    lock (_locker) _objectsOfAllThreadsInWork = _objectsOfAllThreadsInWork is null ? _objectsOfAllThreadsInWork = new List<string>() : _objectsOfAllThreadsInWork;
                return _objectsOfAllThreadsInWork;
            }
        }

        /// <summary>
        /// Объект зенно постера (project).
        /// </summary>
        public static IZennoPosterProjectModel Zenno { get => _zenno; }

        /// <summary>
        /// Объект зенно постера (браузер).
        /// </summary>
        public static Instance Browser
        {
            get => _browser;
            private set
            {
                _browser = value;
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
                Browser = instance;
            }
            catch (Exception ex)
            {
                Logger.Write($"[{nameof(ex.Message)}:{ex.Message}]{Environment.NewLine}{nameof(ex.StackTrace)}:{ex.StackTrace}{Environment.NewLine}", LoggerType.Error, false, false, true, LogColor.Red);
                configurationStatus = false;
            }
        }

        /// <summary>
        /// Очистка кэша проекта.
        /// Очистка ресурсов потока из общего списка.
        /// </summary>
        public void ClearProjectCache()
        {
            if (CurrentObjectCache.Count != 0)
            {
                lock (_locker)
                {
                    if (ProgramMode == ProgramModeEnum.InstanceAccountManagement)
                        InstanceAccountManagement.ThreadInWork = false;
                    CurrentObjectCache.ForEach(res => CurrentObjectsOfAllThreadsInWork.RemoveAll(x => x == res));
                }
            }
        }

        /// <summary>
        /// Инициализация свойств проекта.
        /// </summary>
        /// <returns></returns>
        private static void InitializingProjectData()
        {
            _instanceWindowSize = Zenno.Variables["cfgInstanceWindowSize"].Value;
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
                    var postingSecondWindMode = new Dictionary<string, PostingSecondWindModeEnum>
                    {
                        ["Авторизация и привязка номера"] = PostingSecondWindModeEnum.AuthorizationAndLinkPhone,
                        ["Постинг"] =                       PostingSecondWindModeEnum.Posting
                    }
                    [Zenno.Variables["cfgPostingSecondWindModeOfOperation"].Value];
                    PostingSecondWind.SetSettings(new PostingSecondWindSettings(postingSecondWindMode));
                    break;

                default: throw new Exception($"'{ProgramMode}' - на текущий момент режим отключен");
            }


            _accountOrDonorBaseModel = new AccountOrDonorBaseModel(
                new SettingsAccountOrDonorFromZennoVariablesModel
                (                   
                    Zenno.Variables["cfgIfFolderErrorThenCreateIt"]
                ),
                new ProfileDataModel
                (
                    Zenno.Variables["cfgUseWalkedProfileFromSharedFolder"],
                    Zenno.Variables["cfgMinSizeProfileUseInModes"]
                ));            
        }

    }
}
