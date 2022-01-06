using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.Enums.Log;
using Yandex.Zen.Core.Toolkit.Extensions;
using Yandex.Zen.Core.Models.TableHandler;
using Yandex.Zen.Core.Enums;
using Yandex.Zen.Core.Toolkit;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using Yandex.Zen.Core.Toolkit.LoggerTool.Models;
using Yandex.Zen.Core.Toolkit.BrowserCustomizer;
using Yandex.Zen.Core.Toolkit.BrowserCustomizer.Enums;
using Yandex.Zen.Core.Services.ChannelManagerService.Enums;
using Yandex.Zen.Core.Services.ChannelManagerService.Models.ChannelSettings.DataModels;
using Yandex.Zen.Core.Toolkit.TableTool.Enums;
using Yandex.Zen.Core.Services.WalkerOnZenService;

namespace Yandex.Zen.Core.Services.ChannelManagerService
{
    [Obsolete]
    public class MainChannelManager_obsolete : ServicesDataAndComponents_obsolete
    {
        private static readonly object _locker = new object();

        [ThreadStatic]
        public static FileInfo SettingsFile = new FileInfo(Path.Combine(ObjectDirectory.FullName, "_logger", "config_setted_channel_settings.ini"));

        private readonly StartPageCreateChannelEnum _startPageCreateChannelZen;

        private readonly bool _launchIsAllowed;
        private readonly bool _goWalkingZenBeforeCreateChannel;
        private readonly bool _skipWalkingOnZenIfWalked;
        private readonly bool _modifyButtonYandexZenMediaIfProxyUsa;
        private readonly bool _changesWereChannelSettings;
        private bool _channelAlreadyCreated;


        /// <summary>
        /// Конструктор для использования данного класса из других классов.
        /// </summary>
        /// <param name="createAnEmptyConstructor"></param>
        public MainChannelManager_obsolete(bool createAnEmptyConstructor) => SetSettingsMode();

        /// <summary>
        /// Конструктор для скрипта (настройка лога, проверка и установка прочих данных).
        /// </summary>
        public MainChannelManager_obsolete()
        {
            AccountsTable = Zenno.Tables["AccountsForCreateZenChannel"];

            if (AccountsTable.RowCount == 0)
            {
                Program.StopTemplate(Zenno, $"Таблица с аккаунтами/донорами пуста");
                return;
            }

            SetSettingsMode();

            switch (Zenno.Variables["cfgStartPageCreateChannelZen"].Value)
            {
                case "Создание канала через zen.yandex/media":
                    _startPageCreateChannelZen = StartPageCreateChannelEnum.ZenYandexMedia;
                    break;
                case "Создание канала через zen.yandex/media/zen/login":
                    _startPageCreateChannelZen = StartPageCreateChannelEnum.ZenYandexZenMediaLogin;
                    break;
                default:
                    Logger.Write($"Не удалось определить вариант селектора у функции \"Создание канала через\"", LoggerType.Warning, false, true, true, LogColor.Yellow);
                    return;
            }

            TableGeneralAccountFile = new FileInfo(Zenno.ExecuteMacro(Zenno.Variables["cfgPathFileAccounts"].Value));
            TableModeAccountFile = new FileInfo(Zenno.ExecuteMacro(Zenno.Variables["cfgPathFileAccountsForCreateZenChannel"].Value));
            TableGeneralAndTableModeIsSame = TableGeneralAccountFile.FullName.ToLower() == TableModeAccountFile.FullName.ToLower();

            BindingPhoneToAccountIfRequaid = Zenno.Variables["cfgBindingPhoneIfRequiredForCreateZenChannel"].Value.Contains("Привязывать номер");
            _skipWalkingOnZenIfWalked = Zenno.Variables["cfgSkipWalkingOnZenForCreateZenChannel"].Value.Contains("Пропустить гуляне по zen.yandex, если уже гулял");
            _goWalkingZenBeforeCreateChannel = Zenno.Variables["cfgWalkToZenBeforeCreateChannel"].Value.Contains("Перед регой идти погулять на zen.yandex");
            _modifyButtonYandexZenMediaIfProxyUsa = bool.Parse(Zenno.Variables["cfgModifyYandexZenMediaIfProxyUsa"].Value);

            lock (_locker) _launchIsAllowed = ResourceHandler();
        }

        /// <summary>
        /// Установка настроек для режима (перменных).
        /// </summary>
        private void SetSettingsMode()
        {

        }

        /// <summary>
        /// Запуск скрипта.
        /// </summary>
        public void Start(List<ChannelSettingsEnum> channelSettings) => ExecuteSettingsAndDesign(channelSettings);

        /// <summary>
        /// Запуск скрипта.
        /// </summary>
        public void Start()
        {
            if (!_launchIsAllowed) return;

            // Создать канал
            if (!_channelAlreadyCreated)
                _channelAlreadyCreated = CreateChannel();

            if (_channelAlreadyCreated)
            {

            }
        }

