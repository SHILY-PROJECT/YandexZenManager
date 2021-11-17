﻿using System;
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
    /// <summary>
    /// Класс для запуска выполнения скрипта
    /// </summary>
    public class Program : DataStore, IZennoExternalCode
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
            _ = new DataStore(instance, zenno);

            var objectBase = new ObjectBaseModel();

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
                    case ProgramModeEnum.PostingSecondWind:             new PostingSecondWind(objectBase).Start();  break;
                }
            }
            catch (Exception ex)
            {
                Logger.Write($"[Exception message:{ex.Message}]{Environment.NewLine}Exception stack trace:{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}", LoggerType.Error, false, true, true, LogColor.Red);
            }

            // Очистка ресурсов
            if (CurrentObjectCache.Count != 0)
            {
                lock (_locker)
                {
                    if (ProgramMode == ProgramModeEnum.InstanceAccountManagement)
                        InstanceAccountManagement.ThreadInWork = false;
                    CurrentObjectCache.ForEach(res => CurrentObjectsOfAllThreadsInWork.RemoveAll(x => x == res));
                }
            }

            return 0;
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