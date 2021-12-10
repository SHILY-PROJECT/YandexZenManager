using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.InterfacesLibrary.SmsService.Enums;
using ZennoLab.InterfacesLibrary.Enums.Http;
using Global.ZennoExtensions;
using System.Threading;
using System.Text.RegularExpressions;
using System.IO;
using System.Runtime.InteropServices;
using Yandex.Zen.Core.Enums;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using Yandex.Zen.Core.Toolkit;
using Yandex.Zen.Core.Services.InstanceAccountManagementService.Enums;
using Yandex.Zen.Core.Toolkit.BrowserCustomizer;

namespace Yandex.Zen.Core.Services.InstanceAccountManagementService
{
    public class InstanceAccountManagement : ServicesDataAndComponents
    {
        private static readonly object _locker = new object();

        private readonly bool _launchIsAllowed;
        private readonly int _showInstanceOnWaitSeconds;
        private readonly List<string> _loginList;
        private readonly AccountProcessingModeEnum _accountProcessingMode;
        private readonly StartPageInstanceEnum _startPageInstance;

        public static bool ThreadInWork;

        /// <summary>
        /// Конструктор для скрипта (настройка лога, проверка и установка прочих данных).
        /// </summary>
        public InstanceAccountManagement()
        {
            AccountsTable = Zenno.Tables["AccountsShared"];

            // Ручное управление аккаунтом в инстансе - Конвертация типа режима обработки аккаунтов
            var statusAccountProcessingMode = new Dictionary<string, AccountProcessingModeEnum>()
            {
                {"Обрабатывать только первый логин", AccountProcessingModeEnum.FirstLoginOnly},
                {"Все логины по порядку", AccountProcessingModeEnum.AllLoginsInOrder}
            }
            .TryGetValue(Zenno.Variables["cfgManagingAnInstanceAccountProcessingMode"].Value, out _accountProcessingMode);

            if (!statusAccountProcessingMode)
            {
                Logger.Write("Не удалось определить режим обработки аккаунтов", LoggerType.Warning, false, true, true, LogColor.Yellow);
                return;
            }

            // Определение стартовой страницы аккаунта
            var statusGoodGetStartPage = new Dictionary<string, StartPageInstanceEnum>()
            {
                { "Поисковая система yandex", StartPageInstanceEnum.YandexSearchSystem },
                { "Дзен канал аккаунта", StartPageInstanceEnum.ZenChannelAccount },
                { "Редактор профиля дзен канала", StartPageInstanceEnum.ZenChannelProfileEditor },
                { "zen.yandex/media", StartPageInstanceEnum.ZenYandexMedia },
                { "zen.yandex/media/zen/login", StartPageInstanceEnum.ZenYandexMediaZenLogin },
                { "passport.yandex/profile", StartPageInstanceEnum.PassportYandexProfile }
            }
            .TryGetValue(Zenno.Variables["cfgStartPageForInstanceAccountManagement"].Value, out _startPageInstance);

            if (!statusGoodGetStartPage)
            {
                Logger.Write("Не удалось определить режим обработки аккаунтов", LoggerType.Warning, false, true, true, LogColor.Yellow);
                return;
            }

            // Определение максимального времени на обработку аккаунта
            var statusShowInstanceOnWaitSeconds = new Dictionary<string, int>
            {
                { "10 минут", 600 },
                { "20 минут", 1200 },
                { "30 минут", 1800 },
                { "40 минут", 2400 },
                { "50 минут", 3000 },
                { "1 час", 3600 },
                { "2 часа", 7200 },
                { "3 часа", 10800 },
                { "4 часа", 14400 },
                { "5 часов", 18000 },
                { "6 часов", 21600 },
                { "7 часов", 25200 },
                { "8 часов", 28800 },
                { "9 часов", 32400 },
                { "10 часов", 36000 }
            }
            .TryGetValue(Zenno.Variables["cfgShowInstanceOnWaitSeconds"].Value, out _showInstanceOnWaitSeconds);

            if (!statusShowInstanceOnWaitSeconds)
            {
                Logger.Write("Не удалось определить время обработки одного аккаунта в инстансе", LoggerType.Warning, false, true, true, LogColor.Yellow);
                return;
            }

            _loginList = Zenno.Variables["cfgLoginsFromSettings"].Value.Split(new[] { Environment.NewLine }, StringSplitOptions.None).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

            if (_loginList.Count == 0)
            {
                Logger.Write("В настройках не указано ни одного аккаунта для обработки", LoggerType.Warning, false, true, true, LogColor.Yellow);
                return;
            }

            // Доступ только для одного потока
            lock (_locker)
            {
                if (ThreadInWork)
                {
                    Logger.Write("Режим может работать только в один поток", LoggerType.Info, false, true, true, LogColor.Violet);
                    Program.ResetExecutionCounter(Zenno);
                    return;
                }
                else
                {
                    ProjectSettingsDataStore.ResourcesCurrentThread.Add("first_thread_in_work");
                    ThreadInWork = true;
                    _launchIsAllowed = true;
                }
            }
        }

