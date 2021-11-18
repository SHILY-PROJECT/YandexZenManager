using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Resources;
using System.Text;
using System.Threading;
using Yandex.Zen.Core;
using Yandex.Zen.Core.Enums;
using Yandex.Zen.Core.Models.AccountOrDonorModels;
using Yandex.Zen.Core.Services;
using Yandex.Zen.Core.Services.Models;
using Yandex.Zen.Core.Tools;
using Yandex.Zen.Core.Tools.Extensions;
using Yandex.Zen.Core.Tools.LoggerTool;
using Yandex.Zen.Core.Tools.LoggerTool.Enums;
using Yandex.Zen.Core.Tools.PhoneServiceTool;
using ZennoLab.CommandCenter;
using ZennoLab.Emulation;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.InterfacesLibrary.ProjectModel;
using ZennoLab.InterfacesLibrary.ProjectModel.Collections;
using ZennoLab.InterfacesLibrary.ProjectModel.Enums;
using Yandex.Zen.Core.Tools.PhoneServiceTool.Models;

namespace Yandex.Zen
{
    public class DataStore
    {
        private static readonly object _locker = new object();

        [ThreadStatic] private static IZennoPosterProjectModel _zenno;
        [ThreadStatic] private static Instance _browser;
        [ThreadStatic] private static AccountOrDonorBaseModel _accountOrDonorBaseModel;
        [ThreadStatic] private static ProgramModeEnum _programMode;
        [ThreadStatic] private static TableModel _mainTable;
        [ThreadStatic] private static TableModel _modeTable;
        [ThreadStatic] private static List<string> _currentObjectCache;
        private static DirectoryInfo _profilesDirectory;
        private static DirectoryInfo _accountsDirectory;
        private static List<string> _objectsOfAllThreadsInWork;
        private static string _instanceWindowSize;


        public static AccountOrDonorBaseModel ResourceObject { get => _accountOrDonorBaseModel; private set { _accountOrDonorBaseModel = value; } }
        
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
            get => _accountsDirectory is null ? _accountsDirectory = new DirectoryInfo($@"{Zenno.Directory}\Accounts") : _accountsDirectory;
        }

        /// <summary>
        /// Общая директория со всеми профилями.
        /// </summary>
        public static DirectoryInfo ProfilesDirectory { get => _profilesDirectory is null ? _profilesDirectory = new DirectoryInfo($@"{Zenno.Directory}\profiles") : _profilesDirectory; }
        
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
        public static PhoneServiceNew PhoneServiceNew { get; private set; }

        /*todo Снести CaptchaServiceDll после рефакторинга*/
        public static string CaptchaServiceDll { get; private set; }
        /// <summary>
        /// Капча сервис (данные dll).
        /// </summary>
        public static CaptchaServiceNew CaptchaServiceDllNew { get; private set; }

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
        public static IZennoPosterProjectModel Zenno
        {
            get => _zenno;
            private set { _zenno = value; }
        }

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
                Zenno = zenno;
                InitializingProjectProperties();
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
        private static void InitializingProjectProperties()
        {
            #region [ ИНИЦИАЛИЗАЦИЯ СВОЙСТВ ПРОЕКТА ]==========================================
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

            /* todo После переноса сервисов на улучшенную архитектуру - снести данный объект и его создание*/
            if (CaptchaServiceDll is null)
                lock (_locker) CaptchaServiceDll = CaptchaServiceDll is null ? CaptchaServiceDll = Zenno.Variables["cfgCaptchaServiceDll"].Value : CaptchaServiceDll;
            if (PhoneService is null)
                lock (_locker) PhoneService = PhoneService is null ? PhoneService = new PhoneService(Zenno.Variables["cfgSmsServiceAndCountry"].Value) : PhoneService;
           
            #endregion

            #region [ ИНИЦИАЛИЗАЦИЯ СВОЙСТВ СЕРВИСОВ ]==========================================
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

            CaptchaServiceDllNew = new CaptchaServiceNew(Zenno.Variables["cfgCaptchaServiceDll"]);
            PhoneServiceNew = new PhoneServiceNew(Zenno.Variables["cfgSmsServiceAndCountry"], new PhoneSettingsModel
            (
                Zenno.Variables["cfgNumbAttempsGetPhone"],
                Zenno.Variables["cfgNumbMinutesWaitSmsCode"],
                Zenno.Variables["cfgNumbAttemptsRequestSmsCode"]
            ));
            
            
            ServicesComponents.MinSizeProfileUseInModes = Zenno.Variables["cfgMinSizeProfileUseInModes"].Value.ExtractNumber();
            ServicesComponents.CreateFolderResourceIfNotExist = bool.Parse(Zenno.Variables["cfgIfFolderErrorThenCreateIt"].Value);
            #endregion

            ResourceObject = new AccountOrDonorBaseModel(new SettingsUseProfileSharedModel
            (
                Zenno.Variables["cfgUseWalkedProfileFromSharedFolder"],
                Zenno.Variables["cfgMinSizeProfileUseInModes"]
            ));
        }

    }
}
