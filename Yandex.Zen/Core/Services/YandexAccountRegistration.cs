using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;
using ZennoLab.InterfacesLibrary.Enums.Log;
using System.Threading;
using ZennoLab.InterfacesLibrary.Enums.Http;
using System.Text.RegularExpressions;
using System.IO;
using Global.ZennoExtensions;
using Yandex.Zen.Core.Tools;
using Yandex.Zen.Core.Enums.Logger;
using Yandex.Zen.Core.Enums.Extensions;
using Yandex.Zen.Core.Tools.Extensions;
using Yandex.Zen.Core.Tools.Macros;
using Yandex.Zen.Core.Models.TableHandler;
using Yandex.Zen.Core.Enums.WalkingProfile;
using Yandex.Zen.Core.Enums.YandexAccountRegistration;
using Yandex.Zen.Core.Enums;
using Yandex.Zen.Core.ServicesCommonComponents;

namespace Yandex.Zen.Core.Services
{

    public class YandexAccountRegistration : ServiceComponents
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
        public YandexAccountRegistration()
        {
            AccountsTable = zenno.Tables["DonorsForRegistration"];

            instance.UseFullMouseEmulation = true;

            _transferDonorWithNewName = bool.Parse(zenno.Variables["cfgTransferDonorWithNewName"].Value);
            ShorDonorNameForLog = bool.Parse(zenno.Variables["cfgShorDonorNameForLog"].Value);

            TableGeneralAccountFile = new FileInfo(zenno.ExecuteMacro(zenno.Variables["cfgPathFileAccounts"].Value));
            TableModeAccountFile = new FileInfo(zenno.ExecuteMacro(zenno.Variables["cfgPathFileDonorsForRegistration"].Value));
            TableGeneralAndTableModeIsSame = TableGeneralAccountFile.FullName.ToLower() == TableModeAccountFile.FullName.ToLower();

            var actionsBeforeRegistration = zenno.Variables["cfgActionsBeforeRegistrationYandexAccount"].Value;
            var skipWalkingOnZen = zenno.Variables["cfgSkipWalkingOnZenForRegistrationYandex"].Value;
            _beforeRegistrationCheckCaptcha = actionsBeforeRegistration.Contains("Перед регой идти в поисковик yandex и разгадывать капчу");
            _beforeRegistrationGoToWalkingZen = actionsBeforeRegistration.Contains("Перед регой идти погулять на zen.yandex");
            _useOnlyThoseAccountsThatHaveWalkedThroughZen = skipWalkingOnZen.Contains("Использовать только те доноры, которые уже гуляли по zen.yandex");
            _skipWalkingOnZenIfWalked = skipWalkingOnZen.Contains("Пропустить гуляне по zen.yandex, если донор уже гулял");

            // Настройки для гуляния по zen.yandex
            //NumbZenItemOpen = zenno.Variables["cfgNumbZenItemOpen"].ExtractNumber();    // количество статей открывать.
            //NumbLikeToZen = zenno.Variables["cfgNumbLikeToZen"].ExtractNumber();        // количество статей лайкать.
            //NumbDislikeToZen = zenno.Variables["cfgNumbDislikeToZen"].ExtractNumber();  // количество статей дизлайкать.
            //StepBetweenItems = zenno.Variables["cfgStepBetweenItems"].Value;            // количество статей пропускать.

            _uploadAvatarAfterRegistration = bool.Parse(zenno.Variables["cfgUploadAvatarToYandex"].Value);            

            var sharedFolderDonors = zenno.ExecuteMacro(zenno.Variables["cfgPathFolderDonors"].Value);

            // Проверяем наличие аккаунтов в таблице
            if (AccountsTable.RowCount == 0)
            {
                Program.StopTemplate(zenno, $"[{TableModeAccountFile.FullName}]\tТаблица с донорами пуста");
                return;
            }

            // Проверяем наличие папки (создаем её, если нужно)
            if (string.IsNullOrWhiteSpace(sharedFolderDonors))
            {
                Program.StopTemplate(zenno, $"Не указана папка с донорами");
                return;
            }
            else _generalFolderDonors = new DirectoryInfo(sharedFolderDonors);

            if (!_generalFolderDonors.Exists)
            {
                if (!CreateFolderResourceIfNotExist)
                {
                    Program.StopTemplate(zenno, $"Указанная общая папка с донорами не существует");
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
            .TryGetValue(zenno.Variables["cfgYandexRegistrationStartPage"].Value, out _registrationStartPage);

            if (!statusConvertStartPage)
            {
                Program.StopTemplate(zenno, $"Не удалось определить режим стартовой страницы регистрации");
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
                .TryGetValue(zenno.Variables["cfgTypeSourceSearchKeys"].Value, out _sourceSearchKeysType);

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
                
                if ((numbOfWalks == 0) || (numbOfWalks != 0 && !_skipWalkingOnZenIfWalked))
                {
                    Logger.Write($"[Действия перед регистрацией]\tПереход на \"zen.yandex\" перед регистрацией для прогулки", LoggerType.Info, true, false, true);

                    new WalkingOnZen(ResourceType.Donor).Start();

                    // Проверка прогулки по yandex.zen (если статус false - разгружаем ресурсы и завершаем работу скрипта)
                    if (!WalkingOnZen.StatusWalkIsGood) return;
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

            var key = WalkingProfile.GetKeysForSearchService(_sourceSearchKeysType).GetLine(LineOptions.RandomWithRemoved);

            var xpathFieldSearch = new[] { "//span[contains(@class, 'search input')]/descendant::input[contains(@class,'input__control')]", "Поле - Поиск" };
            var xpathItemsPage = new[] { "//li[@class='serp-item']/descendant::h2/a[@href!='']", "" };

            foreach(var url in urlList)
            {
                // Переходим к поисковой системе
                instance.ActiveTab.Navigate(url, true);

                AcceptingPrivacyPolicyCookie();

                var heFieldSearch = instance.FuncGetFirstHe(xpathFieldSearch, false, false);

                if (heFieldSearch.IsNullOrVoid())
                {
                    Logger.Write($"[URL: {url}]\tОшибка загрузки", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    return false;
                }

                heFieldSearch.SetValue(instance.ActiveTab, key, LevelEmulation.SuperEmulation, rnd.Next(150, 500), false, false, true, rnd.Next(150, 500));

                var heItems = instance.FuncGetHeCollection(xpathItemsPage, false, false, 5);

                if (heItems == null || heItems.Count == 0)
                {
                    Logger.Write
                    (
                        $"[URL: {url}]\tНе найдено ни одного элемента на сайте\t" +
                        $"КИНЬ МНЕ ПАПКУ И ДАННЫЕ ДОНОРА!\t" +
                        $"[Папка: {ResourceDirectory.FullName}]\t" +
                        $"[Donor: {InstUrl}]\t" +
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
                            instance.ActiveTab.Navigate($"https://yandex.{Domain}/", true);

                            AcceptingPrivacyPolicyCookie();

                            instance.FuncGetFirstHe($"//a[contains(@href, 'https://yandex.{Domain}/all')]", "Все сервисы яндекс", false, false).Click(instance.ActiveTab, rnd.Next(150, 500));

                            instance.FuncGetFirstHe("//div[@class='main']/descendant::a[@id='tab-mail']|//li[contains(@class, 'services')]/descendant::a[contains(@data-id, 'mail')]", "Tab Почта", true, true, 10).Click(instance.ActiveTab, rnd.Next(150, 500));
                            instance.FuncGetFirstHe("//div[contains(@class, 'HeadBanne')]/descendant::a[contains(@href, 'registration')]/span", "Создать аккаунт", true, true, 10).Click(instance.ActiveTab, rnd.Next(150, 500));
                        }
                        catch { continue; }

                        if (!instance.FuncGetFirstHe("//input[@id='firstname']", "Имя", true, true, 7).IsNullOrVoid())
                        {
                            Logger.Write($"Форма регистрации успешно загружена. Переход к заполнению формы", LoggerType.Info, true, false, true);
                            break;
                        }
                    }
                    break;
                case RegistrationStartPageEnum.RegistrationByDirectLink:
                    var urlReg = $"https://passport.yandex.{Domain}/registration/mail";

                    Logger.Write($"[{urlReg}]\tРегистрация через прямую ссылку", LoggerType.Info, true, false, false);

                    instance.ActiveTab.Navigate(urlReg, true);

                    if (!instance.FuncGetFirstHe("//input[@id='firstname']", "Имя", true, true, 7).IsNullOrVoid())
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
                    heFieldFirstName = instance.FuncGetFirstHe("//input[@id='firstname']", "Имя", true, true, 7);
                    heFieldLastName = instance.FuncGetFirstHe("//input[@id='lastname']", "Фамилия");
                    heFieldPassword = instance.FuncGetFirstHe("//input[@id='password']", "Пароль");
                    heFieldPasswordConfirm = instance.FuncGetFirstHe("//input[@id='password_confirm']", "Подтверждение пароля");
                    heFieldLogin = instance.FuncGetFirstHe("//input[@id='login']", "Логин");                  
                    heButtonNoPhone = instance.FuncGetFirstHe("//div[contains(@class, 'no-phone')]/span[text()!='']", "У меня нет телефона");
                    heButtonSubmit = instance.FuncGetFirstHe("//div[contains(@class, 'submit')]/descendant::button[@type='submit']", "Зарегистрироваться");
                }
                catch { continue; }

                heFieldFirstName.SetValue(instance.ActiveTab, _firstName, LevelEmulation.SuperEmulation, rnd.Next(150, 500));
                heFieldLastName.SetValue(instance.ActiveTab, _lastName, LevelEmulation.SuperEmulation, rnd.Next(150, 500));
                heFieldPassword.SetValue(instance.ActiveTab, Password, LevelEmulation.SuperEmulation, rnd.Next(150, 500));
                heFieldPasswordConfirm.Click(instance.ActiveTab, rnd.Next(150, 500));
                heFieldPasswordConfirm.SetValue(instance.ActiveTab, Password, LevelEmulation.SuperEmulation, rnd.Next(150, 500));
                
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

                        heFieldLogin.Click(instance.ActiveTab, rnd.Next(500, 1000));
                        heLoginsList = instance.FuncGetHeCollection(xpathLoginList, false);

                        if (heLoginsList == null) continue;

                        heRandLogin = heLoginsList.GetByNumber(rnd.Next(0, heLoginsList.Count));
                        heRandLogin.Click(instance.ActiveTab, rnd.Next(2000, 2500));

                        if (!string.IsNullOrWhiteSpace(heFieldLogin.GetValue()))
                        {
                            Login = heRandLogin.GetAttribute("data-login");
                            break;
                        }

                        heFieldFirstName.SetValue(instance.ActiveTab, _firstName, LevelEmulation.SuperEmulation, rnd.Next(150, 500));
                    }
                }
                catch { continue; }

                heButtonNoPhone.Click(instance.ActiveTab, rnd.Next(500, 1000));

                try
                {                    
                    heQuestion = instance.FuncGetFirstHe(xpathQuestion);
                    heQuestionOptions = instance.FuncGetHeCollection(xpathQuestionOptions, true, true, 7);
                    heFieldAnswer = instance.FuncGetFirstHe(xpathFieldAnswer);
                }
                catch { continue; }

                heQuestion.SetValue(instance.ActiveTab, heQuestionOptions.GetByNumber(rnd.Next((heQuestionOptions.Count - 1))).GetAttribute("innerhtml"), LevelEmulation.SuperEmulation, rnd.Next(500, 1000), true);
                heQuestion.Click(instance.ActiveTab, rnd.Next(150, 500));
                heFieldAnswer.SetValue(instance.ActiveTab, Answer, LevelEmulation.SuperEmulation, rnd.Next(150, 500));

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

                        heFieldCaptcha = instance.FuncGetFirstHe(xpathFieldCaptcha);
                        heImgCaptcha = instance.FuncGetFirstHe(xpathImgCaptcha, true, true, 10);

                        try
                        {
                            // Отправка капчи на распознавание
                            captcha = CaptchaService.Recognize(heImgCaptcha);

                            // Проверяем результат распознавания
                            if (string.IsNullOrWhiteSpace(captcha)) continue;

                            // Убрать значение в поле капчи, если там что-то есть
                            if (heFieldCaptcha.GetAttribute("value") != "") heFieldCaptcha.SetAttribute("value", "");

                            // Ввод капчи
                            heFieldCaptcha.SetValue(instance.ActiveTab, captcha, LevelEmulation.SuperEmulation, rnd.Next(500, 1000));
                            heButtonSubmit.Click(instance.ActiveTab, rnd.Next(3000, 4000));

                            var counterWaitEndRegistration = 0;

                            instance.ActiveTab.NavigateTimeout = 20;

                            while (true)
                            {
                                if (++counterWaitEndRegistration > 30) throw new Exception($"Превышено время ожидания окончания регистрации");

                                if (!instance.FuncGetFirstHe(xpathCheckElementsGoodRegistration, false, false).IsNullOrVoid())
                                {
                                    // Пропустить предложение о загрузку аватара после регистрации
                                    instance.FuncGetFirstHe(xptahSkipUploadAvatar, false, false, 0).Click(instance.ActiveTab, rnd.Next(1000, 1500)); ;

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

                                if (instance.FuncGetFirstHe(xpathCaptchaError, false, false, 1) != null)
                                {
                                    instance.FuncGetFirstHe(xpathImgCaptcha, true, true, 10).Click(instance.ActiveTab, rnd.Next(1000, 1500));
                                    throw new Exception($"Капча введена неверно");
                                }

                                hePrivacyPolicy = instance.FuncGetFirstHe("//div[contains(@class, 'eula-popup-wrapper')]/descendant::button", "Privacy Policy and Terms of Use", false, false, 1);

                                if (hePrivacyPolicy != null) hePrivacyPolicy.Click(instance.ActiveTab, rnd.Next(150, 500));

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
            var countryProfileAndProxy = $"{zenno.Profile.Country}/{IpInfo.CountryFullName} - {IpInfo.CountryShortName}";

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
            TableHandler.WriteToCellInSharedAndMode(TableColumnEnum.Inst.InstaUrl, InstUrl, new List<InstDataItem>
            {
                new InstDataItem(TableColumnEnum.Inst.Profile, countryProfileAndProxy),
                new InstDataItem(TableColumnEnum.Inst.Login, Login),
                new InstDataItem(TableColumnEnum.Inst.Password, Password),
                new InstDataItem(TableColumnEnum.Inst.Answer, Answer),
                new InstDataItem(TableColumnEnum.Inst.AccountDatetimeCreated, datetime)
            });

            // Сохранение профиля
            ProfileWorker.SaveProfile(true);
        }

        /// <summary>
        /// Копирование донора в папку с аккаунтами, с переименованием (старая папка удаляется после копирования).
        /// </summary>
        private void CopyDonorWithNewName(bool deleteSourceDirectoryAfterCopy)
        {
            DirectoryInfo dirSource, dirTarget;
            string endTextLog;

            dirSource = new DirectoryInfo(ResourceDirectory.FullName);
            
            if (_transferDonorWithNewName)
            {
                dirTarget = new DirectoryInfo($@"{zenno.Directory}\Accounts\{Login}");
                endTextLog = "(с новым именем)";
            }
            else
            {
                dirTarget = new DirectoryInfo($@"{zenno.Directory}\Accounts\{ResourceDirectory.Name}");
                endTextLog = "(с исходным именем)";
            }

            ResourceDirectory = new DirectoryInfo(dirTarget.FullName);

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
                    InstUrl = AccountsTable.GetCell((int)TableColumnEnum.Inst.InstaUrl, row);
                    ResourceDirectory = new DirectoryInfo(Path.Combine(_generalFolderDonors.FullName, $@"{Regex.Match(InstUrl, @"(?<=com/).*?(?=/)").Value}"));

                    Logger.LogResourceText = ShorDonorNameForLog ? $"[Donor: {ResourceDirectory.Name}]\t" : $"[Donor: {InstUrl}]\t";

                    // Проверка на наличия ресурса и его занятость
                    if (!ResourceIsAvailable(InstUrl, row)) continue;

                    // Проверка актуальности донора
                    if (!string.IsNullOrWhiteSpace(AccountsTable.GetCell((int)TableColumnEnum.Inst.Login, row)))
                    {
                        Logger.Write($"[Row: {row + 2}]\tУже использовался для регистрации yandex аккаунта", LoggerType.Info, false, false, false);
                        continue;
                    }

                    // Проверка директории на существование (создать, если требуется)
                    if (!ResourceDirectoryExists())
                    {
                        ResourceDirectory.Refresh();

                        if (!ResourceDirectory.Exists) continue;
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
                    var firstAndLastName = AccountsTable.GetCell((int)TableColumnEnum.Inst.FirstAndLastName, row);

                    if (!string.IsNullOrWhiteSpace(firstAndLastName))
                    {
                        var separators = new[] { ' ', ':', ';' };

                        if (!separators.Any(x => firstAndLastName.Contains(x)))
                        {
                            Logger.Write($"[Row: {row + 2}]\tНекорректное заполнение строки с именем и фамилией. Варианты заполнения ИФ: \"Имя Фамили\", \"Имя:Фамилия\", \"Имя;Фамилия\"", LoggerType.Warning, true, true, true, LogColor.Yellow);
                            continue;
                        }

                        _firstName = firstAndLastName.Split(separators)[0];
                        _lastName = firstAndLastName.Split(separators)[1];

                        if (string.IsNullOrWhiteSpace(_firstName) || string.IsNullOrWhiteSpace(_lastName))
                        {
                            Logger.Write($"[Row: {row + 2}]\tОтсутствует имя или фамилия. Варианты заполнения ИФ: \"Имя Фамили\", \"Имя:Фамилия\", \"Имя;Фамилия\"", LoggerType.Warning, true, true, true, LogColor.Yellow);
                            continue;
                        }
                    }
                    else
                    {
                        Logger.Write($"[Row: {row + 2}]\tЯчейка с именем и фамилией пуста. Варианты заполнения ИФ: \"Имя Фамили\", \"Имя:Фамилия\", \"Имя;Фамилия\"", LoggerType.Warning, true, true, true, LogColor.Yellow);
                        continue;
                    }

                    // Проверка и установка аватара в соответствующее поле
                    if (_uploadAvatarAfterRegistration && !GetAvatar()) continue;

                    // Получение и загрузка профиля
                    if (!ProfileWorker.LoadProfile(true)) return false;

                    // Получение прокси
                    if (!SetProxy((int)TableColumnEnum.Inst.Proxy, row, true)) continue;

                    // Генерация пароля и ответа на контрольный вопрос
                    Password = TextMacros.GenerateString(15, "abcd");
                    Answer = TextMacros.GenerateString(9, "c");

                    // Успешное получение ресурса
                    Program.ResourcesMode.Add(InstUrl);
                    Program.ResourcesAllThreadsInWork.Add(InstUrl);
                    Logger.Write($"[Proxy table: {Proxy} | Proxy country: {IpInfo.CountryShortName} — {IpInfo.CountryFullName}]\t[ИФ: {_firstName} {_lastName}]\t[Row: {row + 2}]\tДонор успешно подключен", LoggerType.Info, true, false, true);
                    return true;
                }

                // Не удалось получить ресурс
                Program.ResetExecutionCounter(zenno);
                Logger.Write($"Отсутствуют свободные/подходящие доноры", LoggerType.Info, false, true, true, LogColor.Violet);
                return false;
            }
        }
    }
}
