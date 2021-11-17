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
using Yandex.Zen.Core.Models.ObjectModels;
using Yandex.Zen.Core.Services;
using Yandex.Zen.Core.Services.Models;
using Yandex.Zen.Core.Tools;
using Yandex.Zen.Core.Tools.Extensions;
using Yandex.Zen.Core.Tools.LoggerTool;
using Yandex.Zen.Core.Tools.LoggerTool.Enums;
using ZennoLab.CommandCenter;
using ZennoLab.Emulation;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.InterfacesLibrary.ProjectModel;
using ZennoLab.InterfacesLibrary.ProjectModel.Enums;

namespace Yandex.Zen
{
    public class DataStore
    {
        public DataStore(){ }

        public DataStore(Instance instance, IZennoPosterProjectModel zenno)
        {
            Zenno = zenno;
            Instance = instance;

            try
            {
                InitializingProjectProperties();
            }
            catch (Exception ex)
            {
                Logger.Write($"[Exception message:{ex.Message}]{Environment.NewLine}Exception stack trace:{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}", LoggerType.Error, false, false, true, LogColor.Red);
            }
        }

        private static readonly object _locker = new object();

        [ThreadStatic] private static IZennoPosterProjectModel _zenno;
        [ThreadStatic] private static Instance _instance;
        [ThreadStatic] private static ProgramModeEnum _programMode;
        [ThreadStatic] private static TableModel _mainTable;
        [ThreadStatic] private static TableModel _modeTable;
        [ThreadStatic] private static List<string> _currentObjectCache;

        private static DirectoryInfo _profilesDirectory;
        private static DirectoryInfo _accountsDirectory;
        private static List<string> _objectsOfAllThreadsInWork;
        private static string _instanceWindowSize;

        public static TableModel MainTable { get => _mainTable; }
        public static TableModel ModeTable { get => _modeTable; }
        public static DirectoryInfo AccountsDirectory { get => _accountsDirectory is null ? _accountsDirectory = new DirectoryInfo($@"{ServicesComponents.Zenno.Directory}\Accounts") : _accountsDirectory; }
        public static DirectoryInfo ProfilesDirectory { get => _profilesDirectory is null ? _profilesDirectory = new DirectoryInfo($@"{Zenno.Directory}\profiles") : _profilesDirectory; }
        public static ProgramModeEnum ProgramMode { get => _programMode; }
        public static PhoneService PhoneService { get; set; }
        public static string CaptchaServiceDll { get; set; }

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
        public static Instance Instance
        {
            get => _instance;
            private set
            {
                _instance = value;
                _instance.SetWindowSize
                (
                    int.Parse(_instanceWindowSize.Split('x')[0]),
                    int.Parse(_instanceWindowSize.Split('x')[1])
                );
                _instance.ClearCache();
                _instance.ClearCookie();
                _instance.IgnoreAdditionalRequests = false;
                _instance.IgnoreAjaxRequests = false;
                _instance.IgnoreFrameRequests = false;
                _instance.IgnoreFlashRequests = true;
            }
        }

        /// <summary>
        /// Инициализация свойств проекта.
        /// </summary>
        /// <returns></returns>
        private static void InitializingProjectProperties()
        {
            #region [ ИНИЦИАЛИЗАЦИЯ СВОЙСТВ PROGRAM ]==========================================
            _instanceWindowSize = Zenno.Variables["cfgInstanceWindowSize"].Value;

            _programMode = new Dictionary<string, ProgramModeEnum>()
            {
                ["Ручное управление аккаунтом в инстансе"] = ProgramModeEnum.InstanceAccountManagement,
                ["Нагуливание профилей"] = ProgramModeEnum.WalkingProfile,
                ["Нагуливание аккаунтов/доноров по zen.yandex"] = ProgramModeEnum.WalkingOnZen,
                ["Регистрация аккаунтов yandex"] = ProgramModeEnum.YandexAccountRegistration,
                ["Создание и оформление канала zen.yandex"] = ProgramModeEnum.ZenChannelCreationAndDesign,
                ["Публикация статей на канале zen.yandex"] = ProgramModeEnum.ZenArticlePublication,
                ["Накрутка активности"] = ProgramModeEnum.CheatActivity,
                ["Posting - second wind (new theme)"] = ProgramModeEnum.PostingSecondWind
            }
            [Zenno.Variables["cfgScriptServices"].Value];

            if (CaptchaServiceDll is null)
                lock (_locker) CaptchaServiceDll = CaptchaServiceDll is null ? CaptchaServiceDll = Zenno.Variables["cfgCaptchaServiceDll"].Value : CaptchaServiceDll;

            if (PhoneService is null)
                lock (_locker) PhoneService = PhoneService is null ? PhoneService = new PhoneService(Zenno.Variables["cfgSmsServiceAndCountry"].Value) : PhoneService;
            #endregion

            #region [ ИНИЦИАЛИЗАЦИЯ СВОЙСТВ СЕРВИСОВ ]==========================================
            _mainTable = new TableModel(Zenno.Tables["AccountsShared"], Zenno.Variables["cfgPathFileAccounts"]);
            _modeTable = new Dictionary<ProgramModeEnum, TableModel>
            {
                [ProgramModeEnum.InstanceAccountManagement] = new TableModel(Zenno.Tables["AccountsShared"], Zenno.Variables["cfgPathFileAccounts"]),
                [ProgramModeEnum.YandexAccountRegistration] = new TableModel(Zenno.Tables["DonorsForRegistration"], Zenno.Variables["cfgPathFileDonorsForRegistration"]),
                [ProgramModeEnum.ZenChannelCreationAndDesign] = new TableModel(Zenno.Tables["AccountsForCreateZenChannel"], Zenno.Variables["cfgPathFileAccountsForCreateZenChannel"]),
                [ProgramModeEnum.ZenArticlePublication] = new TableModel(Zenno.Tables["AccountsForPosting"], Zenno.Variables["cfgPathFileAccountsForPosting"]),
                [ProgramModeEnum.CheatActivity] = new TableModel(Zenno.Tables["AccountsForCheatActivity"], Zenno.Variables["cfgAccountsForCheatActivity"]),
                [ProgramModeEnum.PostingSecondWind] = new TableModel(Zenno.Tables["AccountsPostingSecondWind"], Zenno.Variables["cfgPathFileAccountsPostingSecondWind"])
            }
            [_programMode];

            if (ServicesComponents.AccountsGeneralTable is null)
                lock (_locker) ServicesComponents.AccountsGeneralTable = ServicesComponents.AccountsGeneralTable is null ? ServicesComponents.AccountsGeneralTable = Zenno.Tables["AccountsShared"] : ServicesComponents.AccountsGeneralTable;

            ServicesComponents.TimeToSecondsWaitPhone = Zenno.Variables["cfgNumbAttempsGetPhone"].Value.ExtractNumber();
            ServicesComponents.MinutesWaitSmsCode = Zenno.Variables["cfgNumbMinutesWaitSmsCode"].Value.Split(' ')[0].ExtractNumber();
            ServicesComponents.AttemptsReSendSmsCode = Zenno.Variables["cfgNumbAttemptsRequestSmsCode"].Value.Split(' ')[0].ExtractNumber();
            ServicesComponents.MinSizeProfileUseInModes = Zenno.Variables["cfgMinSizeProfileUseInModes"].Value.ExtractNumber();
            ServicesComponents.CreateFolderResourceIfNotExist = bool.Parse(Zenno.Variables["cfgIfFolderErrorThenCreateIt"].Value);
            #endregion
        }

    }
}
