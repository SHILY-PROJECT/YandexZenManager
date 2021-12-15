using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using Global.ZennoExtensions;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.Enums.Log;
using Yandex.Zen.Core.Toolkit;
using Yandex.Zen.Core.Toolkit.Extensions;
using Yandex.Zen.Core.Toolkit.Macros;
using Yandex.Zen.Core.Models.TableHandler;
using Yandex.Zen.Core.Enums;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using Yandex.Zen.Core.Toolkit.Extensions.Enums;
using Yandex.Zen.Core.Services.WalkingOnZenService;
using Yandex.Zen.Core.Services.WalkingProfileService;
using Yandex.Zen.Core.Services.WalkingProfileService.Enums;
using Yandex.Zen.Core.Services.AccounRegisterService.Enums;
using Yandex.Zen.Core.Toolkit.BrowserCustomizer;
using Yandex.Zen.Core.Toolkit.BrowserCustomizer.Enums;

namespace Yandex.Zen.Core.Services.AccounRegisterService
{

    public class MainAccounRegister : Obsolete_ServicesDataAndComponents
    {
        private static readonly object _locker = new object();

        private readonly DirectoryInfo _generalFolderDonors;
        private readonly RegistrationStartPageEnum _registrationStartPage;
        private readonly SourceSearchKeysTypeEnum _sourceSearchKeysType;

        private readonly bool _launchIsAllowed;
        private readonly bool _transferDonorWithNewName;
        private readonly bool _beforeRegistrationCheckCaptcha;
        private readonly bool _beforeRegistrationGoToWalkingZen;
        private readonly bool _skipWalkingOnZenIfWalked;
        private readonly bool _uploadAvatarAfterRegistration;
        private readonly bool _useOnlyThoseAccountsThatHaveWalkedThroughZen;

        private string _firstName;
        private string _lastName;

