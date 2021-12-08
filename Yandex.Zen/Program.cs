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
using Yandex.Zen.Core.Services.CheatActivityService;
using Yandex.Zen.Core.Services.InstanceAccountManagementService;
using Yandex.Zen.Core.Services.Models;
using Yandex.Zen.Core.Services.PostingSecondWindService;
using Yandex.Zen.Core.Services.WalkingOnZenService;
using Yandex.Zen.Core.Services.WalkingProfileService;
using Yandex.Zen.Core.Services.YandexAccountRegistrationService;
using Yandex.Zen.Core.Services.ZenArticlePublicationService;
using Yandex.Zen.Core.Services.ZenChannelCreationAndDesignService;
using Yandex.Zen.Core.Toolkit;
using Yandex.Zen.Core.Toolkit.BrowserCustomizer;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using ZennoLab.CommandCenter;
using ZennoLab.Emulation;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.InterfacesLibrary.ProjectModel;
using ZennoLab.InterfacesLibrary.ProjectModel.Enums;

namespace Yandex.Zen
{
    /// <summary>
    /// Класс для запуска выполнения скрипта
    /// </summary>
    public class Program : ProjectDataStore, IZennoExternalCode
    {
        private static readonly object _locker = new object();

        /// <summary>
        /// Метод для запуска выполнения скрипта
        /// </summary>
        /// <param name="instance">Объект инстанса выделеный для данного скрипта</param>
        /// <param name="zenno">Объект проекта выделеный для данного скрипта</param>
        /// <returns>Код выполнения скрипта</returns>		
        public int Execute(Instance instance, IZennoPosterProjectModel zenno)
        {
            ConfigureProject(instance, zenno, out var configurationStatus);
            if (configurationStatus is false) return 0;

            try
            {
                switch (ProgramMode)
                {
                    case ProgramModeEnum.WalkingProfile:                new WalkingProfile().Start();               break;
                    case ProgramModeEnum.YandexAccountRegistration:     new YandexAccountRegistration().Start();    break;
                    case ProgramModeEnum.ZenChannelCreationAndDesign:   new ZenChannelCreationAndDesign().Start();  break;
                    case ProgramModeEnum.ZenArticlePublication:         new ZenArticlePublication().Start();        break;
                    case ProgramModeEnum.WalkingOnZen:                  new WalkingOnZen().Start();                 break;
                    case ProgramModeEnum.InstanceAccountManagement:     new InstanceAccountManagement().Start();    break;
                    case ProgramModeEnum.CheatActivity:                 new CheatActivity().Start();                break;
                    case ProgramModeEnum.PostingSecondWind:             new PostingSecondWind().Start();            break;
                }
            }
            catch (Exception ex)
            {
                Logger.Write($"[Exception message:{ex.Message}]{Environment.NewLine}Exception stack trace:{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}", LoggerType.Error, false, true, true, LogColor.Red);
            }
            CleanUpResourcesFromCache();
            return 0;
        }

        /// <summary>
        /// Добавление ресурса в списки занятости.
        /// </summary>
        public static void AddResourcesToCache(string obj, bool addToResourcesCurrentThread, bool addToResourcesAllThreadsInWork)
        {
            if (addToResourcesCurrentThread) ResourcesCurrentThread.Add(obj);
            if (addToResourcesAllThreadsInWork) ResourcesAllThreadsInWork.Add(obj);
        }

        /// <summary>
        /// Очистка кэша проекта.
        /// Очистка ресурсов потока из общего списка.
        /// </summary>
        public void CleanUpResourcesFromCache()
        {
            if (ResourcesCurrentThread.Any())
            {
                lock (_locker)
                {
                    if (ProgramMode == ProgramModeEnum.InstanceAccountManagement)
                        InstanceAccountManagement.ThreadInWork = false;
                    ResourcesCurrentThread.ForEach(res => ResourcesAllThreadsInWork.RemoveAll(x => x == res));
                }
            }
        }

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