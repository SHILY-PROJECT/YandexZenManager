using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.CommandCenter;
using Global.ZennoExtensions;
using Yandex.Zen.Core.Toolkit;
using Yandex.Zen.Core.Toolkit.Extensions;
using Yandex.Zen.Core.Toolkit.Macros;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using Yandex.Zen.Core.Toolkit.Extensions.Enums;
using Yandex.Zen.Core.Services.WalkingProfileService.Enums;
using Yandex.Zen.Core.Toolkit.BrowserCustomizer;
using Yandex.Zen.Core.Toolkit.BrowserCustomizer.Enums;

namespace Yandex.Zen.Core.Services.WalkingProfileService
{
    public class WalkingProfile : ServicesDataAndComponents
    {
        private static readonly object _locker = new object();

        private readonly ProfileWalkingMode _walkingMode;
        private readonly SourceSearchKeysTypeEnum _sourceSearchKeysType;
        private readonly InstanceSettings.BusySettings _individualStateBusy;
        private readonly bool _individualStateBusyEnabled;
        private SaveProfileModeEnum _saveMode;

        /// <summary>
        /// Конструктор для скрипта (настройка лога).
        /// </summary>
        public WalkingProfile()
        {
            // Нагуливание профилей - Конвертация режима обработки профилей
            var statusProfileWalkingMode = new Dictionary<string, ProfileWalkingMode>()
            {
                {"Нагуливать новые профиля", ProfileWalkingMode.WalkingNewProfile},
                {"Нагуливать имеющиеся профиля", ProfileWalkingMode.WalkingOldProfile}
            }
            .TryGetValue(Zenno.Variables["cfgModeWalkingProfile"].Value, out _walkingMode);

            if (!statusProfileWalkingMode)
            {
                Logger.Write($"Не удалось определить режим нагуливания профилей", LoggerType.Warning, false, true, true, LogColor.Yellow);
                return;
            }

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

            // Индивидуальные настройки инстанса
            var individualSettings = Zenno.Variables["cfgIndividualSettingsForWalkingProfiles"].Value;

            if (individualSettings.Contains("Индивидуальные настройки инстанса"))
            {
                var otherSettings = InstanceSettings.OtherSettings.ExtractOtherSettingsFromVariable(Zenno.Variables["cfgIndividualInstanceSettingsForWalkingProfile"].Value);

                InstanceSettings.OtherSettings.SetOtherSettings(otherSettings);
            }

            // Индивидуальные настройки состояния занятости
            _individualStateBusyEnabled = individualSettings.Contains("Индивидуальные настройки состояния занятости на сайтах");

            if (_individualStateBusyEnabled)
                _individualStateBusy = InstanceSettings.BusySettings.ExtractBusySettingsFromVariable(Zenno.Variables["cfgPolicyOfIgnoringForWalkingProfile"].Value);
        }