        /// <summary>
        /// Конструктор для скрипта (настройка лога, проверка и установка прочих данных).
        /// </summary>
        public MainAccounRegister()
        {
            AccountsTable = Zenno.Tables["DonorsForRegistration"];

            Instance.UseFullMouseEmulation = true;

            _transferDonorWithNewName = bool.Parse(Zenno.Variables["cfgTransferDonorWithNewName"].Value);
            ShorDonorNameForLog = bool.Parse(Zenno.Variables["cfgShorDonorNameForLog"].Value);

            TableGeneralAccountFile = new FileInfo(Zenno.ExecuteMacro(Zenno.Variables["cfgPathFileAccounts"].Value));
            TableModeAccountFile = new FileInfo(Zenno.ExecuteMacro(Zenno.Variables["cfgPathFileDonorsForRegistration"].Value));
            TableGeneralAndTableModeIsSame = TableGeneralAccountFile.FullName.ToLower() == TableModeAccountFile.FullName.ToLower();

            var actionsBeforeRegistration = Zenno.Variables["cfgActionsBeforeRegistrationYandexAccount"].Value;
            var skipWalkingOnZen = Zenno.Variables["cfgSkipWalkingOnZenForRegistrationYandex"].Value;
            _beforeRegistrationCheckCaptcha = actionsBeforeRegistration.Contains("Перед регой идти в поисковик yandex и разгадывать капчу");
            _beforeRegistrationGoToWalkingZen = actionsBeforeRegistration.Contains("Перед регой идти погулять на zen.yandex");
            _useOnlyThoseAccountsThatHaveWalkedThroughZen = skipWalkingOnZen.Contains("Использовать только те доноры, которые уже гуляли по zen.yandex");
            _skipWalkingOnZenIfWalked = skipWalkingOnZen.Contains("Пропустить гуляне по zen.yandex, если донор уже гулял");

            // Настройки для гуляния по zen.yandex
            //NumbZenItemOpen = zenno.Variables["cfgNumbZenItemOpen"].ExtractNumber();    // количество статей открывать.
            //NumbLikeToZen = zenno.Variables["cfgNumbLikeToZen"].ExtractNumber();        // количество статей лайкать.
            //NumbDislikeToZen = zenno.Variables["cfgNumbDislikeToZen"].ExtractNumber();  // количество статей дизлайкать.
            //StepBetweenItems = zenno.Variables["cfgStepBetweenItems"].Value;            // количество статей пропускать.

            _uploadAvatarAfterRegistration = bool.Parse(Zenno.Variables["cfgUploadAvatarToYandex"].Value);

            var sharedFolderDonors = Zenno.ExecuteMacro(Zenno.Variables["cfgPathFolderDonors"].Value);

            // Проверяем наличие аккаунтов в таблице
            if (AccountsTable.RowCount == 0)
            {
                Program.StopTemplate(Zenno, $"[{TableModeAccountFile.FullName}]\tТаблица с донорами пуста");
                return;
            }

            // Проверяем наличие папки (создаем её, если нужно)
            if (string.IsNullOrWhiteSpace(sharedFolderDonors))
            {
                Program.StopTemplate(Zenno, $"Не указана папка с донорами");
                return;
            }
            else _generalFolderDonors = new DirectoryInfo(sharedFolderDonors);

            if (!_generalFolderDonors.Exists)
            {
                if (!CreateFolderResourceIfNoExist)
                {
                    Program.StopTemplate(Zenno, $"Указанная общая папка с донорами не существует");
                    return;
                }
                else _generalFolderDonors.Create();
            }

            // Регистрация аккаунтов yandex - Конвертация стартовой страницы регистрации
            var statusConvertStartPage = new Dictionary<string, RegistrationStartPageEnum>()
            {
                {"Регистрироваться по прямой ссылке", RegistrationStartPageEnum.RegistrationByDirectLink},
                {"Регистрироваться через поиск", RegistrationStartPageEnum.RegisterThroughSearch}
            }
            .TryGetValue(Zenno.Variables["cfgYandexRegistrationStartPage"].Value, out _registrationStartPage);

            if (!statusConvertStartPage)
            {
                Program.StopTemplate(Zenno, $"Не удалось определить режим стартовой страницы регистрации");
                return;
            }

            if (_beforeRegistrationCheckCaptcha)
            {
                // Нагуливание профилей - Конвертация источника ключевиков
                var statusSourceSearchKeysType = new Dictionary<string, SourceSearchKeysTypeEnum>()
                {
                    {"Ключевики из файла", SourceSearchKeysTypeEnum.FromFile},
                    {"Ключевики из настроек", SourceSearchKeysTypeEnum.FromSettings}
                }
                .TryGetValue(Zenno.Variables["cfgTypeSourceSearchKeys"].Value, out _sourceSearchKeysType);

                if (!statusSourceSearchKeysType)
                {
                    Logger.Write($"Не удалось определить источник ключевиков", LoggerType.Warning, false, true, true, LogColor.Yellow);
                    return;
                }
            }

            lock (_locker) _launchIsAllowed = ResourceHandler();
        }

        /// <summary>
        /// Запуск скрипта.
        /// </summary>
        public void Start()
        {
            if (!_launchIsAllowed) return;

            // Проверка наличия капчи в поиске яндекс перед регистрацией
            if (_beforeRegistrationCheckCaptcha)
            {
                Logger.Write($"[Действия перед регистрацией]\tПереход в поисковик \"yandex.ru\" и \"yandex.com\" перед регистрацией для поиска и разгадывания капчи", LoggerType.Info, true, false, true);

                var statusRecognize = RecognizeCaptchaBeforeRegistration();

                if (!statusRecognize) return;
            }

            // Идти гулять на zen.yandex
            if (_beforeRegistrationGoToWalkingZen)
            {
                var numbOfWalks = Logger.GetAccountLog(LogFilter.WalkingUnixtime).Count;

                if (numbOfWalks == 0 || numbOfWalks != 0 && !_skipWalkingOnZenIfWalked)
                {
                    Logger.Write($"[Действия перед регистрацией]\tПереход на \"zen.yandex\" перед регистрацией для прогулки", LoggerType.Info, true, false, true);

                    new MainWalkingOnZen(ResourceTypeEnum.Donor).Start();

                    // Проверка прогулки по yandex.zen (если статус false - разгружаем ресурсы и завершаем работу скрипта)
                    if (!MainWalkingOnZen.StatusWalkIsGood) return;
                }
            }

            Registration();
        }

