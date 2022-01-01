using System;
using System.Linq;
using System.Threading;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.InterfacesLibrary.ProjectModel;
using Yandex.Zen.Core;
using Yandex.Zen.Core.Enums;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using Yandex.Zen.Core.Services.ActivityManagerService;
using Yandex.Zen.Core.Services.PublicationManagerService;
using Yandex.Zen.Core.Services.WalkerOnZenService;
using Yandex.Zen.Core.Services.WalkerProfileService;
using Yandex.Zen.Core.Services.AccounRegisterService;
using Yandex.Zen.Core.Services.ChannelManagerService;
using Yandex.Zen.Core.Services.BrowserAccountManagerService;

namespace Yandex.Zen
{
    /// <summary>
    /// Класс для запуска выполнения скрипта
    /// </summary>
    public class Program : IZennoExternalCode
    {
        private static readonly object _locker = new object();

        [ThreadStatic] public static ProgramModeEnum _currentMode;

        /// <summary>
        /// Текущий режим работы шаблона.
        /// </summary>
        public static ProgramModeEnum CurrentMode { get => _currentMode; set => Logger.ConfigureModeLog(_currentMode = value); }

        /// <summary>
        /// Метод для запуска выполнения скрипта
        /// </summary>
        /// <param name="instance">Объект инстанса выделеный для данного скрипта</param>
        /// <param name="zenno">Объект проекта выделеный для данного скрипта</param>
        /// <returns>Код выполнения скрипта</returns>		
        public int Execute(Instance instance, IZennoPosterProjectModel zenno)
        {
            //DataKeeper.Configure(instance, zenno, out var configurationStatus);
            //if (configurationStatus is false) return 0;

            var manager = new DataManager_new(instance, zenno);

            try
            {
                switch (CurrentMode)
                {
                    case ProgramModeEnum.WalkerProfileService:
                        new MainWalkerProfile().Start();
                        break;

                    case ProgramModeEnum.AccounRegisterService:
                        new MainAccounRegister().Start();
                        break;

                    case ProgramModeEnum.ChannelManagerService:
                        new MainChannelManager().Start();
                        break;

                    case ProgramModeEnum.PublicationManagerService:
                        new MainPublicationManager().Start();
                        break;

                    case ProgramModeEnum.WalkerOnZenService:
                        new MainWalkerOnZen().Start();
                        break;

                    case ProgramModeEnum.BrowserAccountManagerService:
                        new MainBrowserAccountManager().Start();
                        break;

                    case ProgramModeEnum.ActivityManagerService:
                        new MainActivityManager().Start();
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Write(ex.FormatException(), LoggerType.Error, false, true, true, LogColor.Red);
            }
            CleanUpResourcesFromCache();
            return 0;
        }

        /// <summary>
        /// Добавление ресурса в списки занятости.
        /// </summary>
        public static void AddResourceToCache(string obj, bool addToResourcesCurrentThread, bool addToResourcesAllThreadsInWork)
        {
            if (addToResourcesCurrentThread) DataKeeper.ResourcesCurrentThread.Add(obj);
            if (addToResourcesAllThreadsInWork) DataKeeper.ResourcesAllThreadsInWork.Add(obj);
        }

        /// <summary>
        /// Очистка кэша проекта.
        /// Очистка ресурсов потока из общего списка.
        /// </summary>
        public static void CleanUpResourcesFromCache()
        {
            var curRes = DataKeeper.ResourcesCurrentThread;
            var allRes = DataKeeper.ResourcesAllThreadsInWork;

            if (curRes.Any())
            {
                lock (_locker)
                {
                    if (CurrentMode == ProgramModeEnum.BrowserAccountManagerService)
                        MainBrowserAccountManager.ThreadInWork = false;

                    curRes.ForEach(res
                        => allRes.RemoveAll(x => x == res));
                }
            }
        }

        /// <summary>
        /// Проверка ресурса на занятость другим потоком (аккаунт, донор, профиль).
        /// </summary>
        public static bool CheckResourceInWork(string resource)
            => DataKeeper.ResourcesAllThreadsInWork.Any(x => x.Equals(resource, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Сброс заданного количества выполнений и остановка скрипта, сохранить лог, бросить исклюение (IZennoPosterProjectModel).
        /// </summary>
        /// <param name="zenno"></param>
        /// <param name="textLog"></param>
        public static void StopTemplate(IZennoPosterProjectModel zenno, string textLog)
        {
            StopTemplate(zenno);
            Logger.Write(textLog, LoggerType.Warning, false, true, true, LogColor.Yellow);
        }

        /// <summary>
        /// Сброс заданного количества выполнений скрипта.
        /// </summary>
        /// <param name="zenno"></param>
        public static void ResetExecutionCounter(IZennoPosterProjectModel zenno)
        {
            lock (_locker)
            {
                ZennoPoster.SetTries(new Guid(zenno.TaskId), ZennoPoster.GetThreadsCount(new Guid(zenno.TaskId)));
                Thread.Sleep(2000);
            }
        }

        /// <summary>
        /// Сброс заданного количества выполнений и остановка скрипта.
        /// </summary>
        /// <param name="zenno"></param>
        public static void StopTemplate(IZennoPosterProjectModel zenno)
        {
            lock (_locker)
            {
                ZennoPoster.SetTries(new Guid(zenno.TaskId), 0);
                ZennoPoster.StopTask(new Guid(zenno.TaskId));
            }
        }

    }
}