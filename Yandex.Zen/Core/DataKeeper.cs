using System;
using System.IO;
using System.Collections.Generic;
using ZennoLab.CommandCenter;
using Yandex.Zen.Core.Enums;
using Yandex.Zen.Core.Toolkit;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.InterfacesLibrary.ProjectModel;
using Yandex.Zen.Core.Models;
using Yandex.Zen.Core.ServiceModules.ObjectModule;

namespace Yandex.Zen.Core
{
    /// <summary>
    /// Класс для хранения, инициализации данных и конфигурации проекта.
    /// </summary>
    public class DataKeeper
    {
        [ThreadStatic] private static IZennoPosterProjectModel _zenno;
        [ThreadStatic] private static Instance _browser;
        [ThreadStatic] private static ObjectBase _resourceBaseModel;
        [ThreadStatic] private static ProgramModeEnum _programMode;
        [ThreadStatic] private static TableModel _mainTable;
        [ThreadStatic] private static TableModel _modeTable;
        [ThreadStatic] private static List<string> _resourcesCurrentThread = new List<string>();
        private static readonly List<string> _resourcesAllThreadsInWork = new List<string>();
        private static DirectoryInfo _sharedDirectoryOfProfiles;
        private static DirectoryInfo _sharedDirectoryOfAccounts;

        /// <summary>
        /// SMS сервис (автоматическое заполнение свойств данных при конфигурации проекта).
        /// </summary>
        [Obsolete] public static Obsolete_SmsService PhoneService { get; private set; }

        /// <summary>
        /// Объект типа аккаунта или донора с соответствующими данными.
        /// </summary>
        public static ObjectBase Resource
        {
            get
            {
                if (_resourceBaseModel is null)
                    throw new Exception($"'{Resource}' - object cannot is null");
                return _resourceBaseModel;
            }
        }

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
        public static DirectoryInfo SharedDirectoryOfAccounts
        {
            get
            {
                var dir = _sharedDirectoryOfAccounts ?? (_sharedDirectoryOfAccounts = new DirectoryInfo($@"{Zenno.Directory}\accounts"));
                if (dir.Exists is false) dir.Create();
                return dir;
            }
        }

        /// <summary>
        /// Общая директория со всеми профилями.
        /// </summary>
        public static DirectoryInfo SharedDirectoryOfProfiles
        {
            get
            {
                var dir = _sharedDirectoryOfProfiles ?? (_sharedDirectoryOfProfiles = new DirectoryInfo($@"{Zenno.Directory}\profiles"));
                if (dir.Exists is false) dir.Create();
                return dir;
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
        public static void Configure(Instance instance, IZennoPosterProjectModel zenno, out bool configurationStatus)
        {
            try
            {
                _browser = instance;
                _zenno = zenno;
                InitializationDataAndConfiguration();
                configurationStatus = true;
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
        private static void InitializationDataAndConfiguration()
        {
            SetBrowserSettings(Zenno.Variables["cfgInstanceWindowSize"].Value);

            
        }

        /// <summary>
        /// Установка настроек браузера.
        /// </summary>
        private static void SetBrowserSettings(string instanceWindowSize)
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
