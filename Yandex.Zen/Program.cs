using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.InterfacesLibrary.ProjectModel;
using Yandex.Zen.Core;
using Yandex.Zen.Core.Enums;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using Yandex.Zen.Core.Services.BrowserAccountManagerService;
using Yandex.Zen.Core.Services;
using Yandex.Zen.Core.Toolkit;
using Yandex.Zen.Core.Interfaces;
using Yandex.Zen.Core.ServicesComponents;

namespace Yandex.Zen
{
    /// <summary>
    /// Класс для запуска выполнения скрипта
    /// </summary>
    public class Program : IZennoExternalCode
    {
        private static readonly object _locker = new object();
        private static List<string> _objectsAllThreadsInWork;

        [ThreadStatic] private static IDataManager _manager;
        [ThreadStatic] private static List<string> _objectsCurrentThread;
        [ThreadStatic] private static DirectoryInfo _commonAccountDirectory;
        [ThreadStatic] private static DirectoryInfo _commonProfileDirectory;

        /// <summary>
        /// Текущие объекты потока.
        /// </summary>
        public static List<string> ObjectsCurrentThread { get => _objectsCurrentThread ?? (_objectsCurrentThread = new List<string>()); }

        /// <summary>
        /// Все объекты всех потоков, которые сейчас в работе.
        /// </summary>
        public static List<string> ObjectsAllThreadsInWork { get => _objectsAllThreadsInWork ?? (_objectsAllThreadsInWork = new List<string>()); }

        /// <summary>
        /// Общая директория со всеми аккаунтами.
        /// </summary>
        public static DirectoryInfo CommonAccountDirectory
        {
            get
            {
                if (!_commonAccountDirectory.Exists)
                    _commonAccountDirectory.Create();
                return _commonAccountDirectory;
            }
        }

        /// <summary>
        /// Общая директория со всеми профилями.
        /// </summary>
        public static DirectoryInfo CommonProfileDirectory
        {
            get
            {
                if (!_commonProfileDirectory.Exists)
                    _commonProfileDirectory.Create();
                return _commonProfileDirectory;
            }
        }

        /// <summary>
        /// Текущий сервис (текущий режим работы шаблона).
        /// </summary>
        public static Type CurrentServiceType { get => _manager.CurrentServiceType; }

        /// <summary>
        /// Инстанс сервиса.
        /// </summary>
        public static IService Service { get => _manager.Service; }

        /// <summary>
        /// Метод для запуска выполнения скрипта
        /// </summary>
        /// <param name="instance">Объект инстанса выделеный для данного скрипта</param>
        /// <param name="zenno">Объект проекта выделеный для данного скрипта</param>
        /// <returns>Код выполнения скрипта</returns>		
        public int Execute(Instance instance, IZennoPosterProjectModel zenno)
        {
            var manager = _manager = new DataManager(instance, zenno);

            if (manager.TryConfigureProjectSettings())
            {
                try
                {
                    _ = _commonAccountDirectory ?? (_commonAccountDirectory = new DirectoryInfo($@"{manager.Zenno.Directory}\accounts"));
                    _ = _commonProfileDirectory ?? (_commonProfileDirectory = new DirectoryInfo($@"{manager.Zenno.Directory}\profiles"));

                    ServiceLocator.GetStartOfService(CurrentServiceType, manager)?.Invoke();
                }
                catch (Exception ex)
                {
                    Logger.Write(ex.FormatException(), LoggerType.Error, false, true, true, LogColor.Red);
                }
                finally
                {
                    CleanUpObjectsFromCache();
                }
            }

            return 0;
        }

        /// <summary>
        /// Добавление ресурса в списки занятости.
        /// </summary>
        public static void AddObjectToCache(string obj, bool addToResourcesCurrentThread, bool addToResourcesAllThreadsInWork)
        {
            if (addToResourcesCurrentThread) ObjectsCurrentThread.Add(obj);
            if (addToResourcesAllThreadsInWork) ObjectsAllThreadsInWork.Add(obj);
        }

        /// <summary>
        /// Очистка кэша проекта.
        /// Очистка ресурсов потока из общего списка.
        /// </summary>
        public static void CleanUpObjectsFromCache()
        {
            if (ObjectsCurrentThread.Any() is false) return;

            lock (_locker)
            {
                if (Service != null && Service is BrowserAccountManager)
                {
                    BrowserAccountManager.IsInProcess = false;
                }

                ObjectsCurrentThread.ForEach(res
                    => ObjectsAllThreadsInWork.RemoveAll(x => x == res));
            }
        }

        /// <summary>
        /// Проверка ресурса на занятость другим потоком (аккаунт, донор, профиль).
        /// </summary>
        public static bool CheckObjectInWork(string resource)
            => ObjectsAllThreadsInWork.Any(x => x.Equals(resource, StringComparison.OrdinalIgnoreCase));

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