        /// <summary>
        /// Старт работы скрипта.
        /// </summary>
        public void Start()
        {
            var searchServices = new List<SearchServiceEnum>();

            var minSizeProfile = int.Parse(Zenno.Variables["cfgMinSizeProfile"].Value);
            var numbKeyUse = Zenno.Variables["cfgNumbKeyUse"].Value;
            var numbSearchPagesView = Zenno.Variables["cfgNumbSearchPagesView"].Value;
            var searchServicesToUse = Zenno.Variables["cfgSearchServicesToUse"].Value;
            var addCountryProfileToProfileName = bool.Parse(Zenno.Variables["cfgAddCountryProfileToProfileName"].Value);

            _saveMode = new Dictionary<string, SaveProfileModeEnum>
            {
                ["Сохранять профиль после обработки каждого сайта"] = SaveProfileModeEnum.SaveAfterEverySite,
                ["Сохранять профиль только по завершению всей работы"] = SaveProfileModeEnum.SaveOnTaskCompletion,
                ["Сохранять профиль после обработки каждой поисковой системы"] = SaveProfileModeEnum.SaveAfterEverySearchSystem
            }
            [Zenno.Variables["cfgSaveProfileModeForWalkingProfile"].Value];

            if (string.IsNullOrWhiteSpace(searchServicesToUse))
            {
                Logger.Write($"Не выбрано ни одной поисковой системы для нагуливания профилей", LoggerType.Warning, false, true, true, LogColor.Yellow);
                return;
            }

            Instance.UseFullMouseEmulation = false;

            if (!ProjectKeeper.ProfilesDirectory.Exists) ProjectKeeper.ProfilesDirectory.Create();

            long oldSize = 0, newSize = 0;

            lock (_locker)
            {
                switch (_walkingMode)
                {
                    case ProfileWalkingMode.WalkingNewProfile:
                        var countryProfile = addCountryProfileToProfileName ? $"   {Zenno.Profile.Country}" : "";

                        ProfileInfo = new FileInfo($@"{ProjectKeeper.ProfilesDirectory.FullName}\profile{countryProfile}   {DateTime.Now:yyyy-MM-dd   HH-mm-ss---fffffff}.zpprofile");

                        Program.AddResourceToCache(ProfileInfo.FullName, true, true);

                        Logger.SetCurrentObjectForLogText(ProfileInfo.Name);
                        Logger.Write($"Нагуливание нового профиля", LoggerType.Info, false, false, true);

                        break;
                    case ProfileWalkingMode.WalkingOldProfile:
                        var profiles = ProjectKeeper.ProfilesDirectory.EnumerateFiles("*.zpprofile", SearchOption.TopDirectoryOnly).ToList();

                        if (profiles.Count == 0)
                        {
                            Logger.Write($"[Нагуливать имеющиеся профиля]\tПрофиля в папке отсутствуют", LoggerType.Info, false, false, true, LogColor.Violet);
                            return;
                        }

                        profiles = profiles.Where(x => x.Length / 1024 < minSizeProfile).ToList();

                        while (true)
                        {
                            if (profiles.Count == 0)
                            {
                                Logger.Write($"[Нагуливать имеющиеся профиля]\t[Минимальный размер профиля: {minSizeProfile} КБ]\tНет профилей которым требуется нагулка", LoggerType.Info, false, false, true, LogColor.Violet);
                                Program.ResetExecutionCounter(Zenno);
                                return;
                            }

                            var profile = profiles.First();

                            if (!Program.CheckResourceInWork(profile.FullName))
                            {
                                Program.AddResourceToCache(profile.FullName, true, true);

                                ProfileInfo = profile;
                                oldSize = ProfileInfo.Length / 1024;

                                break;
                            }
                            else profiles.RemoveAt(0);
                        }

                        Zenno.Profile.Load(ProfileInfo.FullName, true);

                        Logger.SetCurrentObjectForLogText(ProfileInfo.Name);
                        Logger.Write($"[Размер профиля: {ProfileInfo.Length / 1024} KB]\tПрофиль взят на догуливание", LoggerType.Info, false, false, true);

                        break;
                }
                Thread.Sleep(50);
            }

            // Добавляем поисковые сисемы по которым гулять
            if (searchServicesToUse.Contains("google.com")) searchServices.Add(SearchServiceEnum.Google);
            if (searchServicesToUse.Contains("rambler.ru")) searchServices.Add(SearchServiceEnum.Rambler);
            if (searchServicesToUse.Contains("yandex.ru")) searchServices.Add(SearchServiceEnum.YandexRu);
            if (searchServicesToUse.Contains("yandex.com")) searchServices.Add(SearchServiceEnum.YandexCom);

            // Идем гулять
            searchServices.ForEach(service =>
            {
                GoWalkingByService(service, GetKeysForSearchService(_sourceSearchKeysType), numbKeyUse.ExtractNumber(), numbSearchPagesView.ExtractNumber());

                // Сохранять профиль по завершению обработки поисковой системы
                if (_saveMode == SaveProfileModeEnum.SaveAfterEverySearchSystem)
                {
                    ProfileWorker.SaveProfile(true);
                    Logger.Write($"[Размер профиля: {ProfileInfo.Length / 1024} КБ]\tСохранение профиля после обработки поисковой системы: {service}", LoggerType.Info, false, false, true, LogColor.Blue);
                }
            });

            // Сохранять профиль по завершению задачи
            if (_saveMode == SaveProfileModeEnum.SaveOnTaskCompletion)
            {
                ProfileWorker.SaveProfile(true);
                Logger.Write($"[Размер профиля: {ProfileInfo.Length / 1024} КБ]\tСохранение профиля по завершению задачи", LoggerType.Info, false, false, true, LogColor.Blue);
            }

            ProfileInfo.Refresh();
            newSize = ProfileInfo.Length / 1024;

            switch (_walkingMode)
            {
                case ProfileWalkingMode.WalkingNewProfile:
                    Logger.Write($"[Размер профиля: {newSize} KB]\tУспешное завершение нагуливания профиля", LoggerType.Info, false, false, true, LogColor.Green);
                    break;
                case ProfileWalkingMode.WalkingOldProfile:
                    Logger.Write($"[Размер профиля: {newSize} KB]\t[Увеличение за догулку: +{newSize - oldSize} КБ]\tУспешное завершение нагуливания профиля", LoggerType.Info, false, false, true, LogColor.Green);
                    break;
            }

            //ClearThreadResource();
        }