        /// <summary>
        /// Создание дзен канала.
        /// </summary>
        /// <returns></returns>
        private bool CreateChannel()
        {
            // Идти гулять на zen перед созданием канала (если канал не создан)
            if (!_channelAlreadyCreated && _goWalkingZenBeforeCreateChannel)
            {
                var numbOfWalks = Logger.GetAccountLog(LogFilter.WalkingUnixtime).Count;

                if (numbOfWalks == 0 || numbOfWalks != 0 && !_skipWalkingOnZenIfWalked)
                {
                    Logger.Write($"[Действия перед регистрацией]\tПереход на \"zen.yandex\" перед регистрацией для прогулки", LoggerType.Info, true, false, true);

                    new MainWalkerOnZen_obsolete(ObjectTypeEnum.Account).Start();

                    if (!MainWalkerOnZen_obsolete.StatusWalkIsGood) return false;
                }
            }

            var xpathButtonStartNow = new[] { "//div[contains(@class, 'notification')]/descendant::button[contains(@class, 'action-button')]", "Кнопка - начать прямо сейчас" };
            var xpathHrefZen = new[] { "//div[contains(@class, 'buttons')]/descendant::a[contains(@href, '/media/zen/login')]", "Ссылка на страницу авторизации zen" };
            var xpathChildButtonCreateZen = new[] { ".//span[contains(@class, 'button')]", "Завести блог в дзене" };
            var xpathLoginFormAuth = new[] { "//input[@name='login']", "Поле - Логин (авторизация)" };
            var xpathSettingsChannel = new[] { "//div[contains(@class, 'channel-block')]/descendant::button[contains(@class, 'channel-block') and not(contains(@class, 'type_chat'))]", "Кнопка - Настройки канала" };
            var xpathAuthAccountList = new[] { "//div[contains(@class, 'passp-auth')]/descendant::div[contains(@class, 'passp-auth-content')]/descendant::div[contains(@class, 'AuthAccountList') and contains(@data-t, 'account') and not(contains(@data-t, 'item'))]", "Форма - Выберите учетную запись (список аккаунтов)" };

            // Получаем url стартовой страницы
            new Dictionary<StartPageCreateChannelEnum, string>()
            {
                { StartPageCreateChannelEnum.ZenYandexMedia, $"https://zen.yandex.{Domain}/media" },
                { StartPageCreateChannelEnum.ZenYandexZenMediaLogin, $"https://zen.yandex.{Domain}/media/zen/login" }
            }
            .TryGetValue(_startPageCreateChannelZen, out string url);

            var counter = default(int);
            var startPageIsOpen = default(bool);

            Logger.Write($"Переход к стартовой странице: {url}", LoggerType.Info, true, false, true);

            while (true)
            {
                if (++counter > 3)
                {
                    Logger.Write($"Не удалось создать канал: {url}", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    return false;
                }

                Instance.ActiveTab.Navigate(url, true);
                AcceptingPrivacyPolicyCookie();

                if (_startPageCreateChannelZen == StartPageCreateChannelEnum.ZenYandexMedia)
                {
                    try
                    {
                        var heHrefZen = Instance.FindFirstElement(xpathHrefZen, true, true, 5);

                        // Модифицируем ссылку кнопки ru на com
                        if (_modifyButtonYandexZenMediaIfProxyUsa && Domain == "com")
                        {
                            var currentHref = heHrefZen.GetAttribute("href");

                            if (currentHref.Contains("zen.yandex.ru"))
                            {
                                heHrefZen.SetAttribute("href", currentHref.Replace("zen.yandex.ru", "zen.yandex.com"));

                                if (counter == 1)
                                    Logger.Write($"[Доп.действия]\tКнопка \"Завести блог в дзене\" модифицирована", LoggerType.Info, true, false, true);
                            }
                        }

                        heHrefZen.FindChildByXPath(xpathChildButtonCreateZen[0], 0).Click(Instance.ActiveTab, Rnd.Next(150, 500));
                    }
                    catch { continue; }
                }

                // Поиск элементов страницы авторизации, либо авторизованного профиля
                for (int i = 0; i < 5; i++)
                {
                    if (!Instance.FindFirstElement(xpathLoginFormAuth, false, false, 2).IsNullOrVoid() || !Instance.FindFirstElement(xpathAuthAccountList, false, false, 2).IsNullOrVoid())
                    {
                        Logger.Write($"Переход к авторизации", LoggerType.Info, true, false, true);

                        startPageIsOpen = true;
                        break;
                    }
                    else if (!Instance.FindFirstElement(xpathSettingsChannel, false, false, 2).IsNullOrVoid() || Instance.ActiveTab.URL.Contains("profile/editor/id"))
                    {
                        Logger.Write($"Кабинет канала открыт", LoggerType.Info, true, false, true);

                        // Клик по окну приветствия (начать прямо сейчас)
                        var heButtonStartNow = Instance.FindFirstElement(xpathButtonStartNow, false, false, 5);

                        if (!heButtonStartNow.IsNullOrVoid())
                        {
                            heButtonStartNow.Click(Instance.ActiveTab, Rnd.Next(2000, 3000));
                            Logger.Write($"[Доп.действия]\tКлик - Начать прямо сейчас", LoggerType.Info, true, false, true);
                        }

                        // Сохранение результата и выход из метода
                        return SaveCreatedChannel();
                    }
                }

                // Выход из цикла при успешном открытии стартовой страницы
                if (startPageIsOpen) break;
            }

            // Авторизация аккаунта
            var authorizationIsGood = true;
            //var authorizationIsGood = Authorization.Auth(BindingPhoneToAccountIfRequaid, xpathSettingsChannel);

            // Проверка статуса авторизации
            if (authorizationIsGood)
            {
                // Клик по окну приветствия (начать прямо сейчас)
                var heButtonStartNow = Instance.FindFirstElement(xpathButtonStartNow, false, false, 5);

                if (!heButtonStartNow.IsNullOrVoid())
                {
                    heButtonStartNow.Click(Instance.ActiveTab, Rnd.Next(2000, 3000));
                    Logger.Write($"[Доп.действия]\tКлик - Начать прямо сейчас", LoggerType.Info, true, false, true);
                }

                // Сохранение результата и выход из метода
                return SaveCreatedChannel();
            }

            return false;
        }

        /// <summary>
        /// Сохранение канала в общую таблицу и таблицу режима (+сохранение профиля).
        /// </summary>
        /// <returns></returns>
        private bool SaveCreatedChannel()
        {
            // Сохранение результата в таблицы
            var currentUrl = Instance.ActiveTab.URL;
            var id = Regex.Match(currentUrl, @"(?<=/profile/editor/id/).*$").Value;
            var domain = Regex.Match(currentUrl, @"(?<=zen\.yandex\.).*?(?=/profile/)").Value;

            if (string.IsNullOrWhiteSpace(id))
            {
                Logger.Write($"Не найден идентификатор канала: {currentUrl}", LoggerType.Warning, true, true, true, LogColor.Yellow);
                Logger.ErrorAnalysis(true, true, true, new List<string>
                {
                    Instance.ActiveTab.URL,
                    $"Не найден идентификатор канала...",
                    string.Empty
                });
                return false;
            }
            else ZenChannel = $"https://zen.yandex.{domain}/id/{id}";

            // Бэкап данных
            Logger.MakeBackupData(new List<string>
            {
                $"Profile editor: {currentUrl}",
                $"Channel created: {ZenChannel}"
            },
            true);

            // Сохранение результата в таблицу режима и общую таблицу
            TableHandler.WriteToCellInSharedAndMode(TableColumnEnum.Inst.Login, Login, new List<InstDataItem>
            {
                new InstDataItem(TableColumnEnum.Inst.ZenChannel, ZenChannel),
                new InstDataItem(TableColumnEnum.Inst.ChannelDatetimeCreated, Logger.GetDateTime(DateTimeFormat.yyyyMMddThreeSpaceHHmmss))
            });

            // Сохранение профиля
            ProfileWorker_obsolete.SaveProfile(true);

            Logger.Write($"[Channel created: {ZenChannel}]\tКанал успешно создан", LoggerType.Info, true, true, true, LogColor.Green);
            return true;
        }

        /// <summary>
        /// Выполнить настройку и оформление канала.
        /// </summary>
        /// <param name="channelSettings"></param>
        private void ExecuteSettingsAndDesign(List<ChannelSettingsEnum> channelSettings)
        {
            while (true)
            {
                if (channelSettings.Count == 0) break;
                /*
                    todo - Проверять текущую страницу и открытость формы
                */

                var setting = channelSettings[0];

                switch (setting)
                {
                    case ChannelSettingsEnum.BindingPhoneToChannel:
                        BindingPhoneToChannel();
                        break;

                    case ChannelSettingsEnum.ChangeChannelImage:
                        ChangeChannelImage();
                        break;

                    case ChannelSettingsEnum.ChangeChannelName:
                        ChangeChannelName();
                        break;

                    case ChannelSettingsEnum.ChangeChannelDescription:
                        ChangeChannelDescription();
                        break;

                    case ChannelSettingsEnum.AddUrlToSocialNetwork:
                        AddUrlToSocialNetwork();
                        break;

                    case ChannelSettingsEnum.EnablePrivateMessages:
                        EnablePrivateMessages();
                        break;

                    case ChannelSettingsEnum.SetMail:
                        SetMail();
                        break;

                    case ChannelSettingsEnum.AgreeToReceiveZenNewsletter:
                        AgreeToReceiveZenNewsletter();
                        break;

                    case ChannelSettingsEnum.SetSite:
                        SetSite();
                        break;

                    case ChannelSettingsEnum.ConnectMetric:
                        ConnectMetric();
                        break;

                    case ChannelSettingsEnum.AcceptTermsOfUserAgreement:
                        AcceptTermsOfUserAgreement();
                        break;
                }



                channelSettings.RemoveAt(0);
            }

            /*
                todo - Добавить сохранение настроек оформления канала, если были изменения (ChangesWereChannelSettings = true)
            */
        }

        /// <summary>
        /// Привязать телефон к каналу.
        /// </summary>
        /// <returns></returns>
        private BindingPhoneToChannelData BindingPhoneToChannel()
        {
            var bindingPhoneToChannel = new BindingPhoneToChannelData();

            var xpathFieldPhone = new[] { "//form[contains(@class, 'phone-validate')]/descendant::input", "Поле - Номер телефона" };
            var xpathFieldSmsCode = new[] { "//form[contains(@class, 'phone-validate')]/descendant::div[contains(@class, 'code') and contains(@class, 'phone')]/descendant::input", "Поле - Sms код" };
            var xpathButtonSubmit = new[] { "//form[contains(@class, 'phone-validate')]/descendant::button[contains(@type, 'submit')]", "Кнопка - Подтвердить" };
            var xpathButtonReSendCode = new[] { "//form[contains(@class, 'phone-validate')]/descendant::div[contains(@class, 'resend')]", "Кнопка - Повторная отправка sms кода" };
            var xpathButtonConfirmSmsCode = new[] { "//form[contains(@class, 'phone-validate')]/descendant::div[contains(@class, 'button') and contains(@class, 'phone')]/descendant::button[contains(@type, 'submit')]", "Кнопка - Подтвердить введенный код" };
            var xpathCheckmark = new[] { "//form[contains(@class, 'phone-validate')]/descendant::div[contains(@class, 'phone-validate') and contains(@class, 'checkmark')]", "Галка - Номер привязан" };

            var heFieldPhone = Instance.FindFirstElement(xpathFieldPhone, false, true);
            var heButtonSubmit = Instance.FindFirstElement(xpathButtonSubmit, false, true);

            // Проверка наличия элемента
            if (heFieldPhone.IsNullOrVoid())
            {
                Logger.Write($"Не найдено поле для указания телефона", LoggerType.Warning, true, true, true, LogColor.Yellow);
                Logger.ErrorAnalysis(true, true, true, new List<string>
                {
                    Instance.ActiveTab.URL,
                    $"Не найдено поле для указания телефона",
                    xpathFieldPhone.FormatXPathForLog(),
                    string.Empty
                });
                return null;
            }

            // Привязка номера
            if (!string.IsNullOrWhiteSpace(heFieldPhone.GetAttribute("value")))
            {
                var phone = heFieldPhone.GetAttribute("value");

                Logger.Write($"К каналу уже привязан номер телефона: {phone}", LoggerType.Info, true, false, true);

                var rxJobId = new Regex(@"(?<=\[Sms\ job\ id:\ ).*?(?=])");
                var rxDll = new Regex(@"(?<=\[Sms\ service\ dll:\ ).*?(?=])");
                var str = Logger.GetAccountLog(LogFilter.AllLines).Where(x => x.Contains(phone) && rxJobId.IsMatch(x) && rxDll.IsMatch(x)).Last();

                bindingPhoneToChannel.ActionsStatus = true;
                bindingPhoneToChannel.TimeAction = new TimeData();
                bindingPhoneToChannel.Phone = phone;
                bindingPhoneToChannel.JobId = rxJobId.Match(str).Value;
                bindingPhoneToChannel.ServiceDll = rxDll.Match(str).Value;
            }
            else if (!string.IsNullOrWhiteSpace(Phone))
            {
                // Лайтовая установка телефона, если к аккаунту привязан уже номер
                heFieldPhone.SetValue(Instance.ActiveTab, Phone, LevelEmulation.SuperEmulation, Rnd.Next(150, 500));
                heButtonSubmit.Click(Instance.ActiveTab, Rnd.Next(500, 1000));

                // Проверка наличия checkmark
                if (Instance.FindFirstElement(xpathCheckmark, false, true, 5).IsNullOrVoid())
                {
                    Logger.Write($"Не найден checkmark подтверждающий, что номер привязался", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    Logger.ErrorAnalysis(true, true, true, new List<string>
                    {
                        Instance.ActiveTab.URL,
                        $"Не найден checkmark подтверждающий, что номер привязался",
                        xpathCheckmark.FormatXPathForLog(),
                        string.Empty
                    });
                    return null;
                }
                else Logger.Write($"Телефон успешно привязан к каналу: {Phone}", LoggerType.Info, true, false, true, LogColor.Blue);

                // Сохранение данных
                Logger.MakeBackupData(new List<string>
                {
                    $"Channel binding phone: {Phone}"
                },
                true);

                var rxJobId = new Regex(@"(?<=\[Sms\ job\ id:\ ).*?(?=])");
                var rxDll = new Regex(@"(?<=\[Sms\ service\ dll:\ ).*?(?=])");
                var str = Logger.GetAccountLog(LogFilter.AllLines).Where(x => x.Contains(Phone) && rxJobId.IsMatch(x) && rxDll.IsMatch(x)).Last();

                bindingPhoneToChannel.ActionsStatus = true;
                bindingPhoneToChannel.TimeAction = new TimeData();
                bindingPhoneToChannel.Phone = Phone;
                bindingPhoneToChannel.JobId = rxJobId.Match(str).Value;
                bindingPhoneToChannel.ServiceDll = rxDll.Match(str).Value;
            }
            else
            {
                // Получение номера
                Phone = SmsService_obsolete.GetPhone(out string job_id, TimeToSecondsWaitPhone);

                // Выход из метода, если не удалось получить номер
                if (string.IsNullOrWhiteSpace(Phone))
                {
                    return null;
                }

                var phoneLog = $"[Sms service dll: {DataKeeper_obsolete.PhoneService.Dll}]\t[Sms job id: {job_id}]\t[Phone: {Phone}]\t";

                // Ввод номера телефона и отправка sms кода
                heFieldPhone.SetValue(Instance.ActiveTab, Phone, LevelEmulation.SuperEmulation, Rnd.Next(150, 500));
                heButtonSubmit.Click(Instance.ActiveTab, Rnd.Next(1000, 1500));

                // Получение кнопки для повторной отправки sms кода
                var heButtonReSendCode = Instance.FindFirstElement(xpathButtonReSendCode, false, true);

                if (heButtonReSendCode.IsNullOrVoid())
                {
                    Logger.Write($"Не найдена кнопка для повторной отправки sms кода", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    Logger.ErrorAnalysis(true, true, true, new List<string>
                    {
                        Instance.ActiveTab.URL,
                        $"Не найдена кнопка для повторной отправки sms кода",
                        xpathFieldPhone.FormatXPathForLog(),
                        string.Empty
                    });
                }

                // Получение sms кода
                var sms_code = SmsService_obsolete.GetSmsCode(job_id, MinutesWaitSmsCode, heButtonReSendCode, AttemptsReSendSmsCode, phoneLog);

                // Проверка наличия sms кода (если кода нет нет, то отмена номера и выход из метода)
                if (string.IsNullOrWhiteSpace(sms_code))
                {
                    SmsService_obsolete.CancelPhone(job_id, phoneLog);
                    return null;
                }

                Logger.Write($"{phoneLog}Код успешно получен: {sms_code}", LoggerType.Info, true, false, true, LogColor.Blue);

                // Получение элементов для ввода sms кода
                var heFieldSmsCode = Instance.FindFirstElement(xpathFieldSmsCode, false, true, 5);
                var heButtonConfirmSmsCode = Instance.FindFirstElement(xpathButtonConfirmSmsCode, false, true);

                // Проверка наличия элементов для ввода sms кода
                if (new[] { heFieldSmsCode, heButtonConfirmSmsCode }.Any(x => x.IsNullOrVoid()))
                {
                    Logger.Write($"Не найдены элементы для подтверждения sms кода", LoggerType.Warning, true, true, true, LogColor.Yellow);

                    var heElements = new List<string>();

                    if (heFieldSmsCode.IsNullOrVoid()) heElements.Add(xpathFieldSmsCode.FormatXPathForLog());
                    if (heButtonConfirmSmsCode.IsNullOrVoid()) heElements.Add(xpathButtonConfirmSmsCode.FormatXPathForLog());

                    Logger.ErrorAnalysis(true, true, true, new List<string>
                    {
                        Instance.ActiveTab.URL,
                        $"Не найдены элементы для подтверждения sms кода",
                        string.Join(Environment.NewLine, heElements),
                        string.Empty
                    });

                    return null;
                }

                // Ввод sms кода
                heFieldSmsCode.SetValue(Instance.ActiveTab, Phone, LevelEmulation.SuperEmulation, Rnd.Next(1000, 1500));
                heButtonConfirmSmsCode.Click(Instance.ActiveTab, Rnd.Next(1000, 1500));

                // Проверка наличия checkmark
                if (Instance.FindFirstElement(xpathCheckmark, false, true, 5).IsNullOrVoid())
                {
                    Logger.Write($"Не найден checkmark подтверждающий, что номер привязался", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    Logger.ErrorAnalysis(true, true, true, new List<string>
                    {
                        Instance.ActiveTab.URL,
                        $"Не найден checkmark подтверждающий, что номер привязался",
                        xpathCheckmark.FormatXPathForLog(),
                        string.Empty
                    });
                    return null;
                }
                else Logger.Write($"Телефон успешно привязан к каналу: {Phone}", LoggerType.Info, true, false, true, LogColor.Blue);

                // Сохранение данных
                Logger.MakeBackupData(new List<string>
                {
                    $"Channel binding phone: {Phone}"
                },
                true);

                bindingPhoneToChannel.ActionsStatus = true;
                bindingPhoneToChannel.TimeAction = new TimeData();
                bindingPhoneToChannel.Phone = Phone;
                bindingPhoneToChannel.JobId = job_id;
                bindingPhoneToChannel.ServiceDll = DataKeeper_obsolete.PhoneService.Dll;
            }

            return bindingPhoneToChannel;
        }

        /// <summary>
        /// Изменить изображение канала.
        /// </summary>
        /// <returns></returns>
        private ChangeChannelImageData ChangeChannelImage()
        {
            var changeChannelImageData = new ChangeChannelImageData();

            var xpathMenuAvatar = new[] { "//div[contains(@class, 'profile-menu-avatar')]", "Батя для работы с аватаром" };
            var xpathElementForCheck = new[] { ".//div[contains(@style, 'background-image')]", "Элемент с сылкой на текущий аватар" };
            var xpathButtonChangeImage = new[] { ".//div[contains(@class, 'change-logo')]", "Кнопка - Изменить аватар" };

            var setted = default(bool);
            var counterAttempts = default(int);

            while (true)
            {
                // Счетчик попыток
                if (++counterAttempts > 3)
                {
                    Logger.Write($"Не удалось установить описание канала", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    Logger.ErrorAnalysis(true, true, true, new List<string>
                    {
                        Instance.ActiveTab.URL,
                        $"Не удалось установить описание канала",
                        string.Empty
                    });
                    return null;
                }

                // Получение элементов для изменения изображения канала
                var heMenuAvatar = Instance.FindFirstElement(xpathMenuAvatar, false, true, 5);
                var heElementForCheck = heMenuAvatar.FindChildByXPath(xpathElementForCheck[0], 0);
                var heButtonChangeImage = heMenuAvatar.FindChildByXPath(xpathButtonChangeImage[0], 0);

                // Проверка наличия элемента
                if (new[] { heMenuAvatar, heElementForCheck, heButtonChangeImage }.Any(x => x.IsNullOrVoid()))
                {
                    Logger.Write($"Не удалось изменить изображение канала", LoggerType.Warning, true, true, true, LogColor.Yellow);

                    var heElements = new List<string>();

                    if (heMenuAvatar.IsNullOrVoid()) heElements.Add(xpathMenuAvatar.FormatXPathForLog());
                    if (heElementForCheck.IsNullOrVoid()) heElements.Add(xpathElementForCheck.FormatXPathForLog());
                    if (heButtonChangeImage.IsNullOrVoid()) heElements.Add(xpathButtonChangeImage.FormatXPathForLog());

                    Logger.ErrorAnalysis(true, true, true, new List<string>
                    {
                        Instance.ActiveTab.URL,
                        $"Не найден какой-то элемент для изменения изображения канала...",
                        string.Join(Environment.NewLine, heElements),
                        string.Empty
                    });

                    return null;
                }

                if (!heElementForCheck.GetAttribute("style").Contains("avatar"))
                {
                    Instance.SetFilesForUpload(AvatarInfo, true);
                    heButtonChangeImage.Click(Instance.ActiveTab, Rnd.Next(10000, 12000));
                    setted = true;
                }
                else
                {
                    Logger.Write(setted ? $"Изображение канала успешно изменено" : $"Изображение канала уже было изменено", LoggerType.Info, true, false, true);

                    changeChannelImageData.ActionsStatus = true;
                    changeChannelImageData.TimeAction = new TimeData();
                    changeChannelImageData.ImageFile = AvatarInfo.FullName;

                    return changeChannelImageData;
                }
            }
        }

        /// <summary>
        /// Изменение названия канала.
        /// </summary>
        /// <returns></returns>
        private ChangeChannelNameData ChangeChannelName()
        {
            var changeChannelNameData = new ChangeChannelNameData();

            var xpathFieldChannelName = new[] { "//div[contains(@class, 'profile-menu')]/descendant::div[contains(@class, 'name-container')]/descendant::input", "Поле - Название канала" };

            var setted = default(bool);
            var counterAttempts = default(int);

            while (true)
            {
                // Счетчик попыток
                if (++counterAttempts > 3)
                {
                    Logger.Write($"Не удалось изменить название канала", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    Logger.ErrorAnalysis(true, true, true, new List<string>
                    {
                        Instance.ActiveTab.URL,
                        $"Не удалось изменить название канала",
                        string.Empty
                    });
                    return null;
                }

                // Получение поля с описанием канала
                var heFieldChannelName = Instance.FindFirstElement(xpathFieldChannelName, false, true, 5);

                // Проверка наличия элемента
                if (heFieldChannelName.IsNullOrVoid())
                {
                    Logger.Write($"Не найдено поле с названием канала", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    Logger.ErrorAnalysis(true, true, true, new List<string>
                    {
                        Instance.ActiveTab.URL,
                        $"Не найдено поле с названием канала",
                        xpathFieldChannelName.FormatXPathForLog(),
                        string.Empty
                    });
                    return null;
                }

                // Проверяем название канала
                if (heFieldChannelName.GetAttribute("value") != ChannelName)
                {
                    var position = default(int);
                    var counterBackspaceIterations = default(int);

                    // Клик по форме для фокусировки
                    heFieldChannelName.Click(Instance.ActiveTab, Rnd.Next(1000, 2000));

                    while (true)
                    {
                        if (++counterBackspaceIterations > 100)
                        {
                            Logger.Write($"Превышено количество попыток удаления старого названия канала", LoggerType.Warning, true, true, true, LogColor.Yellow);
                            Logger.ErrorAnalysis(true, true, true, new List<string>
                            {
                                Instance.ActiveTab.URL,
                                $"Превышено количество попыток удаления старого названия канала",
                                string.Empty
                            });
                            return null;
                        }

                        var length = heFieldChannelName.GetAttribute("value").Length;

                        // Выходим из цикла при успешном удалении имени
                        if (length == 0) break;

                        // Кликаем по полю, если количество букв не изменилось
                        if (length == position)
                        {
                            heFieldChannelName.Click(Instance.ActiveTab, Rnd.Next(500, 1000), false);
                        }
                        else position = length;

                        // Проверяем фокусировку на поле
                        if (!heFieldChannelName.ParentElement.GetAttribute("class").Contains("focused")) heFieldChannelName.Click(Instance.ActiveTab, Rnd.Next(1000, 2000));

                        // Удаление символов
                        Instance.ActiveTab.KeyEvent("Back", "press", "");

                        Thread.Sleep(new Random().Next(20, 150));
                    }

                    // Заполнение поля
                    heFieldChannelName.SetValue(Instance.ActiveTab, ChannelName, LevelEmulation.SuperEmulation, Rnd.Next(1000, 2000));

                    setted = true;
                }
                else
                {
                    Logger.Write(setted ? $"Название канала успешно изменено" : $"Название канала уже было измененное", LoggerType.Info, true, false, true);

                    changeChannelNameData.ActionsStatus = true;
                    changeChannelNameData.TimeAction = new TimeData();
                    changeChannelNameData.ChannelName = ChannelName;

                    return changeChannelNameData;
                }
            }
        }

        /// <summary>
        /// Заполнение поля с описанием канала.
        /// </summary>
        private ChangeChannelDescriptionData ChangeChannelDescription()
        {
            var changeChannelDescriptionData = new ChangeChannelDescriptionData();

            var xpathFieldDescription = new[] { "//div[contains(@class, 'profile-menu')]/descendant::textarea[contains(@class, 'control')]", "Поле - Описание канала" };

            var setted = default(bool);
            var counterAttempts = default(int);

            while (true)
            {
                // Счетчик попыток
                if (++counterAttempts > 3)
                {
                    Logger.Write($"Не удалось установить описание канала", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    Logger.ErrorAnalysis(true, true, true, new List<string>
                    {
                        Instance.ActiveTab.URL,
                        $"Не удалось установить описание канала",
                        string.Empty
                    });
                    return null;
                }

                // Получение поля с описанием канала
                var heFieldDescription = Instance.FindFirstElement(xpathFieldDescription, false, true, 5);

                // Проверка наличия элемента
                if (heFieldDescription.IsNullOrVoid())
                {
                    Logger.Write($"Не найдено поле с описанием канала", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    Logger.ErrorAnalysis(true, true, true, new List<string>
                    {
                        Instance.ActiveTab.URL,
                        $"Не найдено поле с описанием канала",
                        xpathFieldDescription.FormatXPathForLog(),
                        string.Empty
                    });
                    return null;
                }

                if (string.IsNullOrWhiteSpace(heFieldDescription.GetAttribute("innerhtml")))
                {
                    heFieldDescription.SetValue(Instance.ActiveTab, DescriptionChannel, LevelEmulation.SuperEmulation, Rnd.Next(1000, 2000));
                    setted = true;
                }
                else
                {
                    Logger.Write(setted ? $"Описание канала успешно установлено" : $"Описание канала уже было установлено", LoggerType.Info, true, false, true);

                    changeChannelDescriptionData.ActionsStatus = true;
                    changeChannelDescriptionData.TimeAction = new TimeData();
                    changeChannelDescriptionData.ChannelDescription = DescriptionChannel;

                    return changeChannelDescriptionData;
                }
            }
        }

        /// <summary>
        /// Установить ссылку на социальную сеть.
        /// </summary>
        /// <returns></returns>
        private AddUrlToSocialNetworkData AddUrlToSocialNetwork()
        {
            var addUrlToSocialNetworkData = new AddUrlToSocialNetworkData();

            var xpathSocialLinkElement = new[] { "//div[contains(@class, 'profile-menu') and contains(@class, 'social-links')]", "Батя отвечающий за ссылки соц.сетей" };
            var xpathButtonAddSocialLink = new[] { ".//div[contains(@class, 'add-link')]/descendant::button", "Кнопка - Добавить ссылку на соц.сеть" };
            var xpathFieldItemSocialLink = new[] { ".//ul[contains(@class, 'social-links') and contains(@class, 'list')]/descendant::div[contains(@class, 'item')]/descendant::input", "Поле - Ссылка на соц.сеть" };
            var xpathErrorSocialLink = new[] { ".//ul[contains(@class, 'social-links') and contains(@class, 'list')]/descendant::div[contains(@class, 'error')]", "Ошибка ссылки на соц.сети" };
            var xpathSocialLinkTitle = new[] { ".//span[contains(@class, 'title')]", "Тайтл - Соц.сеть" };

            var setted = default(bool);
            var counterAttempts = default(int);

            while (true)
            {
                // Счетчик попыток
                if (++counterAttempts > 3)
                {
                    Logger.Write($"Не удалось установить ссылку на соц.сеть", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    Logger.ErrorAnalysis(true, true, true, new List<string>
                    {
                        Instance.ActiveTab.URL,
                        $"Не удалось установить ссылку на соц.сеть",
                        string.Empty
                    });
                    return null;
                }

                // Получение поля с описанием канала
                var heSocialLinkElement = Instance.FindFirstElement(xpathSocialLinkElement, false, true, 5);
                var heButtonAddSocialLink = heSocialLinkElement.FindChildByXPath(xpathButtonAddSocialLink[0], 0);
                var heFieldItemSocialLink = heSocialLinkElement.FindChildByXPath(xpathFieldItemSocialLink[0], 0);
                var heErrorSocialLink = heSocialLinkElement.FindChildByXPath(xpathErrorSocialLink[0], 0);

                // Проверка наличия родительского элемента
                if (heSocialLinkElement.IsNullOrVoid())
                {
                    Logger.Write($"Не найден родительский элемент для установки ссылки на соц.сеть", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    Logger.ErrorAnalysis(true, true, true, new List<string>
                    {
                        Instance.ActiveTab.URL,
                        $"Не найден родительский элемент для установки ссылки на соц.сеть",
                        xpathSocialLinkElement.FormatXPathForLog(),
                        string.Empty
                    });
                    return null;
                }

                if (!heErrorSocialLink.IsNullOrVoid())
                {
                    heFieldItemSocialLink.SetAttribute("value", "");

                    Logger.Write($"Некорректная ссылка на соц.сеть", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    Logger.ErrorAnalysis(true, true, true, new List<string>
                    {
                        Instance.ActiveTab.URL,
                        $"Некорректная ссылка на соц.сеть: {InstagramUrl}",
                        string.Empty
                    });
                    return null;
                }
                else if (!heFieldItemSocialLink.IsNullOrVoid())
                {
                    // Проверка на наличие ссылки на соц.сеть и её ввод
                    if (string.IsNullOrWhiteSpace(heFieldItemSocialLink.GetAttribute("value")))
                    {
                        // Ввод ссылки на соц.сеть
                        heFieldItemSocialLink.SetValue(Instance.ActiveTab, InstagramUrl, LevelEmulation.SuperEmulation, Rnd.Next(2000, 3000));
                        heSocialLinkElement.FindChildByXPath(xpathSocialLinkTitle[0], 0).Click(Instance.ActiveTab, Rnd.Next(500, 1000));
                        setted = true;
                    }
                    else
                    {
                        Logger.Write(setted ? $"Ссылка на соц.сеть успешно установлена" : $"Ссылка на соц.сеть уже была установлена", LoggerType.Info, true, false, true);

                        addUrlToSocialNetworkData.ActionsStatus = true;
                        addUrlToSocialNetworkData.TimeAction = new TimeData();
                        addUrlToSocialNetworkData.SocialUrl = InstagramUrl;

                        return addUrlToSocialNetworkData;
                    }
                }
                else if (!heButtonAddSocialLink.IsNullOrVoid())
                {
                    heButtonAddSocialLink.Click(Instance.ActiveTab, Rnd.Next(2000, 3000));
                }
                else
                {
                    var heElements = new List<string>();

                    if (heButtonAddSocialLink.IsNullOrVoid()) heElements.Add(xpathButtonAddSocialLink.FormatXPathForLog());
                    if (heFieldItemSocialLink.IsNullOrVoid()) heElements.Add(xpathFieldItemSocialLink.FormatXPathForLog());

                    Logger.Write($"Не найдены элементы для установки соц.сети", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    Logger.ErrorAnalysis(true, true, true, new List<string>
                    {
                        Instance.ActiveTab.URL,
                        $"Не найдены элементы для установки соц.сети",
                        string.Join(Environment.NewLine, heElements),
                        string.Empty
                    });
                    return null;
                }
            }
        }

        /// <summary>
        /// Включение личных сообщений.
        /// </summary>
        /// <returns></returns>
        private EnablePrivateMessagesData EnablePrivateMessages()
        {
            var enablePrivateMessagesData = new EnablePrivateMessagesData();

            var xpathElementMessageStatus = new[] { "//p[contains(@class, 'profile-direct-messages') and contains(@class, 'status_type')]", "Элемент, где нужно проверить маркер разрешающий получать сообщения" };
            var xpathButtonMessageEnable = new[] { "//p[contains(@class, 'messenger-edit-link')]/descendant::a[contains(@class, 'edit-link')]", "Кнопка - Включить сообщения" };

            var setted = default(bool);
            var counterAttempts = default(int);

            while (true)
            {
                // Счетчик попыток
                if (++counterAttempts > 3)
                {
                    Logger.Write($"Не удалось включить получение личных сообщений", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    Logger.ErrorAnalysis(true, true, true, new List<string>
                    {
                        Instance.ActiveTab.URL,
                        $"Не удалось включить получение личных сообщений",
                        string.Empty
                    });
                    return null;
                }

                // Получение элементов обработки личных сообщений
                var heButtonMessageEnable = Instance.FindFirstElement(xpathButtonMessageEnable, false, true, 5);
                var heElementForCheckMessageStatus = Instance.FindFirstElement(xpathElementMessageStatus, false, true);

                // Проверка наличия элемента
                if (heButtonMessageEnable.IsNullOrVoid())
                {
                    Logger.Write($"Не найдена кнопка для включения личных сообщений", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    Logger.ErrorAnalysis(true, true, true, new List<string>
                    {
                        Instance.ActiveTab.URL,
                        $"Не найдена кнопка для включения личных сообщений",
                        xpathButtonMessageEnable.FormatXPathForLog(),
                        string.Empty
                    });
                    return null;
                }

                // Проверка наличия элемента
                if (heElementForCheckMessageStatus.IsNullOrVoid())
                {
                    Logger.Write($"Не найден элемент для проверки включения личных сообщений", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    Logger.ErrorAnalysis(true, true, true, new List<string>
                    {
                        Instance.ActiveTab.URL,
                        $"Не найден элемент для проверки включения личных сообщений",
                        xpathElementMessageStatus.FormatXPathForLog(),
                        string.Empty
                    });
                    return null;
                }

                // Включение личных сообщений
                if (!heElementForCheckMessageStatus.GetAttribute("class").ToLower().Contains("allowed"))
                {
                    heButtonMessageEnable.Click(Instance.ActiveTab, Rnd.Next(1000, 2000));
                    setted = true;
                }
                else
                {
                    Logger.Write(setted ? $"Получение сообщений успешно включено" : $"Получение сообщений уже было включено", LoggerType.Info, true, false, true);

                    enablePrivateMessagesData.ActionsStatus = true;
                    enablePrivateMessagesData.TimeAction = new TimeData();

                    return enablePrivateMessagesData;
                }
            }
        }

        /// <summary>
        /// Установка почты канала.
        /// </summary>
        /// <returns></returns>
        private SetMailData SetMail()
        {
            var setMailData = new SetMailData();

            var xpathSelectMail = new[] { "//div[contains(@class, 'profile-menu')]/descendant::textarea[contains(@class, 'control')]", "Селект - Электронная почта" };
            var xpatChildOption = new[] { ".//div[contains(@class, 'option')]", "Почта из списка" };

            var setted = default(bool);
            var counterAttempts = default(int);

            while (true)
            {
                // Счетчик попыток
                if (++counterAttempts > 3)
                {
                    Logger.Write($"Не удалось установить описание канала", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    Logger.ErrorAnalysis(true, true, true, new List<string>
                    {
                        Instance.ActiveTab.URL,
                        $"Не удалось установить описание канала",
                        string.Empty
                    });
                    return null;
                }

                // Получение селектора почты
                var heSelectMail = Instance.FindFirstElement(xpathSelectMail, false, true, 5);

                // Проверка наличия элемента
                if (heSelectMail.IsNullOrVoid())
                {
                    Logger.Write($"Не найден селектор с выбором почты", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    Logger.ErrorAnalysis(true, true, true, new List<string>
                    {
                        Instance.ActiveTab.URL,
                        $"Не найден селектор с выбором почты",
                        xpathSelectMail.FormatXPathForLog(),
                        string.Empty
                    });
                    return null;
                }

                // Установка почты
                if (heSelectMail.GetAttribute("innerhtml") != "")
                {
                    heSelectMail.Click(Instance.ActiveTab, Rnd.Next(3000, 4000));

                    var heOption = heSelectMail.NextSibling.FindChildByXPath(xpatChildOption[0], 0);

                    // Выбираем почту из списка
                    if (heOption.IsNullOrVoid())
                    {
                        Logger.Write($"Не элемент с почтой", LoggerType.Warning, true, false, false);
                    }
                    else heOption.Click(Instance.ActiveTab, Rnd.Next(3000, 4000));

                    setted = true;
                }
                else
                {
                    Logger.Write(setted ? $"Почта канала успешно установлена" : $"Почта канала уже было установлена", LoggerType.Info, true, false, true);

                    setMailData.ActionsStatus = true;
                    setMailData.TimeAction = new TimeData();
                    setMailData.Mail = heSelectMail.GetAttribute("innerhtml");

                    return setMailData;
                }
            }
        }

        /// <summary>
        /// Согласие на рассылку от дзен.
        /// </summary>
        /// <returns></returns>
        private AgreeToReceiveZenNewsletterData AgreeToReceiveZenNewsletter()
        {
            var agreeToReceiveZenNewsletterData = new AgreeToReceiveZenNewsletterData();

            var xpathCheckElement = new[] { "//div[contains(@class, 'contact-info')]/descendant::label[contains(@class, 'checkbox') and contains(@class, 'with-label')]", "Лейбл - Я согласен получать рассылку Дзен" };
            var xpathChildCheckbox = new[] { ".//input[contains(@class, 'checkbox')]", "Чекбокс - Я согласен получать рассылку Дзен" };

            var setted = default(bool);
            var counterAttempts = default(int);

            while (true)
            {
                // Счетчик попыток
                if (++counterAttempts > 3)
                {
                    Logger.Write($"Не удалось поставить чекбокс \"Я согласен получать рассылку Дзен\"", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    Logger.ErrorAnalysis(true, true, true, new List<string>
                    {
                        Instance.ActiveTab.URL,
                        $"Не удалось поставить чекбокс \"Я согласен получать рассылку Дзен\"",
                        string.Empty
                    });
                    return null;
                }

                // Получение элементов
                var heCheckElement = Instance.FindFirstElement(xpathCheckElement, false, true, 5);
                var heChildCheckbox = heCheckElement.FindChildByXPath(xpathChildCheckbox[0], 0);

                // Проверка наличия элемента
                if (new[] { heCheckElement, heChildCheckbox }.Any(x => x.IsNullOrVoid()))
                {
                    Logger.Write($"Не найден какой-то элемент для принятия согласия рассылки дзен...", LoggerType.Warning, true, true, true, LogColor.Yellow);

                    var heElements = new List<string>();

                    if (heCheckElement.IsNullOrVoid()) heElements.Add(xpathCheckElement.FormatXPathForLog());
                    if (heChildCheckbox.IsNullOrVoid()) heElements.Add(xpathChildCheckbox.FormatXPathForLog());

                    Logger.ErrorAnalysis(true, true, true, new List<string>
                    {
                        Instance.ActiveTab.URL,
                        $"Не найден какой-то элемент для принятия согласия рассылки дзен...",
                        string.Join(Environment.NewLine, heElements),
                        string.Empty
                    });

                    return null;
                }

                if (!heCheckElement.GetAttribute("class").ToLower().Contains("is-checked"))
                {
                    heChildCheckbox.Click(Instance.ActiveTab, Rnd.Next(1500, 3000));
                    setted = true;
                }
                else
                {
                    Logger.Write(setted ? $"Согласие на рассылку дзен успешно принято" : $"Согласие на рассылку дзен уже была принято", LoggerType.Info, true, false, true);

                    agreeToReceiveZenNewsletterData.ActionsStatus = true;
                    agreeToReceiveZenNewsletterData.TimeAction = new TimeData();

                    return agreeToReceiveZenNewsletterData;
                }
            }
        }

        /// <summary>
        /// Заполнение поля с сайтом.
        /// </summary>
        /// <returns></returns>
        private SetSiteData SetSite()
        {
            var setSiteData = new SetSiteData();

            var xpathFieldSite = new[] { "//div[contains(@class, 'profile-menu')]/descendant::div[contains(@class, 'profile-input')]/descendant::input[contains(@class, 'textinput')]", "Поле - Сайт" };
            var xpathFieldSiteStatus = new[] { $"{xpathFieldSite[0]}/ancestor::span[contains(@class, 'textinput')]", "Статус поля" };

            var setted = default(bool);
            var counterAttempts = default(int);

            while (true)
            {
                // Счетчик попыток
                if (++counterAttempts > 3)
                {
                    Logger.Write($"Не удалось установить сайт", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    Logger.ErrorAnalysis(true, true, true, new List<string>
                    {
                        Instance.ActiveTab.URL,
                        $"Не удалось установить сайт",
                        string.Empty
                    });
                    return null;
                }

                // Получение поля с описанием канала
                var heFieldSite = Instance.FindFirstElement(xpathFieldSite, false, true, 5);
                var heFieldSiteStatus = Instance.FindFirstElement(xpathFieldSiteStatus, false, true);

                // Проверка наличия элемента
                if (heFieldSite.IsNullOrVoid())
                {
                    Logger.Write($"Не найдено поле с сайтом", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    Logger.ErrorAnalysis(true, true, true, new List<string>
                    {
                        Instance.ActiveTab.URL,
                        $"Не найдено поле с сайтом",
                        xpathFieldSite.FormatXPathForLog(),
                        string.Empty
                    });
                    return null;
                }

                // Проверка наличия элемента
                if (heFieldSiteStatus.IsNullOrVoid())
                {
                    Logger.Write($"Не найдено поле с сайтом", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    Logger.ErrorAnalysis(true, true, true, new List<string>
                    {
                        Instance.ActiveTab.URL,
                        $"Не найдено поле с сайтом",
                        xpathFieldSiteStatus.FormatXPathForLog(),
                        string.Empty
                    });
                    return null;
                }

                // Проверка корректности заполнения сайта
                if (heFieldSiteStatus.GetAttribute("class").ToLower().Contains("error"))
                {
                    // Сброс значения поля
                    heFieldSite.SetAttribute("value", "");

                    Logger.Write($"Поле с сайтом заполнено некорректно", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    Logger.ErrorAnalysis(true, true, true, new List<string>
                    {
                        Instance.ActiveTab.URL,
                        $"Поле с сайтом заполнено некорректно",
                        xpathFieldSiteStatus.FormatXPathForLog(),
                        string.Empty
                    });
                    return null;
                }

                // Заполнение поля
                if (string.IsNullOrWhiteSpace(heFieldSite.GetAttribute("value")))
                {
                    heFieldSite.SetValue(Instance.ActiveTab, InstagramUrl, LevelEmulation.SuperEmulation, Rnd.Next(2000, 3000));
                    setted = true;
                }
                else
                {
                    Logger.Write(setted ? $"Сайт на канале успешно установлен" : $"Сайт на канале уже был установлен", LoggerType.Info, true, false, true);

                    setSiteData.ActionsStatus = true;
                    setSiteData.TimeAction = new TimeData();
                    setSiteData.SiteUrl = InstagramUrl;

                    return setSiteData;
                }
            }
        }

        /// <summary>
        /// Получение и установка счетчика канала.
        /// </summary>
        /// <returns></returns>
        private ConnectMetricData ConnectMetric()
        {
            var connectMetricData = new ConnectMetricData();

            var xpathCurrentMetrikaId = new[] { "//div[contains(@class, 'profile-metrika')]/descendant::a[contains(@class, 'profile-metrika') and @href!='']", "Элемент с текущим id метрики" };
            var xpathButtonAddMetrika = new[] { "//div[contains(@class, 'profile-metrika')]/descendant::button", "Кнопка - Добавить метрику" };
            var xpathFieldMetrikaId = new[] { "//div[contains(@class, 'profile-metrika')]/descendant::input", "Поле - Номер счетчика" };
            var xpathButtonLinkedMetrikaOk = new[] { "//div[contains(@class, 'verify-popup') and contains(@class, 'content')]/descendant::button[contains(@class, 'yellow')]", "Кнопка - Подключить" };

            var setted = default(bool);
            var counterIdForMetrika = default(string);
            var counterAttemptsSetMetrika = default(int);

            while (true)
            {
                if (++counterAttemptsSetMetrika > 3)
                {
                    Logger.Write($"Не удалось установить метрику", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    return null;
                }

                // Включение метрики
                if (Instance.FindFirstElement(xpathCurrentMetrikaId).GetAttribute("href").Contains("add"))
                {
                    Instance.FindFirstElement(xpathButtonAddMetrika).Click(Instance.ActiveTab, Rnd.Next(150, 500));

                    // Получение ID счетчика для метрики
                    counterIdForMetrika = GetCounterIdForMetrika();

                    // Завершение публикации, если не удалось настроить метрику
                    if (counterIdForMetrika == null) return null;

                    Instance.FindFirstElement(xpathFieldMetrikaId).SetValue(Instance.ActiveTab, counterIdForMetrika, LevelEmulation.SuperEmulation, Rnd.Next(500, 1000));
                    Instance.FindFirstElement(xpathButtonAddMetrika).Click(Instance.ActiveTab, Rnd.Next(500, 1000));
                    Instance.FindFirstElement(xpathButtonLinkedMetrikaOk).Click(Instance.ActiveTab, Rnd.Next(1500, 2000));

                    setted = true;
                }
                else
                {
                    Logger.Write(setted ? $"[ID: {counterIdForMetrika}]\tМетрика успешно установлена" : $"Метрика уже была установлена", LoggerType.Info, true, false, true);

                    connectMetricData.ActionsStatus = true;
                    connectMetricData.TimeAction = new TimeData();
                    connectMetricData.CounterId = counterIdForMetrika;

                    return connectMetricData;
                }
            }
        }

        /// <summary>
        /// Получение ID счетчика для метрики.
        /// </summary>
        /// <returns></returns>
        private string GetCounterIdForMetrika()
        {
            var counterIdForMetrika = default(string);

            var xpathFieldCounterName = new[] { "//span[contains(@data-bem, '\"name\":\"name\"')]/descendant::input", "Поле - Имя счетчика" };
            var xpathFieldCounterSite = new[] { "//span[contains(@data-bem, '\"name\":\"site\"')]/descendant::input", "Поле - Имя счетчика" };
            var xpathFieldEmail = new[] { "//div[contains(@class, 'counter-edit') and contains(@class, 'email')]/descendant::input", "Поле - Емейл" };
            var xpathCheckboxConditions = new[] { "//span[contains(@data-bem, '\"name\":\"agreement\"')]/descendant::input", "Чекбокс - Я принимаю условия Пользовательского соглашения" };
            var xpathCheckboxSubscriptions = new[] { "//span[contains(@data-bem, '\"name\":\"subscriptions_agreement\"')]/descendant::input", "Чекбокс - Я подтверждаю свое согласие на получение рекламных и иных маркетинговых сообщений от ООО «ЯНДЕКС»" };
            var xpathButtonSubmit = new[] { "//div[contains(@class, 'show')]/descendant::button[contains(@class, 'submit')]", "Кнопка - Создать счетчик" };

            Instance.NavigateInNewTab("Создание счетчика — Яндекс.Метрика", "https://metrika.yandex.ru/add", ZenChannel, true);

            // Получение текущего емейла
            var email = Regex.Match(Instance.ActiveTab.DomText, "(?<=\"ownEmail\":\").*?(?=\")").Value;

            if (string.IsNullOrWhiteSpace(email))
            {
                Logger.Write($"На странице получения счетчика не найден текущий емейл", LoggerType.Warning, true, true, true, LogColor.Yellow);
                Logger.ErrorAnalysis(true, true, true, new List<string>
                {
                    Instance.ActiveTab.URL,
                    $"На странице получения счетчика не найден текущий емейл",
                    string.Empty
                });

                // Закрыть все ненужные вкладки
                CloseAllTabsExceptChannel();

                return null;
            }

            // Получение элементов для обработки счетчика
            var heFieldCounterName = Instance.FindFirstElement(xpathFieldCounterName, false, true);
            var heFieldCounterSite = Instance.FindFirstElement(xpathFieldCounterSite, false, true);
            var heFieldEmail = Instance.FindFirstElement(xpathFieldEmail, false, true);
            var heCheckboxConditions = Instance.FindFirstElement(xpathCheckboxConditions, false, true);
            var heCheckboxSubscriptions = Instance.FindFirstElement(xpathCheckboxSubscriptions, false, true);
            var heButtonSubmit = Instance.FindFirstElement(xpathButtonSubmit, false, true);

            // Проверка наличия элементов
            if (new[] { heFieldCounterName, heFieldCounterSite, heFieldEmail, heCheckboxConditions, heCheckboxSubscriptions }.Any(x => x.IsNullOrVoid()))
            {
                Logger.Write($"Не найдены элементы для обработки счетчика...", LoggerType.Warning, true, true, true, LogColor.Yellow);

                var heElements = new List<string>();

                if (heFieldCounterName.IsNullOrVoid()) heElements.Add(xpathFieldCounterName.FormatXPathForLog());
                if (heFieldCounterSite.IsNullOrVoid()) heElements.Add(xpathFieldCounterSite.FormatXPathForLog());
                if (heFieldEmail.IsNullOrVoid()) heElements.Add(xpathFieldEmail.FormatXPathForLog());
                if (heCheckboxConditions.IsNullOrVoid()) heElements.Add(xpathCheckboxConditions.FormatXPathForLog());
                if (heCheckboxSubscriptions.IsNullOrVoid()) heElements.Add(xpathCheckboxSubscriptions.FormatXPathForLog());
                if (heButtonSubmit.IsNullOrVoid()) heElements.Add(xpathButtonSubmit.FormatXPathForLog());

                Logger.ErrorAnalysis(true, true, true, new List<string>
                {
                    Instance.ActiveTab.URL,
                    $"Не найдены элементы для обработки счетчика...",
                    string.Join(Environment.NewLine, heElements),
                    string.Empty
                });

                // Закрыть все ненужные вкладки
                CloseAllTabsExceptChannel();

                return null;
            }

            // Заполнение полей
            heFieldCounterName.SetValue(Instance.ActiveTab, ZenChannel, LevelEmulation.SuperEmulation);
            heFieldCounterSite.SetValue(Instance.ActiveTab, ZenChannel, LevelEmulation.SuperEmulation);
            heFieldEmail.SetValue(Instance.ActiveTab, email, LevelEmulation.SuperEmulation);

            // Принятие условий пользовательского соглашения
            if (heCheckboxConditions.GetAttribute("aria-checked").Contains("false"))
                heCheckboxConditions.Click(Instance.ActiveTab, Rnd.Next(150, 500));

            // Подтверждение на принятие рекламных сообщений от яндекс
            if (heCheckboxSubscriptions.GetAttribute("aria-checked").Contains("false"))
                heCheckboxSubscriptions.Click(Instance.ActiveTab, Rnd.Next(150, 500));

            // Создание счетчика
            heButtonSubmit.Click(Instance.ActiveTab, Rnd.Next(2000, 3000));

            // Получение ID счетчика для метрики
            counterIdForMetrika = Regex.Match(Instance.ActiveTab.DomText, "(?<=\"counter\":\\{\"id\":)[0-9]+(?=,)").Value;

            if (string.IsNullOrWhiteSpace(counterIdForMetrika))
                counterIdForMetrika = Regex.Match(Instance.ActiveTab.URL, @"(?<=metrika\.yandex\.ru/onboarding/)[0-9]+(?=\?)").Value;

            // Извлечение ID счетчика
            if (string.IsNullOrWhiteSpace(counterIdForMetrika))
            {
                Logger.Write($"ID счетчика для метрики не найден", LoggerType.Warning, true, true, true, LogColor.Yellow);

                // Закрыть все ненужные вкладки
                CloseAllTabsExceptChannel();

                return null;
            }

            Logger.Write($"[ID: {counterIdForMetrika}]\tНомер счетчика для метрики успешно получен", LoggerType.Info, true, false, true);

            return counterIdForMetrika;
        }

        /// <summary>
        /// Закрытие всех вкладок, кроме вкладки с каналом.
        /// </summary>
        private void CloseAllTabsExceptChannel() =>
            Instance.AllTabs.ToList().ForEach(x => { if (x.URL != ZenChannel) { x.Close(); } });

        /// <summary>
        /// Принятие пользовательского соглашения.
        /// </summary>
        /// <returns></returns>
        private AcceptTermsOfUserAgreementData AcceptTermsOfUserAgreement()
        {
            var acceptTermsOfUserAgreementData = new AcceptTermsOfUserAgreementData();

            var xpathCheckElement = new[] { "//div[contains(@class, 'profile-agreement')]/descendant::label[contains(@class, 'checkbox') and contains(@class, 'with-label')]", "Лейбл - Я принимаю условия пользовательского соглашения" };
            var xpathChildCheckbox = new[] { ".//input[contains(@class, 'checkbox')]", "Чекбокс - Я принимаю условия пользовательского соглашения" };

            var setted = default(bool);
            var counterAttempts = default(int);

            while (true)
            {
                // Счетчик попыток
                if (++counterAttempts > 3)
                {
                    Logger.Write($"Не удалось поставить чекбокс \"Я принимаю условия пользовательского соглашения\"", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    Logger.ErrorAnalysis(true, true, true, new List<string>
                    {
                        Instance.ActiveTab.URL,
                        $"Не удалось поставить чекбокс \"Я принимаю условия пользовательского соглашения\"",
                        string.Empty
                    });
                    return null;
                }

                // Получение элементов
                var heCheckElement = Instance.FindFirstElement(xpathCheckElement, false, true, 5);
                var heChildCheckbox = heCheckElement.FindChildByXPath(xpathChildCheckbox[0], 0);

                // Проверка наличия элемента
                if (new[] { heCheckElement, heChildCheckbox }.Any(x => x.IsNullOrVoid()))
                {
                    Logger.Write($"Не найден какой-то элемент для принятия пользовательского соглашения...", LoggerType.Warning, true, true, true, LogColor.Yellow);

                    var heElements = new List<string>();

                    if (heCheckElement.IsNullOrVoid()) heElements.Add(xpathCheckElement.FormatXPathForLog());
                    if (heChildCheckbox.IsNullOrVoid()) heElements.Add(xpathChildCheckbox.FormatXPathForLog());

                    Logger.ErrorAnalysis(true, true, true, new List<string>
                    {
                        Instance.ActiveTab.URL,
                        $"Не найден какой-то элемент для принятия пользовательского соглашения...",
                        string.Join(Environment.NewLine, heElements),
                        string.Empty
                    });

                    return null;
                }

                if (!heCheckElement.GetAttribute("class").ToLower().Contains("is-checked"))
                {
                    heChildCheckbox.Click(Instance.ActiveTab, Rnd.Next(1500, 3000));
                    setted = true;
                }
                else
                {
                    Logger.Write(setted ? $"Пользовательское соглашение успешно принято" : $"Пользовательское соглашение уже была принято", LoggerType.Info, true, false, true);

                    acceptTermsOfUserAgreementData.ActionsStatus = true;
                    acceptTermsOfUserAgreementData.TimeAction = new TimeData();

                    return acceptTermsOfUserAgreementData;
                }
            }
        }

        /// <summary>
        /// Получение, обработка и установка данных перед запуском.
        /// </summary>
        /// <returns></returns>
        private bool ResourceHandler()
        {
            var accountsCount = AccountsTable.RowCount;

            for (int row = 0; row < accountsCount; row++)
            {
                // Получение аккаунта, настройка до.лога, информация о директории и файле описания аккаунта
                Login = AccountsTable.GetCell((int)TableColumnEnum.Inst.Login, row);
                ObjectDirectory = new DirectoryInfo($@"{Zenno.Directory}\Accounts\{Login}");


                // Проверка на наличия ресурса и его занятость
                if (!ResourceIsAvailable(Login, row)) continue;

                // Получение пароля
                Password = AccountsTable.GetCell((int)TableColumnEnum.Inst.Password, row);

                if (string.IsNullOrWhiteSpace(Password))
                {
                    if (TableGeneralAndTableModeIsSame)
                    {
                        Logger.Write($"[Row: {row + 2}]\tОтсутствует пароль", LoggerType.Warning, true, true, true, LogColor.Yellow);
                        continue;
                    }

                    Password = AccountsGeneralTable.GetCell((int)TableColumnEnum.Inst.Login, Login, (int)TableColumnEnum.Inst.Password);

                    if (string.IsNullOrWhiteSpace(Password))
                    {
                        Logger.Write($"[Row: {row + 2}]\tОтсутствует пароль", LoggerType.Warning, true, true, true, LogColor.Yellow);
                        continue;
                    }
                }

                // Получение ответа на секретный вопрос
                Answer = AccountsTable.GetCell((int)TableColumnEnum.Inst.Answer, row);

                if (string.IsNullOrWhiteSpace(Answer))
                {
                    if (TableGeneralAndTableModeIsSame)
                    {
                        Logger.Write($"[Row: {row + 2}]\tОтсутствует ответ на контрольный вопрос", LoggerType.Warning, true, true, true, LogColor.Yellow);
                        continue;
                    }

                    Answer = AccountsGeneralTable.GetCell((int)TableColumnEnum.Inst.Login, Login, (int)TableColumnEnum.Inst.Answer);

                    if (string.IsNullOrWhiteSpace(Answer))
                    {
                        Logger.Write($"[Row: {row + 2}]\tОтсутствует ответ на контрольный вопрос", LoggerType.Warning, true, true, true, LogColor.Yellow);
                        continue;
                    }
                }

                // Проверка наличия zen канала
                ZenChannel = AccountsTable.GetCell((int)TableColumnEnum.Inst.ZenChannel, row);

                if (string.IsNullOrWhiteSpace(ZenChannel))
                {
                    ZenChannel = AccountsGeneralTable.GetCell((int)TableColumnEnum.Inst.Login, Login, (int)TableColumnEnum.Inst.ZenChannel);

                    if (!string.IsNullOrWhiteSpace(ZenChannel))
                    {
                        Logger.Write($"В таблице найден канал: {ZenChannel}", LoggerType.Info, false, false, true);
                        _channelAlreadyCreated = true;
                    }
                }
                else if (!TableGeneralAndTableModeIsSame)
                {
                    Logger.Write($"В таблице найден канал: {ZenChannel}", LoggerType.Info, false, false, false);
                    _channelAlreadyCreated = true;
                }

                // Преобразование ссылки дзен канала в ссылку профиля канала, если канал есть
                if (_channelAlreadyCreated)
                {
                    ZenChannelProfile = Regex.Match(ZenChannel, @"(?<=zen\.yandex\.[a-z]+/id/).*$").Value;

                    if (string.IsNullOrWhiteSpace(ZenChannelProfile))
                    {
                        Logger.Write($"Не удалось преобразовать ссылку \"ZenChannel\" в \"ZenChannelProfile\", возможно, ссылка на канал заполнена некорректно", LoggerType.Info, true, true, true, LogColor.Yellow);
                        continue;
                    }
                    else ZenChannelProfile = $"https://zen.yandex.{Domain}/profile/editor/id/{ZenChannelProfile}";
                }

                // Проверка директории на существование (создать, если требуется)
                if (!ResourceDirectoryExists()) continue;

                // Получение и загрузка профиля
                if (!ProfileWorker_obsolete.LoadProfile(true)) continue;

                // Получение прокси
                if (!SetProxy((int)TableColumnEnum.Inst.Proxy, row, true)) continue;

                // Успешное получение ресурса
                Program.AddObjectToCache(Login, true, true);
                Logger.Write($"[Proxy table: {Proxy} | Proxy country: {IpInfo.CountryShortName} — {IpInfo.CountryFullName}]\t[Row: {row + 2}]\tАккаунт успешно подключен", LoggerType.Info, true, false, true);
                return true;
            }

            // Не удалось получить ресурс
            Program.ResetExecutionCounter(Zenno);
            Logger.Write($"Отсутствуют свободные/подходящие аккаунты", LoggerType.Info, false, true, true, LogColor.Violet);
            return false;

        }

    }
}