        private void ShowOnTopUserAction(string uniqueTitle, int waitSec)
        {
            var url = string.Empty;

            switch (_startPageInstance)
            {
                case StartPageInstanceEnum.YandexSearchSystem:
                    url = $"https://yandex.{Domain}/";
                    break;
                case StartPageInstanceEnum.ZenChannelAccount:
                    url = ZenChannel;
                    break;
                case StartPageInstanceEnum.ZenChannelProfileEditor:
                    url = ZenChannelProfile;
                    break;
                case StartPageInstanceEnum.ZenYandexMedia:
                    url = $"https://zen.yandex.{Domain}/media";
                    break;
                case StartPageInstanceEnum.ZenYandexMediaZenLogin:
                    url = $"https://zen.yandex.{Domain}/media/zen/login";
                    break;
                case StartPageInstanceEnum.PassportYandexProfile:
                    url = $"https://passport.yandex.{Domain}/profile";
                    break;
            }

            Logger.Write($"Стартовая страница: {url}", LoggerType.Info, true, false, true);

            Instance.NavigateInNewTab(Login, url);
            Instance.AllTabs.ToList().ForEach(x => { if (x.Name != Login) x.Close(); });

            IntPtr hWnd = FindWindowByCaption(IntPtr.Zero, uniqueTitle);
            new Thread(() =>
            {
                Thread.Sleep(1000);
                SetForegroundWindow(hWnd);

                //Logger.LoggerWrite($"{Logger.LogStartsWith}Выполнение SetForeground для тайтла: {uniqueTitle}", LoggerType.Info, true,false, false);

                SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP.NOMOVE | SWP.NOSIZE);
            })
            .Start();

            Logger.Write("Запускается инстанс для ручного управления...", LoggerType.Info, true, false, true, LogColor.Blue);

            Instance.WaitForUserAction(waitSec);
            ProfileWorker.SaveProfile(true);

            Logger.Write("Обработка аккаунта в ручном режиме успешно завершена", LoggerType.Info, true, false, true, LogColor.Green);
        }

        /// <summary>
        /// Запуск скрипта.
        /// </summary>
        public void Start()
        {
            if (!_launchIsAllowed) return;

            // Ручная обработка аккаунтов в инстансе
            foreach (var login in _loginList)
            {
                if (!ResourceHandler(login))
                {
                    switch (_accountProcessingMode)
                    {
                        case AccountProcessingModeEnum.FirstLoginOnly: return;
                        case AccountProcessingModeEnum.AllLoginsInOrder: continue;
                    }
                }

                ShowOnTopUserAction(Instance.FormTitle, _showInstanceOnWaitSeconds);

                if (_accountProcessingMode == AccountProcessingModeEnum.FirstLoginOnly) break;

                Instance.ClearCookie();
                Instance.ClearCache();
            }
        }