        /// <summary>
        /// Получение ключевиков для поисковых запросов.
        /// </summary>
        /// <returns></returns>
        public static List<string> GetKeysForSearchService(SourceSearchKeysTypeEnum sourceSearchKeysType)
        {
            var searchKeys = new List<string>();

            switch (sourceSearchKeysType)
            {
                case SourceSearchKeysTypeEnum.FromSettings:
                    searchKeys = Zenno.Variables["cfgSettingsSearchKeys"].Value.Split(new[] { Environment.NewLine }, StringSplitOptions.None).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                    break;
                case SourceSearchKeysTypeEnum.FromFile:
                    searchKeys = Zenno.Lists["ListSearchKeys"].Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                    break;
            }

            if (searchKeys.Count == 0)
            {
                var textLog = $"[Источник ключевиков: {sourceSearchKeysType}]\tНе найдено ни одного ключевика";
                Logger.Write(textLog, LoggerType.Warning, false, false, true, LogColor.Yellow);
                throw new Exception(textLog);
            }

            searchKeys.Shuffle();
            return searchKeys;
        }

        /// <summary>
        /// Переход к нагуливанию по заданному сервису.
        /// </summary>
        /// <param name="searchService"></param>
        /// <param name="searchKeys"></param>
        /// <param name="numbKeyUse"></param>
        /// <param name="numbSearchPagesView"></param>
        private void GoWalkingByService(SearchServiceEnum searchService, List<string> searchKeys, int numbKeyUse, int numbSearchPagesView)
        {
            new Dictionary<SearchServiceEnum, string>
            {
                {SearchServiceEnum.Google, "https://www.google.com/" },
                {SearchServiceEnum.Rambler, "https://www.rambler.ru/" },
                {SearchServiceEnum.YandexRu, "https://yandex.ru/" },
                {SearchServiceEnum.YandexCom, "https://yandex.com/" }
            }
            .TryGetValue(searchService, out string url);

            new Dictionary<SearchServiceEnum, string>
            {
                { SearchServiceEnum.Google, "//div[@jscontroller!='']/input[@aria-label!='']" },
                { SearchServiceEnum.Rambler, "//form[@action!='']/input[@name='query']" },
                { SearchServiceEnum.YandexRu, "//span[contains(@class, 'search input')]/descendant::input[contains(@class,'input__control')]" },
                { SearchServiceEnum.YandexCom, "//span[contains(@class, 'search input')]/descendant::input[contains(@class,'input__control')]" }
            }
            .TryGetValue(searchService, out string xpathFieldSearch);

            new Dictionary<SearchServiceEnum, string>
            {
                { SearchServiceEnum.Google, "//a[@href!='']/h3[text()!='']" },
                { SearchServiceEnum.Rambler, "//h2[contains(@class, 'item')]/a[@href!='']" },
                { SearchServiceEnum.YandexRu, "//li[@class='serp-item']/descendant::h2/a[@href!='']" },
                { SearchServiceEnum.YandexCom, "//li[@class='serp-item']/descendant::h2/a[@href!='']" }
            }
            .TryGetValue(searchService, out string xpathItemsPage);

            new Dictionary<SearchServiceEnum, string>
            {
                { SearchServiceEnum.Google, "//tr/descendant::td/span[@class!='']/../following-sibling::td/a[@href!='']" },
                { SearchServiceEnum.Rambler, "//span[contains(@class, 'Paging')]/following-sibling::a[@href!='']" },
                { SearchServiceEnum.YandexRu, "//span[contains(@class, 'pager')]/following-sibling::a[@href!='']" },
                { SearchServiceEnum.YandexCom, "//span[contains(@class, 'pager')]/following-sibling::a[@href!='']" }
            }
            .TryGetValue(searchService, out string xpathNextPage);

            Logger.Write($"Переход к нагуливанию", LoggerType.Info, false, false, false, LogColor.Blue);

            var counterKey = 0;

            while (true)
            {
                if (++counterKey > numbKeyUse) return;

                var counterPage = 0;

                // Проверка наличия ключей
                if (searchKeys.Count == 0)
                {
                    Logger.Write($"[Service: {searchService}]\tЗакончились ключи для сервиса", LoggerType.Warning, false, false, true, LogColor.Yellow);
                    return;
                }

                // Закрываем все не нужные вкладки
                if (Instance.AllTabs.Count() != 0) Instance.AllTabs.ToList().ForEach(x => x.Close());

                // Переходим к поисковой системе
                Instance.ActiveTab.Navigate(url, DataLists.ReferenceLinks.GetLine(LineOptions.Random), true);

                var heFieldSearch = Instance.FuncGetFirstHe(xpathFieldSearch, "Поле - Поиск", false, false);

                if (!heFieldSearch.IsNullOrVoid())
                {
                    Logger.Write($"[Service: {searchService}]\tУспешная загрузка сервиса", LoggerType.Info, false, false, true);
                }
                else continue;

                var key = searchKeys.GetLine(LineOptions.RandomWithRemoved);

                heFieldSearch.SetValue(Instance.ActiveTab, key, LevelEmulation.SuperEmulation, Rnd.Next(150, 500), false, false, true, Rnd.Next(150, 500));

                if (Instance.ActiveTab.IsBusy) Instance.ActiveTab.WaitDownloading();

                while (true)
                {
                    var heReCaptcha = Instance.ActiveTab.FindElementByXPath("//input[@id='recaptcha-token' and @value!='']", 0);

                    if (!heReCaptcha.IsNullOrVoid())
                    {
                        Logger.Write($"[Service: {searchService}]\tОбнаружена reCaptcha", LoggerType.Info, false, false, true, LogColor.Yellow);
                        return;
                    }

                    if (++counterPage > numbSearchPagesView) break;

                    // Переход по страницам поисковой системы
                    if (counterPage != 1)
                    {
                        var heNextPage = Instance.FuncGetFirstHe(xpathNextPage, "", false, false);

                        if (heNextPage.IsNullOrVoid()) break;

                        heNextPage.Click(Instance.ActiveTab);
                    }

                    // Переход по элементам страницы (по сайтам)
                    var numbItems = Instance.ActiveTab.FindElementsByXPath(xpathItemsPage).Count;

                    if (numbItems == 0) break;

                    var sourceUrl = Instance.ActiveTab.URL;

                    for (int i = 0; i < numbItems; i++)
                    {
                        // Установка времени ожидания загрузки страницы
                        Instance.ActiveTab.NavigateTimeout = 15;

                        // Индивидуальное состояние занятости
                        if (_individualStateBusyEnabled) InstanceSettings.BusySettings.SetBusySettings(_individualStateBusy);

                        Instance.ActiveTab.FindElementByXPath(xpathItemsPage, i).Click(Instance.ActiveTab);

                        Logger.Write($"[Service: {searchService}]\t[Page: {counterPage} | Item: {i + 1}]\t[{counterKey}][Key: {key}]\tОбработка URL:  {Instance.ActiveTab.URL}", LoggerType.Info, false, false, true);

                        var openSourceTab = false;
                        var scrollIterations = int.Parse(Zenno.Variables["cfgNumbScrollIterations"].Value);

                        // Скроллинг страницы 
                        while (scrollIterations-- > 0)
                        {
                            Instance.ActiveTab.FullEmulationMouseWheel(0, 750);
                            Thread.Sleep(Rnd.Next(100, 150));
                        }

                        var referrer = Instance.ActiveTab.URL;

                        // Закрытие ненужных вкладок
                        Instance.AllTabs.ToList().ForEach(x =>
                        {
                            if (x.URL == sourceUrl) { openSourceTab = true; } else x.Close();
                        });

                        // Состояние занятости по умолчанию
                        if (_individualStateBusyEnabled)
                            InstanceSettings.BusySettings.SetDefaultBusySettings();

                        // Установка времени ожидания загрузки страницы
                        Instance.ActiveTab.NavigateTimeout = 120;

                        // Открытие исходной вкладки
                        if (!openSourceTab)
                        {
                            Instance.ActiveTab.Navigate(sourceUrl, referrer, true);
                            numbItems = Instance.ActiveTab.FindElementsByXPath(xpathItemsPage).Count;
                        }

                        // Сохранение профиля после каждого сайта
                        if (_saveMode == SaveProfileModeEnum.SaveAfterEverySite)
                        {
                            ProfileWorker.SaveProfile(true);
                            Logger.Write($"[Размер профиля: {ProfileInfo.Length / 1024} КБ]\tСохранение профиля после обработки сайта", LoggerType.Info, false, false, true, LogColor.Blue);
                        }
                    }
                }
            }
        }

    }
}