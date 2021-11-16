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
using Yandex.Zen.Core.Enums;
using Yandex.Zen.Core.Enums.Logger;
using Yandex.Zen.Core.Services;
using Yandex.Zen.Core.ServicesCommonComponents;
using Yandex.Zen.Core.Tools;
using Yandex.Zen.Core.Tools.Extensions;
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
    public class Program : ServiceComponents, IZennoExternalCode
    {
        private static readonly object _locker = new object();

        [ThreadStatic] private static IZennoPosterProjectModel _zenno;
        [ThreadStatic] private static Instance _instance;

        private static DirectoryInfo _profilesDirectory;
        private static DirectoryInfo _accountsDirectory;

        public static DirectoryInfo AccountsDirectory
        {
            get => _accountsDirectory is null ? new DirectoryInfo($@"{zenno.Directory}\Accounts") : _accountsDirectory;
            private set { _accountsDirectory = value; }
        }

        public static DirectoryInfo ProfilesDirectory
        {
            get => _profilesDirectory is null ? new DirectoryInfo($@"{Zenno.Directory}\profiles") : _profilesDirectory;
            private set { _profilesDirectory = value; }
        }

        public static IZennoPosterProjectModel Zenno
        { 
            get => _zenno;
            private set { _zenno = value; }
        }

        public static Instance Instance
        {
            get => _instance;
            private set { _instance = value; }
        }

        [ThreadStatic] public static ProgramModeEnum ProgramMode;
        [ThreadStatic] public static List<string> ResourcesMode;

        public static List<string> ResourcesAllThreadsInWork { get; set; }
        public static PhoneService PhoneService { get; set; }
        public static string CaptchaServiceDll { get; set; }

        /// <summary>
        /// Метод для запуска выполнения скрипта
        /// </summary>
        /// <param name="instance">Объект инстанса выделеный для данного скрипта</param>
        /// <param name="zenno">Объект проекта выделеный для данного скрипта</param>
        /// <returns>Код выполнения скрипта</returns>		
        public int Execute(Instance instance, IZennoPosterProjectModel zenno)
        {
            Zenno = zenno;
            Instance = instance;

            if (Logger.GeneralLog == null)
                lock (_locker)
                    if (Logger.GeneralLog == null) Logger.SetGeneralLog();


            if (ResourcesAllThreadsInWork == null)
                lock (_locker)
                    if (ResourcesAllThreadsInWork == null) ResourcesAllThreadsInWork = new List<string>();

            if (AccountsGeneralTable == null)
                lock (_locker)
                    if (AccountsGeneralTable == null) AccountsGeneralTable = ServiceComponents.zenno.Tables["AccountsShared"];

            if (rnd == null)
                lock (_locker)
                    if (rnd == null) rnd = new Random();

            //if (DirectoryProfilesInfo == null)
            //    lock (_locker)
            //        if (DirectoryProfilesInfo == null) DirectoryProfilesInfo = new DirectoryInfo($@"{ServiceComponents.zenno.Directory}\profiles");

            if (CaptchaServiceDll == null)
                lock (_locker)
                    if (CaptchaServiceDll == null) CaptchaServiceDll = ServiceComponents.zenno.Variables["cfgCaptchaServiceDll"].Value;

            if (PhoneService == null)
                lock (_locker)
                    if (PhoneService == null) PhoneService = new PhoneService(ServiceComponents.zenno.Variables["cfgSmsServiceAndCountry"].Value);

            // Режим работы скрипта - Конвертация режима работы скрипта
            var statusConvertMode = new Dictionary<string, ProgramModeEnum>()
            {
                ["Нагуливание профилей"] = ProgramModeEnum.WalkingProfile,
                ["Нагуливание аккаунтов/доноров по zen.yandex"] = ProgramModeEnum.WalkingOnZen,
                ["Регистрация аккаунтов yandex"] = ProgramModeEnum.YandexAccountRegistration,
                ["Создание и оформление канала zen.yandex"] = ProgramModeEnum.ZenChannelCreationAndDesign,
                ["Публикация статей на канале zen.yandex"] = ProgramModeEnum.ZenArticlePublication,
                ["Ручное управление аккаунтом в инстансе"] = ProgramModeEnum.InstanceAccountManagement,
                ["Накрутка активности"] = ProgramModeEnum.CheatActivity,
                ["Posting - second wind (new theme)"] = ProgramModeEnum.PostingSecondWind
            }
            .TryGetValue(ServiceComponents.zenno.Variables["cfgScriptServices"].Value, out ProgramMode);

            if (!statusConvertMode)
            {
                StopTemplate(ServiceComponents.zenno);
                Logger.Write($"Не удалось определить режим работы скрипта: {ServiceComponents.zenno.Variables["cfgScriptServices"].Value}", LoggerType.Warning, false, false, true, LogColor.Yellow);
                return 0;
            }

            // Установка настроек для работы с sms сервисом
            TimeToSecondsWaitPhone = ServiceComponents.zenno.Variables["cfgNumbAttempsGetPhone"].Value.ExtractNumber();
            MinutesWaitSmsCode = ServiceComponents.zenno.Variables["cfgNumbMinutesWaitSmsCode"].Value.Split(' ')[0].ExtractNumber();
            AttemptsReSendSmsCode = ServiceComponents.zenno.Variables["cfgNumbAttemptsRequestSmsCode"].Value.Split(' ')[0].ExtractNumber();

            // Минимальный размер профиля использовать в режимах
            MinSizeProfileUseInModes = ServiceComponents.zenno.Variables["cfgMinSizeProfileUseInModes"].Value.ExtractNumber();

            // Создание директорий и файлов для ресурсов, если их не существует
            CreateFolderResourceIfNotExist = bool.Parse(ServiceComponents.zenno.Variables["cfgIfFolderErrorThenCreateIt"].Value);

            // Задаем размер инстанса
            var instanceWindowSize = ServiceComponents.zenno.Variables["cfgInstanceWindowSize"].Value;
            ServiceComponents.instance.SetWindowSize(int.Parse(instanceWindowSize.Split('x')[0]), int.Parse(instanceWindowSize.Split('x')[1]));

            // Чистка куков и кэша перед работой
            ServiceComponents.instance.ClearCache();
            ServiceComponents.instance.ClearCookie();

            // Политика игнорирования
            ServiceComponents.instance.IgnoreAdditionalRequests = false;
            ServiceComponents.instance.IgnoreAjaxRequests = false;
            ServiceComponents.instance.IgnoreFrameRequests = false;
            ServiceComponents.instance.IgnoreFlashRequests = true;

            // Старт заданного скрипта
            try
            {
                switch (ProgramMode)
                {
                    case ProgramModeEnum.WalkingProfile:
                        new WalkingProfile().Start();
                        break;
                    case ProgramModeEnum.YandexAccountRegistration:
                        new YandexAccountRegistration().Start();
                        break;
                    case ProgramModeEnum.ZenChannelCreationAndDesign:
                        new ZenChannelCreationAndDesign().Start();
                        break;
                    case ProgramModeEnum.ZenArticlePublication:
                        new ZenArticlePublication().Start();
                        break;
                    case ProgramModeEnum.WalkingOnZen:
                        new WalkingOnZen().Start();
                        break;
                    case ProgramModeEnum.InstanceAccountManagement:
                        new InstanceAccountManagement().Start();
                        break;
                    case ProgramModeEnum.CheatActivity:
                        new CheatActivity().Start();
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Write($"[Exception message:{ex.Message}]{Environment.NewLine}Exception stack trace:{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}", LoggerType.Error, false, true, true, LogColor.Red);
            }

            // Очистка ресурсов
            if (ResourcesMode.Count != 0)
            {
                lock (_locker)
                {
                    if (ProgramMode == ProgramModeEnum.InstanceAccountManagement)
                        InstanceAccountManagement.ThreadInWork = false;

                    ResourcesMode.ForEach(res => ResourcesAllThreadsInWork.RemoveAll(x => x == res));
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