        /// <summary>
        /// Переход к поисковой системе перед регистрацией и поиска капчи.
        /// </summary>
        /// <returns></returns>
        private bool RecognizeCaptchaBeforeRegistration()
        {
            var urlList = new List<string>()
            {
                "https://yandex.ru/",
                "https://yandex.com/"
            };

            var key = MainWalkingProfile.GetKeysForSearchService(_sourceSearchKeysType).GetLine(LineOptions.RandomWithRemoved);

            var xpathFieldSearch = new[] { "//span[contains(@class, 'search input')]/descendant::input[contains(@class,'input__control')]", "Поле - Поиск" };
            var xpathItemsPage = new[] { "//li[@class='serp-item']/descendant::h2/a[@href!='']", "" };

            foreach (var url in urlList)
            {
                // Переходим к поисковой системе
                Instance.ActiveTab.Navigate(url, true);

                AcceptingPrivacyPolicyCookie();

                var heFieldSearch = Instance.FindFirstElement(xpathFieldSearch, false, false);

                if (heFieldSearch.IsNullOrVoid())
                {
                    Logger.Write($"[URL: {url}]\tОшибка загрузки", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    return false;
                }

                heFieldSearch.SetValue(Instance.ActiveTab, key, LevelEmulation.SuperEmulation, Rnd.Next(150, 500), false, true, Rnd.Next(150, 500));

                var heItems = Instance.FindElements(xpathItemsPage, false, false, 5);

                if (heItems == null || heItems.Count == 0)
                {
                    Logger.Write
                    (
                        $"[URL: {url}]\tНе найдено ни одного элемента на сайте\t" +
                        $"КИНЬ МНЕ ПАПКУ И ДАННЫЕ ДОНОРА!\t" +
                        $"[Папка: {ObjectDirectory.FullName}]\t" +
                        $"[Donor: {InstagramUrl}]\t" +
                        $"[Proxy: {Proxy}]",
                        LoggerType.Warning, true, true, true, LogColor.Red
                    );

                    return false;
                }
            }

            Logger.Write($"Проверка \"yandex.ru\" и \"yandex.com\" на наличие капичи успешно пройдена", LoggerType.Info, true, false, true);
            return true;
        }

        private int CounterAttemptsRegistration { get; set; } = 0;

        /// <summary>
        /// Регистрация аккаунта.
        /// </summary>
        private void Registration()
        {
            var xpathLoginList = new[] { "//ul[@class='logins__list']/descendant::label[@data-login!='']", "Список предлагаемых логинов" };
            var xpathQuestion = new[] { "//select[contains(@name, 'question')]", "Селектор контрольных вопросов" };
            var xpathQuestionOptions = new[] { "//select[contains(@name, 'question')]/option", "Варианты контрольных вопросов" };
            var xpathFieldAnswer = new[] { "//input[@id='hint_answer']", "Ответ на контрольный вопрос" };
            var xpathFieldCaptcha = new[] { "//div[@class='captcha-wrapper']/descendant::input[@name='captcha']", "Поле ввода капчи" };
            var xpathImgCaptcha = new[] { "//div[@class='captcha-wrapper']/descendant::img[@src!='']", "Изображение капчи" };
            var xpathCaptchaError = new[] { "//div[@class='captcha-wrapper']/descendant::div[@class='error-message']", "" };
            var xpathCheckElementsGoodRegistration = new[] { "//div[@class='personal']/descendant::span[@class='avatar']|//div[contains(@class, 'avatar-block')]/descendant::img[contains(@id, 'avatar')]", "" };
            var xptahSkipUploadAvatar = new[] { "//div[contains(@class, 'registration') and contains(@class, 'avatar-wrapper')]/descendant::span[contains(@class, 'avatar-btn')]/descendant::a[@href!='']", "Кнопка - Пропустить загрузку аватара" };

            var numbAttemptsRegistration = 3;
            var counterAttemptsSearchHe = 0;

            if (++CounterAttemptsRegistration > numbAttemptsRegistration)
            {
                Logger.Write($"Израсходован лимит попыток регистрации", LoggerType.Warning, true, true, true, LogColor.Yellow);
                return;
            }

            // Переход к странице регистрации через поиск, либо напрямую
            switch (_registrationStartPage)
            {
                case RegistrationStartPageEnum.RegisterThroughSearch:
                    Logger.Write($"Регистрация через переход из поисковой системы", LoggerType.Info, true, false, false);

                    var counterAttemptsIterations = 0;

                    while (true)
                    {
                        if (++counterAttemptsIterations > 3) Registration();

                        try
                        {
                            Instance.ActiveTab.Navigate($"https://yandex.{Domain}/", true);

                            AcceptingPrivacyPolicyCookie();

                            Instance.FindFirstElement($"//a[contains(@href, 'https://yandex.{Domain}/all')]", "Все сервисы яндекс", false, false).Click(Instance.ActiveTab, Rnd.Next(150, 500));

                            Instance.FindFirstElement("//div[@class='main']/descendant::a[@id='tab-mail']|//li[contains(@class, 'services')]/descendant::a[contains(@data-id, 'mail')]", "Tab Почта", true, true, 10).Click(Instance.ActiveTab, Rnd.Next(150, 500));
                            Instance.FindFirstElement("//div[contains(@class, 'HeadBanne')]/descendant::a[contains(@href, 'registration')]/span", "Создать аккаунт", true, true, 10).Click(Instance.ActiveTab, Rnd.Next(150, 500));
                        }
                        catch { continue; }

                        if (!Instance.FindFirstElement("//input[@id='firstname']", "Имя", true, true, 7).IsNullOrVoid())
                        {
                            Logger.Write($"Форма регистрации успешно загружена. Переход к заполнению формы", LoggerType.Info, true, false, true);
                            break;
                        }
                    }
                    break;
                case RegistrationStartPageEnum.RegistrationByDirectLink:
                    var urlReg = $"https://passport.yandex.{Domain}/registration/mail";

                    Logger.Write($"[{urlReg}]\tРегистрация через прямую ссылку", LoggerType.Info, true, false, false);

                    Instance.ActiveTab.Navigate(urlReg, true);

                    if (!Instance.FindFirstElement("//input[@id='firstname']", "Имя", true, true, 7).IsNullOrVoid())
                        Logger.Write($"Форма регистрации успешно загружена. Переход к заполнению формы", LoggerType.Info, true, false, true);
                    break;
            }

            while (true)
            {
                AcceptingPrivacyPolicyCookie();

                if (++counterAttemptsSearchHe > 3) Registration();

                string captcha;
                HtmlElement heButtonSubmit, heFieldFirstName, heFieldLastName, heFieldPassword, heFieldPasswordConfirm, heFieldLogin, heButtonNoPhone, heQuestion, heFieldAnswer, heFieldCaptcha, heImgCaptcha, heRandLogin, hePrivacyPolicy;
                HtmlElementCollection heLoginsList, heQuestionOptions;

                try
                {
                    heFieldFirstName = Instance.FindFirstElement("//input[@id='firstname']", "Имя", true, true, 7);
                    heFieldLastName = Instance.FindFirstElement("//input[@id='lastname']", "Фамилия");
                    heFieldPassword = Instance.FindFirstElement("//input[@id='password']", "Пароль");
                    heFieldPasswordConfirm = Instance.FindFirstElement("//input[@id='password_confirm']", "Подтверждение пароля");
                    heFieldLogin = Instance.FindFirstElement("//input[@id='login']", "Логин");
                    heButtonNoPhone = Instance.FindFirstElement("//div[contains(@class, 'no-phone')]/span[text()!='']", "У меня нет телефона");
                    heButtonSubmit = Instance.FindFirstElement("//div[contains(@class, 'submit')]/descendant::button[@type='submit']", "Зарегистрироваться");
                }
                catch { continue; }

                heFieldFirstName.SetValue(Instance.ActiveTab, _firstName, LevelEmulation.SuperEmulation, Rnd.Next(150, 500));
                heFieldLastName.SetValue(Instance.ActiveTab, _lastName, LevelEmulation.SuperEmulation, Rnd.Next(150, 500));
                heFieldPassword.SetValue(Instance.ActiveTab, Password, LevelEmulation.SuperEmulation, Rnd.Next(150, 500));
                heFieldPasswordConfirm.Click(Instance.ActiveTab, Rnd.Next(150, 500));
                heFieldPasswordConfirm.SetValue(Instance.ActiveTab, Password, LevelEmulation.SuperEmulation, Rnd.Next(150, 500));

                try
                {
                    var counterAttemptsSetLogin = 0;

                    while (true)
                    {
                        if (++counterAttemptsSetLogin > 3)
                        {
                            Logger.Write($"[Proxy table: {Proxy} | Proxy country: {IpInfo.CountryShortName} — {IpInfo.CountryFullName}]\tИзрасходованы попытки установки логина", LoggerType.Warning, true, true, true, LogColor.Yellow);
                            throw new Exception();
                        }

                        heFieldLogin.Click(Instance.ActiveTab, Rnd.Next(500, 1000));
                        heLoginsList = Instance.FindElements(xpathLoginList, false);

                        if (heLoginsList == null) continue;

                        heRandLogin = heLoginsList.GetByNumber(Rnd.Next(0, heLoginsList.Count));
                        heRandLogin.Click(Instance.ActiveTab, Rnd.Next(2000, 2500));

                        if (!string.IsNullOrWhiteSpace(heFieldLogin.GetValue()))
                        {
                            Login = heRandLogin.GetAttribute("data-login");
                            break;
                        }

                        heFieldFirstName.SetValue(Instance.ActiveTab, _firstName, LevelEmulation.SuperEmulation, Rnd.Next(150, 500));
                    }
                }
                catch { continue; }

                heButtonNoPhone.Click(Instance.ActiveTab, Rnd.Next(500, 1000));

                try
                {
                    heQuestion = Instance.FindFirstElement(xpathQuestion);
                    heQuestionOptions = Instance.FindElements(xpathQuestionOptions, true, true, 7);
                    heFieldAnswer = Instance.FindFirstElement(xpathFieldAnswer);
                }
                catch { continue; }

                heQuestion.SetValue(Instance.ActiveTab, heQuestionOptions.GetByNumber(Rnd.Next(heQuestionOptions.Count - 1)).GetAttribute("innerhtml"), LevelEmulation.SuperEmulation, Rnd.Next(500, 1000), true);
                heQuestion.Click(Instance.ActiveTab, Rnd.Next(150, 500));
                heFieldAnswer.SetValue(Instance.ActiveTab, Answer, LevelEmulation.SuperEmulation, Rnd.Next(150, 500));

                // Обработка капчи
                var counterAttemptsRecognitionCaptcha = 0;

                try
                {
                    while (true)
                    {
                        if (++counterAttemptsRecognitionCaptcha > 3)
                        {
                            var textLog = $"Израсходованы попытки разгадывания капчи";
                            Logger.Write(textLog, LoggerType.Warning, true, true, true, LogColor.Yellow);
                            throw new Exception(textLog);
                        }

                        heFieldCaptcha = Instance.FindFirstElement(xpathFieldCaptcha);
                        heImgCaptcha = Instance.FindFirstElement(xpathImgCaptcha, true, true, 10);

                        try
                        {
                            // Отправка капчи на распознавание
                            //captcha = CaptchaService.Recognize(heImgCaptcha);
                            captcha = null;
                            /*
                             * todo Переработать обработку капчи
                            */

                            // Проверяем результат распознавания
                            if (string.IsNullOrWhiteSpace(captcha)) continue;

                            // Убрать значение в поле капчи, если там что-то есть
                            if (heFieldCaptcha.GetAttribute("value") != "") heFieldCaptcha.SetAttribute("value", "");

                            // Ввод капчи
                            heFieldCaptcha.SetValue(Instance.ActiveTab, captcha, LevelEmulation.SuperEmulation, Rnd.Next(500, 1000));
                            heButtonSubmit.Click(Instance.ActiveTab, Rnd.Next(3000, 4000));

                            var counterWaitEndRegistration = 0;

                            Instance.ActiveTab.NavigateTimeout = 20;

                            while (true)
                            {
                                if (++counterWaitEndRegistration > 30) throw new Exception($"Превышено время ожидания окончания регистрации");

                                if (!Instance.FindFirstElement(xpathCheckElementsGoodRegistration, false, false).IsNullOrVoid())
                                {
                                    // Пропустить предложение о загрузку аватара после регистрации
                                    Instance.FindFirstElement(xptahSkipUploadAvatar, false, false, 0).Click(Instance.ActiveTab, Rnd.Next(1000, 1500)); ;

                                    // Сохранение данных аккаунта
                                    SaveAccountData();

                                    Logger.Write($"[Login: {Login}]\tУспешная регистрация аккаунта", LoggerType.Info, true, false, true, LogColor.Green);

                                    // Загрузка аватара
                                    if (AvatarInfo != null)
                                    {
                                        Logger.Write($"Переход к загрузке аватара", LoggerType.Info, true, false, true);
                                        UploadAvatarToYandexPassport();
                                    }

                                    // Копирование и переименование донора, а так же удаление старой папки
                                    CopyDonorWithNewName(true);

                                    return;
                                }

                                if (Instance.FindFirstElement(xpathCaptchaError, false, false, 1) != null)
                                {
                                    Instance.FindFirstElement(xpathImgCaptcha, true, true, 10).Click(Instance.ActiveTab, Rnd.Next(1000, 1500));
                                    throw new Exception($"Капча введена неверно");
                                }

                                hePrivacyPolicy = Instance.FindFirstElement("//div[contains(@class, 'eula-popup-wrapper')]/descendant::button", "Privacy Policy and Terms of Use", false, false, 1);

                                if (hePrivacyPolicy != null) hePrivacyPolicy.Click(Instance.ActiveTab, Rnd.Next(150, 500));

                                Thread.Sleep(1000);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Write($"{ex.Message}", LoggerType.Warning, true, true, true, LogColor.Yellow);
                        }
                    }
                }
                catch { continue; }
            }
        }

        /// <summary>
        /// Сохранить результат в таблицу, резервынй результат, профиль.
        /// </summary>
        private void SaveAccountData()
        {
            var datetime = Logger.GetDateTime(DateTimeFormat.yyyyMMddThreeSpaceHHmmss);
            var countryProfileAndProxy = $"{Zenno.Profile.Country}/{IpInfo.CountryFullName} - {IpInfo.CountryShortName}";

            Logger.MakeBackupData(new List<string>
            {
                $"Profile (profile country/proxy country): {countryProfileAndProxy}",
                $"Login: {Login}",
                $"Password: {Password}",
                $"Answer: {Answer}",
                $"Account datetime created: {datetime}",
                string.Empty
            },
            false);

            // Сохранение результата в таблицу режима и общую таблицу
            TableHandler.WriteToCellInSharedAndMode(TableColumnEnum.Inst.InstaUrl, InstagramUrl, new List<InstDataItem>
            {
                new InstDataItem(TableColumnEnum.Inst.Profile, countryProfileAndProxy),
                new InstDataItem(TableColumnEnum.Inst.Login, Login),
                new InstDataItem(TableColumnEnum.Inst.Password, Password),
                new InstDataItem(TableColumnEnum.Inst.Answer, Answer),
                new InstDataItem(TableColumnEnum.Inst.AccountDatetimeCreated, datetime)
            });

            // Сохранение профиля
            Obsolete_ProfileWorker.SaveProfile(true);
        }

        /// <summary>
        /// Копирование донора в папку с аккаунтами, с переименованием (старая папка удаляется после копирования).
        /// </summary>
        private void CopyDonorWithNewName(bool deleteSourceDirectoryAfterCopy)
        {
            DirectoryInfo dirSource, dirTarget;
            string endTextLog;

            dirSource = new DirectoryInfo(ObjectDirectory.FullName);

            if (_transferDonorWithNewName)
            {
                dirTarget = new DirectoryInfo($@"{Zenno.Directory}\Accounts\{Login}");
                endTextLog = "(с новым именем)";
            }
            else
            {
                dirTarget = new DirectoryInfo($@"{Zenno.Directory}\Accounts\{ObjectDirectory.Name}");
                endTextLog = "(с исходным именем)";
            }

            ObjectDirectory = new DirectoryInfo(dirTarget.FullName);

            var textLog = $"[Старый путь: {dirSource.FullName}]\t[Новый путь: {dirTarget.FullName}]\tДонор эволюционировал в аккаунт и был скопирован в новую директорию {endTextLog}";

            DirectoryMacros.DirectoryCopy(dirSource, dirTarget);

            Logger.WriteToResourceLog(dirSource, textLog, LoggerType.Info);
            Logger.Write(textLog, LoggerType.Info, true, true, true, LogColor.Green);

            // Удаление исходной директории после копирования
            if (deleteSourceDirectoryAfterCopy)
            {
                try
                {
                    dirSource.Delete(true);
                }
                catch (Exception ex)
                {
                    var textLogEx = $"[Старый путь донора: {dirSource.FullName}]\t[Exception message: {Regex.Replace(ex.Message, "\n", " ")}]\tНе удалось удалить папку донора после копирования";

                    // Записываем информацию в старую папку
                    Logger.WriteToResourceLog(dirSource, textLogEx, LoggerType.Warning);

                    // Записываем информацию в новую папку
                    Logger.Write(textLogEx, LoggerType.Warning, true, true, true);
                }
            }
        }

        /// <summary>
        /// Получение, обработка и установка данных перед запуском.
        /// </summary>
        /// <returns></returns>
        private bool ResourceHandler()
        {
            lock (SyncObjects.TableSyncer)
            {
                var accountsCount = AccountsTable.RowCount;

                for (int row = 0; row < accountsCount; row++)
                {
                    // Получение аккаунта, информация о директории и файле описания аккаунта
                    InstagramUrl = AccountsTable.GetCell((int)TableColumnEnum.Inst.InstaUrl, row);
                    ObjectDirectory = new DirectoryInfo(Path.Combine(_generalFolderDonors.FullName, $@"{Regex.Match(InstagramUrl, @"(?<=com/).*?(?=/)").Value}"));

                    if (ShorDonorNameForLog) Logger.SetCurrentResourceForLog(ObjectDirectory.Name, ResourceTypeEnum.Donor);
                    else Logger.SetCurrentResourceForLog(InstagramUrl, ResourceTypeEnum.Donor);

                    // Проверка на наличия ресурса и его занятость
                    if (!ResourceIsAvailable(InstagramUrl, row)) continue;

                    // Проверка актуальности донора
                    if (string.IsNullOrWhiteSpace(AccountsTable.GetCell((int)TableColumnEnum.Inst.Login, row)) is false)
                    {
                        Logger.Write($"[Row: {row + 2}]\tУже использовался для регистрации yandex аккаунта", LoggerType.Info, false, false, false);
                        continue;
                    }

                    // Проверка директории на существование (создать, если требуется)
                    if (ResourceDirectoryExists() is false)
                    {
                        ObjectDirectory.Refresh();
                        if (ObjectDirectory.Exists is false) continue;
                    }

                    // Проверка донора на нагуливание по zen.yandex
                    if (_beforeRegistrationGoToWalkingZen && _useOnlyThoseAccountsThatHaveWalkedThroughZen)
                    {
                        if (Logger.GetAccountLog(LogFilter.WalkingUnixtime).Count == 0)
                        {
                            Logger.Write($"[Row: {row + 2}]\tДонор ещё не гулял по zen.yandex и не допущен до регистрации", LoggerType.Info, true, false, false);
                            continue;
                        }
                    }

                    // Получение имени и фамилии для регистрации
                    var nameAndSurname = AccountsTable.GetCell((int)TableColumnEnum.Inst.FirstAndLastName, row);
                    var recommendation = "Варианты заполнения ИФ: \"Имя Фамили\", \"Имя:Фамилия\", \"Имя;Фамилия\"";

                    if (string.IsNullOrWhiteSpace(nameAndSurname) is false)
                    {
                        var separators = new[] { ' ', ':', ';' };

                        if (separators.Any(x => nameAndSurname.Contains(x)) is false)
                        {
                            Logger.Write($"[Row: {row + 2}]\t'{nameof(nameAndSurname)}' - Некорректное заполнение. {recommendation}", LoggerType.Warning, true, true, true, LogColor.Yellow);
                            continue;
                        }

                        _firstName = nameAndSurname.Split(separators)[0];
                        _lastName = nameAndSurname.Split(separators)[1];

                        if (string.IsNullOrWhiteSpace(_firstName) || string.IsNullOrWhiteSpace(_lastName))
                        {
                            Logger.Write($"[Row: {row + 2}]\t'{nameof(_firstName)}: {_firstName}' '{nameof(_lastName)}: {_lastName}' - Отсутствует значение. {recommendation}",
                                LoggerType.Warning, true, true, true, LogColor.Yellow);
                            continue;
                        }
                    }
                    else
                    {
                        Logger.Write($"[Row: {row + 2}]\tЯчейка с именем и фамилией пуста. {recommendation}", LoggerType.Warning, true, true, true, LogColor.Yellow);
                        continue;
                    }

                    // Проверка и установка аватара в соответствующее поле
                    if (_uploadAvatarAfterRegistration && GetAvatar() is false) continue;

                    // Получение и загрузка профиля
                    if (Obsolete_ProfileWorker.LoadProfile(true) is false) return false;

                    // Получение прокси
                    if (SetProxy((int)TableColumnEnum.Inst.Proxy, row, true) is false) continue;

                    // Генерация пароля и ответа на контрольный вопрос
                    Password = TextMacros.GenerateString(15, "abcd");
                    Answer = TextMacros.GenerateString(9, "c");

                    // Успешное получение ресурса
                    ProjectKeeper.ResourcesCurrentThread.Add(InstagramUrl);
                    ProjectKeeper.ResourcesAllThreadsInWork.Add(InstagramUrl);
                    Logger.Write($"[Proxy table: {Proxy} | Proxy country: {IpInfo.CountryShortName} — {IpInfo.CountryFullName}]\t[ИФ: {_firstName} {_lastName}]\t[Row: {row + 2}]\tДонор успешно подключен", LoggerType.Info, true, false, true);
                    return true;
                }

                // Не удалось получить ресурс
                Program.ResetExecutionCounter(Zenno);
                Logger.Write($"Отсутствуют свободные/подходящие доноры", LoggerType.Info, false, true, true, LogColor.Violet);
                return false;
            }
        }
    }
}