        /// <summary>
        /// Получение, обработка и установка данных перед запуском.
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        private bool ResourceHandler(string login)
        {
            Login = default;
            ObjectDirectory = default;
            ZenChannel = default;
            ZenChannelProfile = default;
            Proxy = default;

            lock (SyncObjects.TableSyncer)
            {
                var accountsCount = AccountsGeneralTable.RowCount;

                for (int row = 0; row < accountsCount; row++)
                {
                    var loginFromTabel = AccountsGeneralTable.GetCell((int)TableColumnEnum.Inst.Login, row);

                    if (login == loginFromTabel)
                    {
                        Login = login;
                        ObjectDirectory = new DirectoryInfo($@"{Zenno.Directory}\Accounts\{Login}");

                        Logger.SetCurrentObjectForLogText(Login, ResourceTypeEnum.Account);

                        // Проверка наличия zen канала
                        if (_startPageInstance == StartPageInstanceEnum.ZenChannelAccount || _startPageInstance == StartPageInstanceEnum.ZenChannelProfileEditor)
                        {
                            ZenChannel = AccountsGeneralTable.GetCell((int)TableColumnEnum.Inst.ZenChannel, row);

                            if (string.IsNullOrWhiteSpace(ZenChannel))
                            {
                                Logger.Write($"Отсутствует zen канал", LoggerType.Info, true, true, true, LogColor.Yellow);
                                return false;
                            }
                        }

                        // Получение и загрузка профиля
                        if (!ProfileWorker.LoadProfile(true)) return false;

                        // Проверка директории на существование (создать, если требуется)
                        if (!ResourceDirectoryExists()) return false;

                        // Получение прокси
                        if (!SetProxy((int)TableColumnEnum.Inst.Proxy, row, true)) continue;

                        // Преобразование ссылки дзен канала в ссылку профиля канала
                        if (_startPageInstance == StartPageInstanceEnum.ZenChannelAccount || _startPageInstance == StartPageInstanceEnum.ZenChannelProfileEditor)
                        {
                            ZenChannelProfile = Regex.Match(ZenChannel, @"(?<=zen\.yandex\.[a-z]+/id/).*$").Value;

                            if (string.IsNullOrWhiteSpace(ZenChannelProfile))
                            {
                                Logger.Write($"Не удалось преобразовать ссылку \"ZenChannel\" в \"ZenChannelProfile\", возможно, ссылка на канал заполнена некорректно", LoggerType.Info, true, true, true, LogColor.Yellow);
                                return false;
                            }
                            else ZenChannelProfile = $"https://zen.yandex.{Domain}/profile/editor/id/{ZenChannelProfile}";
                        }

                        // Успешное получение ресурса
                        //Program.ListAccountAndDonorInWork.Add(Login);
                        Logger.Write($"[Proxy table: {Proxy} | proxy country: {IpInfo.CountryFullName} | {IpInfo.CountryShortName}]\t[Row: {row + 2}]\tАккаунт успешно найден и подключен", LoggerType.Info, true, false, true);
                        return true;
                    }
                }
            }

            // Не удалось получить ресурс
            Logger.Write($"[Login: {login}]\tВ общей таблице отсутствует аккаунт", LoggerType.Info, false, true, true, LogColor.Yellow);
            return false;
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int uFlags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        private static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

        private static IntPtr HWND_TOPMOST { get; set; } = new IntPtr(-1);

        private static class SWP
        {
            public static readonly int
            NOSIZE = 0x0001,
            NOMOVE = 0x0002,
            NOZORDER = 0x0004,
            NOREDRAW = 0x0008,
            NOACTIVATE = 0x0010,
            DRAWFRAME = 0x0020,
            FRAMECHANGED = 0x0020,
            SHOWWINDOW = 0x0040,
            HIDEWINDOW = 0x0080,
            NOCOPYBITS = 0x0100,
            NOOWNERZORDER = 0x0200,
            NOREPOSITION = 0x0200,
            NOSENDCHANGING = 0x0400,
            DEFERERASE = 0x2000,
            ASYNCWINDOWPOS = 0x4000;
        }

    }
}
