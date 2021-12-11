using System;
using System.Collections.Generic;
using System.Linq;
using Yandex.Zen.Core.Toolkit;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.Enums.Log;
using Global.ZennoExtensions;
using System.Threading;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;
using Yandex.Zen.Core.Toolkit.Extensions;
using Yandex.Zen.Core.Models.TableHandler;
using Yandex.Zen.Core.Enums;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using Yandex.Zen.Core.Toolkit.Extensions.Enums;
using Yandex.Zen.Core.Services.WalkingOnZenService.Enums;
using Yandex.Zen.Core.Services.Components;
using Yandex.Zen.Core.Toolkit.BrowserCustomizer;
using Yandex.Zen.Core.Toolkit.BrowserCustomizer.Enums;

namespace Yandex.Zen.Core.Services.WalkingOnZenService
{
    public class WalkingOnZen : ServicesDataAndComponents
    {
        private static readonly object _locker = new object();

        [ThreadStatic] public static bool StatusWalkIsGood = default;
        [ThreadStatic] public static int NumbZenItemOpen = default;
        [ThreadStatic] public static int NumbLikeToZen = default;
        [ThreadStatic] public static int NumbDislikeToZen = default;
        [ThreadStatic] public static string StepBetweenItems = default;

        private List<ActionOnItem> _actionList;
        private StartPageWalkingOnZen _startPage;
        private UseObjectTypeInWorkEnum _useResourceTypeInWork;
        private InstanceSettings.BusySettings _individualStateBusy;

        private readonly bool _launchIsAllowed;
        private bool _individualStateBusyEnabled;
        private bool _useAuthorizationForAccounts;
        private bool _likeEnable;
        private bool _dislikeEnable;
        private bool _openArticlesEnable;
        private bool _useStepEnable;
        private bool _timeOnPageEnable;
        private bool _skipErrorLike;
        private bool _skipErrorDislike;
        private bool _skipErrorOpenArticles;
        private bool _enableInfoOnHowMuchTimeIsLeftBeforeWalk;
        private bool _changeGeoIfMenuIsVoid;
        private string _timeOnPageArticleZen;

        public ResourceTypeEnum CurrentObjectAtWork { get; set; }

        /// <summary>
        /// Пустой конструктор для скрипта вызова из других методов.
        /// </summary>
        public WalkingOnZen(ResourceTypeEnum resourceTypeAtWork)
        {
            SetSettingsMode();

            if (_actionList.Count == 0) return;

            _useAuthorizationForAccounts = true;
            CurrentObjectAtWork = resourceTypeAtWork;

            _launchIsAllowed = true;
        }

        /// <summary>
        /// Конструктор для скрипта (настройка лога, проверка и установка прочих данных).
        /// </summary>
        public WalkingOnZen()
        {
            AccountsTable = Zenno.Tables["AccountsForWalkingOnZen"];

            new Dictionary<string, UseObjectTypeInWorkEnum>
            {
                { "Использовать для работы только доноры", UseObjectTypeInWorkEnum.UseOnlyDonor },
                { "Использовать для работы только аккаунты", UseObjectTypeInWorkEnum.UseOnlyAccount },
                { "Использовать для работы доноры и аккаунты", UseObjectTypeInWorkEnum.UseAnyResource }
            }
            .TryGetValue(Zenno.Variables["cfgResourceTypeToUseForWalkingOnZen"].Value, out _useResourceTypeInWork);

            // Получение и сравнение таблицы режима и общей таблицы
            TableGeneralAccountFile = new FileInfo(Zenno.ExecuteMacro(Zenno.Variables["cfgPathFileAccounts"].Value));
            TableModeAccountFile = new FileInfo(Zenno.ExecuteMacro(Zenno.Variables["cfgPathFileAccountsForWalkingOnZen"].Value));
            TableGeneralAndTableModeIsSame = TableGeneralAccountFile.FullName.ToLower() == TableModeAccountFile.FullName.ToLower();

            BindingPhoneToAccountIfRequaid = Zenno.Variables["cfgBindingPhoneIfRequiredForWalkingZenForAccount"].Value.Contains("Привязывать номер");
            _useAuthorizationForAccounts = bool.Parse(Zenno.Variables["cfgAuthorizationForAccountsForWalkingOnZen"].Value);

            var logSettings = Zenno.Variables["cfgLogSettingsForWalkingOnZen"].Value;
            _enableInfoOnHowMuchTimeIsLeftBeforeWalk = logSettings.Contains("Через сколько будет доступна прогулка");

            SetSettingsMode();

            if (_actionList.Count == 0) return;

            lock (_locker) _launchIsAllowed = ResourceHandler();
        }

        /// <summary>
        /// Установка настроек для режима (перменных).
        /// </summary>
        private void SetSettingsMode()
        {
            _actionList = new List<ActionOnItem>();

            _individualStateBusyEnabled = bool.Parse(Zenno.Variables["cfgIndividualPolicyOfIgnoringEnabledForWalkingOnZen"].Value);
            _individualStateBusy = InstanceSettings.BusySettings.ExtractBusySettingsFromVariable(Zenno.Variables["cfgPolicyOfIgnoringForWalkingOnZen"].Value);

            _changeGeoIfMenuIsVoid = bool.Parse(Zenno.Variables["cfgChangeGeoIfMenuIsVoidForWalkingZen"].Value);
            _timeOnPageArticleZen = Zenno.Variables["cfgTimeOnPageArticleForWalkingOnZen"].Value;
            ShorDonorNameForLog = bool.Parse(Zenno.Variables["cfgShorDonorNameForLog"].Value);

            var actionsEnable = Zenno.Variables["cfgUseActionForWalkingOnZen"].Value;
            var actionsSkipError = Zenno.Variables["cfgActionSkipErrorForWalkingOnZen"].Value;

            _likeEnable = actionsEnable.Contains("Ставить лайки");
            _dislikeEnable = actionsEnable.Contains("Ставить дизлайки");
            _openArticlesEnable = actionsEnable.Contains("Открывать статьи");
            _timeOnPageEnable = actionsEnable.Contains("Использовать время гуляния по статье zen.yandex");
            _useStepEnable = actionsEnable.Contains("Использовать \"Шаг\"");
            _skipErrorLike = actionsSkipError.Contains("Пропускать ошибки лайков");
            _skipErrorDislike = actionsSkipError.Contains("Пропускать ошибки дизлайков");
            _skipErrorOpenArticles = actionsSkipError.Contains("Пропускать ошибки статей");

            // Настройки для гуляния по zen.yandex
            NumbZenItemOpen = Zenno.Variables["cfgNumbZenItemOpen"].ExtractNumber();    // количество статей открывать.
            NumbLikeToZen = Zenno.Variables["cfgNumbLikeToZen"].ExtractNumber();        // количество статей лайкать.
            NumbDislikeToZen = Zenno.Variables["cfgNumbDislikeToZen"].ExtractNumber();  // количество статей дизлайкать.
            StepBetweenItems = Zenno.Variables["cfgStepBetweenItems"].Value;            // количество статей пропускать.

            // Добавляем список действий над элементами страницы
            if (_openArticlesEnable) for (int i = 0; i < NumbZenItemOpen; i++) _actionList.Add(ActionOnItem.OpenItem);
            if (_likeEnable) for (int i = 0; i < NumbLikeToZen; i++) _actionList.Add(ActionOnItem.Like);
            if (_dislikeEnable) for (int i = 0; i < NumbDislikeToZen; i++) _actionList.Add(ActionOnItem.Dislike);

            // Перемешиваем список для рандомизации действий
            _actionList.Shuffle();

            new Dictionary<string, StartPageWalkingOnZen>
            {
                { "zen.yandex", StartPageWalkingOnZen.ZenYandex },
                { "zen.yandex/about", StartPageWalkingOnZen.ZenYandexAbout }
            }
            .TryGetValue(Zenno.Variables["cfgStartPageForWalkingZen"].Value, out _startPage);
        }

        /// <summary>
        /// Запуск скрипта (для отдельного режима).
        /// </summary>
        public void Start()
        {
            if (!_launchIsAllowed) return;

            // Начать гуляние по zen.yandex
            GoToWalki();
        }

        /// <summary>
        /// Прогулка по "https://zen.yandex/about" эмитируя пользователя.
        /// </summary>
        /// <returns></returns>
        private void GoToWalki()
        {
            Logger.Write($"Старт метода прогулки по дзену", LoggerType.Info, true, false, true);

            HtmlElement item;

            var xpathButtonGoToZen = new[] { "//div[@id='header']/descendant::a[contains(@href, 'zen.yandex') and @href!=contains(@href, 'media')]/descendant::button|//div[contains(@class, 'body-image')]/descendant::a[contains(@class, 'item')]/descendant::span[contains(@class, 'text')]", "Перейти в Дзен" };
            var xpathItems = new[] { "//div[contains(@class, 'feed__item') and @id!='']/descendant::div[@class='card-wrapper__inner']", "Статьи страницы Дзен" };
            var xpathAvatarProfile = new[] { "//div[contains(@class, 'zen-ui-profile') and contains(@class, 'avatar')]/descendant::button[contains(@class, 'zen-ui-avatar')]", "Кнопка - Аватар профиля" };
            var xpathButtonAuth = new[] { "//div[contains(@class, 'right-items')]/descendant::a[contains(@class, 'auth-header-buttons') and text()='Log in' or text()='Войти']", "Кнопка войти" };

            Instance.UseFullMouseEmulation = true;

            var stopwatchFullProgram = new Stopwatch();
            var counterAttempt = 0;

            while (true)
            {
                if (++counterAttempt > 3)
                {
                    Logger.Write($"Что-то пошло не так... Израсходован лимит попыток прогулки по дзену", LoggerType.Warning, true, false, true, LogColor.Yellow);
                    StatusWalkIsGood = false;
                    return;
                }

                // Переход на стартовую страницу
                switch (_startPage)
                {
                    case StartPageWalkingOnZen.ZenYandex:
                        Instance.ActiveTab.Navigate($"https://zen.yandex.ru/", true);
                        AcceptingPrivacyPolicyCookie();
                        break;
                    case StartPageWalkingOnZen.ZenYandexAbout:
                        Instance.ActiveTab.Navigate("https://zen.yandex/about", true);
                        AcceptingPrivacyPolicyCookie();

                        // Получение кнопки для перехода в дзен
                        var heButtonGoToZen = Instance.FuncGetFirstHe(xpathButtonGoToZen, false, true, 5);

                        // Проверка наличия кнопки для перехода в дзен
                        if (heButtonGoToZen.IsNullOrVoid())
                        {
                            //Logger.LoggerWrite($"", );
                            Logger.ErrorAnalysis(true, true, true, new List<string>
                            {
                                Instance.ActiveTab.URL,
                                xpathButtonGoToZen.XPathToStandardView(),
                                string.Empty
                            });
                            continue;
                        }
                        else heButtonGoToZen.Click(Instance.ActiveTab, Rnd.Next(150, 500));
                        break;
                }

                // Проверка наличия элементов
                if (Instance.FuncGetHeCollection(xpathItems, false, true, 5).Count == 0) continue;

                // Смена geo при отсутствии бокового меню
                if (_changeGeoIfMenuIsVoid)
                {
                    if (!CheckMenuItemIfVoidThenChangeGeo())
                    {
                        StatusWalkIsGood = false;
                        return;
                    }

                    // Проверка наличия элементов
                    if (Instance.FuncGetHeCollection(xpathItems, false, true, 5).Count == 0) continue;
                }

                // Проверка авторизации перед прогулкой (для аккаунта)
                if (_useAuthorizationForAccounts && CurrentObjectAtWork == ResourceTypeEnum.Account && Instance.FuncGetFirstHe(xpathAvatarProfile, false, false).IsNullOrVoid())
                {
                    var heButtonAuth = Instance.FuncGetFirstHe(xpathButtonAuth, false, true);

                    if (heButtonAuth.IsNullOrVoid())
                    {
                        StatusWalkIsGood = false;

                        Logger.Write($"Не найдена кнопка для авторизации...", LoggerType.Warning, true, true, true, LogColor.Yellow);
                        Logger.ErrorAnalysis(true, true, true, new List<string>
                        {
                            Instance.ActiveTab.URL,
                            xpathButtonAuth.XPathToStandardView(),
                            "Не найдена кнопка для авторизации...",
                            string.Empty
                        });

                        return;
                    }

                    heButtonAuth.Click(Instance.ActiveTab, Rnd.Next(500, 1000));

                    // Авторизация
                    var statusAuth = Authorization.Auth(BindingPhoneToAccountIfRequaid, xpathItems);

                    // Проверка авторизации
                    if (!statusAuth)
                    {
                        StatusWalkIsGood = false;
                        return;
                    }
                }

                try
                {
                    var items = Instance.FuncGetHeCollection(xpathItems);
                    var numbItem = default(int);
                    var numbStepItem = default(int);

                    if (stopwatchFullProgram.ElapsedMilliseconds == 0) stopwatchFullProgram.Start();

                    while (true)
                    {
                        // Установка времени ожидания загрузки страницы
                        Instance.ActiveTab.NavigateTimeout = 20;

                        if (_actionList.Count == 0) break;

                        // Проверка наличия панели оценки дзен
                        CheckZenAppraisalBar();

                        if (numbItem != 0)
                        {
                            // Использовать шаг
                            if (_useStepEnable)
                            {
                                numbStepItem = StepBetweenItems.ExtractNumber();

                                if (numbStepItem != 0)
                                {
                                    numbItem += numbStepItem;
                                }
                                else numbItem++;
                            }
                            else numbItem++;

                            numbItem = numbItem > items.Count ? items.Count - 1 : numbItem;
                        }

                        item = items.GetByNumber(numbItem);
                        Instance.ActiveTab.FullEmulationMouseMoveAboveHtmlElement(item, Rnd.Next(120, 150), true);

                        if (numbItem == 0) numbItem++;

                        // Выполнить событие
                        if (!GoAction(item, _actionList.First(), numbItem))
                        {
                            ScrollToCurrentLastPosition(xpathItems[0], numbItem);
                            continue;
                        }
                        else
                        {
                            _actionList.RemoveAt(0);
                            ScrollToCurrentLastPosition(xpathItems[0], numbItem);
                        }

                        items = Instance.ActiveTab.FindElementsByXPath(xpathItems[0]);
                    }

                    // Остановить счетчик
                    stopwatchFullProgram.Stop();

                    ProfileWorker.SaveProfile(true);

                    // Сохранение результата в таблицу режима и общую таблицу
                    switch (CurrentObjectAtWork)
                    {
                        case ResourceTypeEnum.Donor:
                            TableHandler.WriteToCellInSharedAndMode(TableColumnEnum.Inst.InstaUrl, InstagramUrl, new InstDataItem(TableColumnEnum.Inst.DatetimeLastWalkingOnZen, Logger.GetDateTime(DateTimeFormat.yyyyMMddThreeSpaceHHmmss)));
                            break;
                        case ResourceTypeEnum.Account:
                            TableHandler.WriteToCellInSharedAndMode(TableColumnEnum.Inst.Login, Login, new InstDataItem(TableColumnEnum.Inst.DatetimeLastWalkingOnZen, Logger.GetDateTime(DateTimeFormat.yyyyMMddThreeSpaceHHmmss)));
                            break;
                    }

                    Logger.Write($"[Walking zen unixtime: {Logger.GetUnixtime()}]\t[{stopwatchFullProgram.ElapsedMilliseconds / 1000 / 60} in min | {stopwatchFullProgram.ElapsedMilliseconds / 1000} in sec]\tПрогулка по дзену успешно завершена", LoggerType.Info, true, false, true, LogColor.Green);
                    StatusWalkIsGood = true;
                    return;
                }
                catch { continue; }
            }
        }

        /// <summary>
        /// Скроллинг к последней рабочей позиции.
        /// </summary>
        /// <param name="xpathItems"></param>
        /// <param name="numbItem"></param>
        private void ScrollToCurrentLastPosition(string xpathItems, int numbItem)
        {
            Instance.ActiveTab.NavigateTimeout = 5;

            HtmlElementCollection items;

            numbItem += 10;

            while (true)
            {
                items = Instance.ActiveTab.FindElementsByXPath(xpathItems);

                if (numbItem > items.Count)
                {
                    items.Last().ScrollIntoView();
                    //instance.ActiveTab.FullEmulationMouseMoveToHtmlElement(items.Last());

                    Thread.Sleep(Rnd.Next(250, 500));

                    if (Instance.ActiveTab.IsBusy)
                        Instance.ActiveTab.WaitDownloading();
                }
                else break;
            }
        }

        /// <summary>
        /// Взаимодействие со статьями на странице.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="actionOnItem"></param>
        /// <returns></returns>
        private bool GoAction(HtmlElement item, ActionOnItem actionOnItem, int itemNumb)
        {
            switch (actionOnItem)
            {
                case ActionOnItem.Like: return ActionLike(item, itemNumb);
                case ActionOnItem.Dislike: return ActionDislike(item, itemNumb);
                case ActionOnItem.OpenItem: return ActionOpenItem(item, itemNumb);
            }

            Logger.Write($"[Actions left: {_actionList.Count - 1}]\t[Item: {itemNumb}]\tСобытие не определено и пропущено", LoggerType.Info, false, false, true);
            return true;
        }

        /// <summary>
        /// Открыть элемент ленты (статью, сайт, видео).
        /// </summary>
        /// <param name="item"></param>
        /// <param name="itemNumb"></param>
        /// <returns></returns>
        private bool ActionOpenItem(HtmlElement item, int itemNumb)
        {
            string contentType, urlWalk;

            var stopwatch = new Stopwatch();
            var scrollIterationsOnSite = 15;
            var setTimeInSeconds = _timeOnPageArticleZen.ExtractNumber();
            var sourceUrl = Instance.ActiveTab.URL;
            var openSourceTab = false;
            var xpathItemLabel = ".//a[@aria-label!='' and @href!='']";
            var heItemLabel = item.FindChildByXPath(xpathItemLabel, 0);

            // Установка времени ожидания загрузки страницы
            Instance.ActiveTab.NavigateTimeout = 20;

            // Индивидуальное состояние занятости
            if (_individualStateBusyEnabled)
                InstanceSettings.BusySettings.SetBusySettings(_individualStateBusy);

            // Обработка ошибки, если элемент не найден
            if (heItemLabel.IsNullOrVoid() && _skipErrorOpenArticles)
            {
                Logger.Write($"[Actions left: {_actionList.Count - 1}]\t[Item: {itemNumb}]\t[Skip error: {_skipErrorOpenArticles}]\tНе удалось открыть URL (элемент найден)", LoggerType.Info, false, false, true);
                return true;
            }
            else if (heItemLabel.IsNullOrVoid() && !_skipErrorOpenArticles)
            {
                Logger.Write($"[Actions left: {_actionList.Count}]\t[Item: {itemNumb}]\t[Skip error: {_skipErrorOpenArticles}]\tНе удалось открыть URL (элемент найден)", LoggerType.Info, false, false, true);
                return false;
            }

            // Клик по статье
            heItemLabel.Click(Instance.ActiveTab, Rnd.Next(1000, 1500), true);

            // Обработка ошибки, если не удалось загрузить страницу из-за прокси
            if (!Instance.ActiveTab.FindElementByXPath("//span[@jsselect='heading' and text()!='']", 0).IsNullOrVoid() || Instance.ActiveTab.URL.Contains("about:blank"))
            {
                urlWalk = Instance.ActiveTab.URL;

                CloseTabs(sourceUrl, openSourceTab);

                if (_skipErrorOpenArticles)
                {
                    Logger.Write($"[Actions left: {_actionList.Count - 1}]\t[Item: {itemNumb}]\t[Skip error: {_skipErrorOpenArticles}]\tНе удалось открыть URL: {urlWalk}", LoggerType.Info, false, false, true);
                    return true;
                }
                else
                {
                    Logger.Write($"[Actions left: {_actionList.Count}]\t[Item: {itemNumb}]\t[Skip error: {_skipErrorOpenArticles}]\tНе удалось открыть URL: {urlWalk}", LoggerType.Info, false, false, true);
                    return false;
                }
            }

            stopwatch.Start();

            if (!Instance.ActiveTab.FindElementByXPath("//div[contains(@id, 'article') and contains(@id, 'left')]/descendant::div[contains(@class, 'sticky')]/descendant::button[contains(@class, 'left-column-button')]|//h1[contains(@class, 'article') and contains(@class, 'title')]", 0).IsNullOrVoid() || Instance.ActiveTab.URL.Contains("zen.yandex.ru/media/"))
            {
                // Обработка статьи
                urlWalk = Instance.ActiveTab.URL;
                contentType = "Article";

                // Находиться на странице заданное количество времени (иначе обычный скроллинг).
                if (_timeOnPageEnable)
                {
                    var hesArticle = new List<HtmlElement>();

                    var xpathButtonsLeftMenu = new[] { "//div[contains(@id, 'article') and contains(@id, 'left')]/descendant::div[contains(@class, 'sticky')]/descendant::button[contains(@class, 'left-column-button')]", "Элементы бокового меню" };
                    var xpathElementsArticles = new[]
                    {
                        new[]
                        {
                            "//h1[contains(@class, 'article') and contains(@class, 'title')]",
                            "//footer[contains(@class, 'article') and contains(@class, 'statistics')]",
                            "//div[contains(@class, 'article-render') and contains(@itemprop, 'articleBody')]/descendant::*[contains(@class, 'article-render')]",
                            "//section[contains(@class, 'theme-tags')]",
                            "//div[contains(@class, 'bottom-block-redesign')]/descendant::div[contains(@class, 'bottom-block-redesign')]"
                        },
                        new[] {"Элементы статьи"}
                    };
                    var xpathAdvertisingRightBlock = new[] { "//div[contains(@class, 'article-right-ad-block') and contains(@class, 'ad-content')]", "Блок рекламы" };
                    var xpathCommentsBlock = new[] { "//div[contains(@class, 'article-comments')]/descendant::div[contains(@class, 'is-redesign') and contains(@class, 'without-paddings')]", "Блок комментариев" };

                    // Добавляем элементы в список для обработки
                    hesArticle.AddRange(Instance.ActiveTab.FindElementsByXPath(xpathButtonsLeftMenu[0]));
                    hesArticle.AddRange(Instance.ActiveTab.FindElementsByXPath(string.Join("|", xpathElementsArticles[0])));
                    hesArticle.AddRange(Instance.ActiveTab.FindElementsByXPath(xpathAdvertisingRightBlock[0]));
                    hesArticle.AddRange(Instance.ActiveTab.FindElementsByXPath(xpathCommentsBlock[0]));

                    var counter = 0;

                    while (true)
                    {
                        // Обработка ошибки, если элементы не найдены
                        if (hesArticle.Count == 0)
                        {
                            urlWalk = Instance.ActiveTab.URL;

                            if (_skipErrorOpenArticles)
                            {
                                Logger.Write($"[Actions left: {_actionList.Count - 1}]\t[Item: {itemNumb}]\t[Skip error: {_skipErrorOpenArticles}]\tНе найдены элементы статьи: {urlWalk}", LoggerType.Info, true, true, true, LogColor.Yellow);
                                return true;
                            }
                            else
                            {
                                Logger.Write($"[Actions left: {_actionList.Count}]\t[Item: {itemNumb}]\t[Skip error: {_skipErrorOpenArticles}]\tНе найдены элементы статьи: {urlWalk}", LoggerType.Info, true, true, true, LogColor.Yellow);
                                return false;
                            }
                        }

                        // Переворот коллекции, если элементы все обработаны
                        if (counter >= hesArticle.Count)
                        {
                            hesArticle.Reverse();
                            counter = 0;
                        }

                        Instance.ActiveTab.FullEmulationMouseMoveAboveHtmlElement(hesArticle[counter], Rnd.Next(120, 150));

                        if (stopwatch.ElapsedMilliseconds / 1000 > setTimeInSeconds) break;

                        counter++;
                    }
                }
                else
                {
                    for (int i = 0; i < scrollIterationsOnSite; i++)
                    {
                        Instance.ActiveTab.FullEmulationMouseWheel(0, Rnd.Next(600, 750));
                        Thread.Sleep(Rnd.Next(100, 150));
                    }
                }
            }
            else if (!Instance.ActiveTab.FindElementByXPath("//div[contains(@class, 'video-viewer-player')]/descendant::div[contains(@class, 'player-ratio')]", 0).IsNullOrVoid() || Instance.ActiveTab.URL.Contains("zen.yandex.ru/#video"))
            {
                // Обработка видео
                urlWalk = Instance.ActiveTab.URL;
                contentType = "Video";

                var xpathButtonCloseVideo = new[] { "//div[contains(@class, 'zen-ui-modal') and contains(@class, 'scrollbar-fix')]/descendant::div[contains(@class, 'close-wrapper')]/descendant::button[contains(@class, 'close')]", "Кнопка - Закрыть видео" };

                // Находиться на странице заданное количество времени (иначе обычный скроллинг).
                if (_timeOnPageEnable)
                {
                    while (true)
                    {
                        Thread.Sleep(1000);

                        if (stopwatch.ElapsedMilliseconds / 1000 > setTimeInSeconds) break;
                    }
                }
                else
                {
                    for (int i = 0; i < scrollIterationsOnSite; i++)
                    {
                        Instance.ActiveTab.FullEmulationMouseWheel(0, Rnd.Next(600, 750));
                        Thread.Sleep(Rnd.Next(100, 150));
                    }
                }

                // Закрыть видео
                Instance.FuncGetFirstHe(xpathButtonCloseVideo, false, true).Click(Instance.ActiveTab, Rnd.Next(1000, 1500));
            }
            else
            {
                // Обработка обычных сайтов (скроллинг страницы)
                urlWalk = Instance.ActiveTab.URL;
                contentType = "Site";

                for (int i = 0; i < scrollIterationsOnSite; i++)
                {
                    Instance.ActiveTab.FullEmulationMouseWheel(0, Rnd.Next(600, 750));
                    Thread.Sleep(Rnd.Next(100, 150));
                }
            }

            stopwatch.Stop();

            CloseTabs(sourceUrl, openSourceTab);

            Logger.Write($"[Actions left: {_actionList.Count - 1}]\t[Item: {itemNumb}]\t[Type: {contentType} | {stopwatch.ElapsedMilliseconds / 1000} in sec]\tURL Обработан: {urlWalk}", LoggerType.Info, false, false, true);
            return true;
        }

        /// <summary>
        /// Поставить дизлайк статье в ленте.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="itemNumb"></param>
        /// <returns></returns>
        private bool ActionDislike(HtmlElement item, int itemNumb)
        {
            var xpathDislike = ".//div[contains(@class, 'icon')]/descendant::button";
            var heDislike = item.FindChildByXPath(xpathDislike, 1);

            // Обработка ошибки, если элемент не найден
            if (heDislike.IsNullOrVoid() && _skipErrorDislike)
            {
                Logger.Write($"[Actions left: {_actionList.Count - 1}]\t[Item: {itemNumb}]\t[Skip error: {_skipErrorDislike}]\tНе удалось поставить дизлайк (не найден потомок у родительского элемента)", LoggerType.Info, false, false, true);
                return true;
            }
            else if (heDislike.IsNullOrVoid() && !_skipErrorDislike)
            {
                Logger.Write($"[Actions left: {_actionList.Count}]\t[Item: {itemNumb}]\t[Skip error: {_skipErrorDislike}]\tНе удалось поставить дизлайк (не найден потомок у родительского элемента)", LoggerType.Info, false, false, true);
                return false;
            }

            heDislike.Click(Instance.ActiveTab, Rnd.Next(1000, 1500));

            Logger.Write($"[Actions left: {_actionList.Count - 1}]\t[Item: {itemNumb}]\tДизлайк поставлен", LoggerType.Info, false, false, true);
            return true;
        }

        /// <summary>
        /// Поставить лайк статье в ленте.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="itemNumb"></param>
        /// <returns></returns>
        private bool ActionLike(HtmlElement item, int itemNumb)
        {
            var xpathLike = ".//div[contains(@class, 'icon')]/descendant::button";
            var heLike = item.FindChildByXPath(xpathLike, 0);

            // Обработка ошибки, если элемент не найден
            if (heLike.IsNullOrVoid() && _skipErrorLike)
            {
                Logger.Write($"[Actions left: {_actionList.Count - 1}]\t[Item: {itemNumb}]\t[Skip error: {_skipErrorLike}]\tНе удалось поставить лайк (не найден потомок у родительского элемента)", LoggerType.Info, false, false, true);
                return true;
            }
            else if (heLike.IsNullOrVoid() && !_skipErrorLike)
            {
                Logger.Write($"[Actions left: {_actionList.Count}]\t[Item: {itemNumb}]\t[Skip error: {_skipErrorLike}]\tНе удалось поставить лайк (не найден потомок у родительского элемента)", LoggerType.Info, false, false, true);
                return false;
            }


            // Клик по лайку
            heLike.Click(Instance.ActiveTab, Rnd.Next(1000, 1500));

            // Проверка действия
            if (item.FindChildByXPath(xpathLike, 0).GetAttribute("aria-pressed").Contains("true"))
            {
                Logger.Write($"[Actions left: {_actionList.Count - 1}]\t[Item: {itemNumb}]\tЛайк поставлен", LoggerType.Info, false, false, true);
                return true;
            }
            else
            {
                if (_skipErrorLike)
                {
                    Logger.Write($"[Actions left: {_actionList.Count - 1}]\t[Item: {itemNumb}]\t[Skip error: {_skipErrorLike}]\tНе удалось поставить лайк (aria-pressed=false)", LoggerType.Info, false, false, true);
                    return true;
                }
                else
                {
                    Logger.Write($"[Actions left: {_actionList.Count}]\t[Item: {itemNumb}]\t[Skip error: {_skipErrorLike}]\tНе удалось поставить лайк (aria-pressed=false)", LoggerType.Info, false, false, true);
                    return false;
                }
            }
        }

        /// <summary>
        /// Закрытие ненужных табов.
        /// </summary>
        /// <param name="sourceUrl"></param>
        /// <param name="openSourceTab"></param>
        private void CloseTabs(string sourceUrl, bool openSourceTab)
        {
            // Установка времени ожидания загрузки страницы
            Instance.ActiveTab.NavigateTimeout = 120;

            // Состояние занятости по умолчанию
            if (_individualStateBusyEnabled)
                InstanceSettings.BusySettings.SetDefaultBusySettings();

            var referrer = Instance.ActiveTab.URL;

            // Закрытие ненужных вкладок
            foreach (var tb in Instance.AllTabs)
            {
                if (tb.URL == sourceUrl)
                {
                    openSourceTab = true;
                }
                else tb.Close();
            }

            if (!openSourceTab) Instance.ActiveTab.Navigate(sourceUrl, referrer, true);
        }

        /// <summary>
        /// Проверка наличия бокового меню в zen.yandex и изменение гео, если требуется.
        /// </summary>
        /// <param name="sourceUrl"></param>
        /// <returns></returns>
        private bool CheckMenuItemIfVoidThenChangeGeo()
        {
            var xpathMenuItem = new[] { "//div[contains(@data-portal-key, sidebar)]/descendant::div[contains(@class, 'nav-menu-item')]", "Боковое меню zen.yandex с элементами" };
            var xpathFieldCity = new[] { "//div[contains(@class, 'geo-options')]/descendant::input[contains(@id, 'city')]", "Поле - Страна" };
            var xpathCheckboxAutoCity = new[] { "//div[contains(@class, 'checkbox') and contains(@class, 'city')]/descendant::span[contains(@class, 'checkbox_checked_yes')]", "Чекбокс автоматического определения страны" };
            var xpathButtonSave = new[] { "//button[contains(@class, 'save')]", "Кнопка - Сохранить" };

            if (!Instance.FuncGetFirstHe(xpathMenuItem, false, false).IsNullOrVoid())
            {
                Logger.Write($"Боковое меню \"zen.yandex\" найдено", LoggerType.Info, false, false, false);
                return true;
            }
            else Logger.Write($"Отсутствует боковое меню \"zen.yandex\". Переход к смене geo", LoggerType.Info, true, false, true);

            //var sourceUrl = $"https://zen.yandex.{Domain}/";
            var sourceUrl = Instance.ActiveTab.URL;
            var counterAttempt = 0;

            while (true)
            {
                if (Instance.ActiveTab.URL != sourceUrl)
                    Instance.ActiveTab.Navigate(sourceUrl, true);

                try
                {
                    if (++counterAttempt > 5)
                    {
                        Logger.Write($"Израсходован лимит попыток на изменение geo", LoggerType.Warning, true, true, true, LogColor.Yellow);
                        return false;
                    }

                    if (Instance.FuncGetFirstHe(xpathMenuItem, false, false).IsNullOrVoid())
                    {
                        Instance.ActiveTab.Navigate("https://yandex.ru/tune/geo", true);

                        // Получаем поле страны
                        var heFieldCity = Instance.FuncGetFirstHe(xpathFieldCity, true, true, 5);

                        // Очистить поле со страной
                        if (heFieldCity.GetAttribute("value") != "") heFieldCity.SetAttribute("value", "");

                        // Устанавливаем случайный русский город в поле со страной и вызываем событие "Enter"
                        heFieldCity.SetValue
                        (
                            Instance.ActiveTab, DictionariesAndLists.ListOfRussianCities.GetLine(LineOptions.Random),
                            LevelEmulation.SuperEmulation, Rnd.Next(3000, 3500), false, false, true, Rnd.Next(3000, 3500)
                        );

                        // Получаем активный чекбокс, если он не деактивирован сам и обрабатываем его
                        var heCheckboxAutoCity = Instance.FuncGetFirstHe(xpathCheckboxAutoCity, false, false);

                        if (!heCheckboxAutoCity.IsNullOrVoid())
                        {
                            heCheckboxAutoCity.FindChildByXPath(".//input[contains(@class, 'checkbox') and contains(@class, 'checkbox')]", 0).Click(Instance.ActiveTab, Rnd.Next(1000, 1500));

                            Logger.Write($"Чекбокс автоматического определения geo не изменил своего состояния автоматически. Поэтому, была предпринята попытка ручного изменения состояния чекбокса", LoggerType.Info, true, false, false);
                        }

                        // Получаем кнопку сохранения изменений и обрабатываем её
                        var heButtonSave = Instance.FuncGetFirstHe(xpathButtonSave);

                        if (!heButtonSave.GetAttribute("disabled").Contains("disabled"))
                        {
                            heButtonSave.Click(Instance.ActiveTab, Rnd.Next(1000, 1500));

                            Instance.ActiveTab.Navigate(sourceUrl, true);
                        }
                        else Logger.Write($"Что-то пошло не так и кнопка \"Сохранить\" не активна", LoggerType.Warning, true, true, false);
                    }
                    else
                    {
                        Logger.Write($"Geo успешно изменено и боковое меню \"zen.yandex\" на месте", LoggerType.Info, true, false, true);
                        ProfileWorker.SaveProfile(true);
                        return true;
                    }
                }
                catch { continue; }
            }
        }

        /// <summary>
        /// Проверка наличия панели оценки zen.yandex (выбор оценки, если есть).
        /// </summary>
        private void CheckZenAppraisalBar()
        {
            var xpathItems = new[] { "//div[contains(@class, 'desktop-interview-modal') and contains(@class, 'modal')]/descendant::div[contains(@class, 'container')]/descendant::div[contains(@class, 'single-choice-image') and contains(@class, 'list')]/descendant::div[contains(@class, 'item-image') and contains(@style, 'avatars')]", "Элементы оценки сервиса zen.yandex" };

            var heItems = Instance.ActiveTab.FindElementsByXPath(xpathItems[0]).ToList();

            if (heItems.Count == 3)
            {
                var buttonText = string.Empty;
                var index = Rnd.Next(1, heItems.Count);

                heItems[index].Click(Instance.ActiveTab, Rnd.Next(1000, 1500));

                switch (index)
                {
                    case 0:
                        buttonText = "Нет";
                        break;
                    case 1:
                        buttonText = "Не знаю";
                        break;
                    case 2:
                        buttonText = "Да";
                        break;
                }

                Logger.Write($"[Порекомендуете ли вы Дзен друзьям и близким?][{buttonText}]\tОбработана панель оценки \"zen.yandex\"", LoggerType.Info, true, false, true);
            }
        }

        /// <summary>
        /// Получение, обработка и установка данных перед запуском.
        /// </summary>
        /// <returns></returns>
        private bool ResourceHandler()
        {
            var pathSharedFolderDonors = Zenno.ExecuteMacro(Zenno.Variables["cfgPathFolderDonors"].Value);

            if (AccountsTable.RowCount == 0)
            {
                Program.StopTemplate(Zenno, $"Таблица с аккаунтами/донорами пуста");
                return false;
            }

            // Проверяем наличие папки (создаем её, если нужно)
            if (string.IsNullOrWhiteSpace(pathSharedFolderDonors))
            {
                Program.StopTemplate(Zenno, $"Не указана папка с донорами");
                return false;
            }
            else if (!Directory.Exists(pathSharedFolderDonors))
            {
                if (!CreateFolderResourceIfNoExist)
                {
                    Program.StopTemplate(Zenno, $"Указанная папка с донорами не существует");
                    return false;
                }
                else Directory.CreateDirectory(pathSharedFolderDonors);
            }

            lock (SyncObjects.TableSyncer)
            {
                var accountsCount = AccountsTable.RowCount;

                for (int row = 0; row < accountsCount; row++)
                {
                    var login = AccountsTable.GetCell((int)TableColumnEnum.Inst.Login, row);
                    var donor = AccountsTable.GetCell((int)TableColumnEnum.Inst.InstaUrl, row);

                    // Получение аккаунта, настройка до.лога, информация о директории и файле описания аккаунта
                    if (!string.IsNullOrWhiteSpace(login))
                    {
                        CurrentObjectAtWork = ResourceTypeEnum.Account;
                        Login = login;
                        ObjectDirectory = new DirectoryInfo($@"{Zenno.Directory}\Accounts\{Login}");
                    }
                    else if (!string.IsNullOrWhiteSpace(donor))
                    {
                        // Проверяем логин из общей таблицы (если есть, то подключаем его в работу, если нет, то всё ок - подрубаем донор)
                        if (!TableGeneralAndTableModeIsSame)
                        {
                            // Получаем логин яндекс из общей для проверки
                            login = AccountsGeneralTable.GetCell((int)TableColumnEnum.Inst.InstaUrl, donor, (int)TableColumnEnum.Inst.Login, false);

                            // Подключаем аккаунт в работу, если он найден в общей таблице
                            if (!string.IsNullOrWhiteSpace(login))
                            {
                                CurrentObjectAtWork = ResourceTypeEnum.Account;
                                Login = login;
                                ObjectDirectory = new DirectoryInfo($@"{Zenno.Directory}\Accounts\{Login}");
                            }
                        }

                        // Подключаем донор в работу, если логина нет ни в таблице режима, ни в общей
                        if (string.IsNullOrWhiteSpace(login))
                        {
                            CurrentObjectAtWork = ResourceTypeEnum.Donor;
                            Login = donor;
                            InstagramUrl = donor;
                            ObjectDirectory = new DirectoryInfo(Path.Combine(pathSharedFolderDonors, $@"{Regex.Match(Login, @"(?<=com/).*?(?=/)").Value}"));
                        }
                    }
                    else continue;

                    if (CurrentObjectAtWork == ResourceTypeEnum.Account) Logger.SetCurrentObjectForLogText(Login, ResourceTypeEnum.Account);
                    else if (ShorDonorNameForLog) Logger.SetCurrentObjectForLogText(ObjectDirectory.Name, ResourceTypeEnum.Donor);
                    else Logger.SetCurrentObjectForLogText(Login, ResourceTypeEnum.Donor);

                    // Тип ресурса для работы использовать
                    switch (_useResourceTypeInWork)
                    {
                        case UseObjectTypeInWorkEnum.UseOnlyDonor:
                            if (CurrentObjectAtWork != ResourceTypeEnum.Donor) continue;
                            else break;
                        case UseObjectTypeInWorkEnum.UseOnlyAccount:
                            if (CurrentObjectAtWork != ResourceTypeEnum.Account) continue;
                            else break;
                    }

                    // Проверка на наличия ресурса и его занятость
                    if (!ResourceIsAvailable(Login, row)) continue;

                    // Проверка директории на существование(создать, если требуется)
                    if (!ResourceDirectoryExists())
                    {
                        ObjectDirectory.Refresh();

                        if (!ObjectDirectory.Exists) continue;
                    }

                    // Проверяем последнее время прогулки
                    var logList = Logger.GetAccountLog(LogFilter.WalkingUnixtime);

                    if (logList.Count != 0)
                    {
                        var timeFromLastWalk = int.Parse(Zenno.Variables["cfgTimeFromLastWalk"].Value) * 60;
                        var lastUnixtime = int.Parse(Logger.GetRegexPatternForAccountLog(LogFilter.WalkingUnixtime).Match(logList.Last()).Value);
                        var nowUnixtime = Logger.GetUnixtime();
                        var calculationUnixtimeResult = nowUnixtime - lastUnixtime;

                        if (calculationUnixtimeResult < timeFromLastWalk)
                        {
                            Logger.Write($"Прошло недостаточно времени с последний прогулки. Прогулка будет доступна через: {(timeFromLastWalk - calculationUnixtimeResult) / 60} минут", LoggerType.Info, true, false, _enableInfoOnHowMuchTimeIsLeftBeforeWalk);
                            continue;
                        }
                    }

                    if (CurrentObjectAtWork == ResourceTypeEnum.Account)
                    {
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
                    }

                    // Получение и загрузка профиля
                    if (!ProfileWorker.LoadProfile(true)) continue;

                    // Получение прокси
                    if (!SetProxy((int)TableColumnEnum.Inst.Proxy, row, true)) continue;

                    //var additionalLog = IpInfo != null ? $" | proxy country: {IpInfo.CountryShortName} — {IpInfo.CountryFullName}" : "";

                    // Успешное получение ресурса
                    Program.AddResourceToCache(Login, true, true);
                    Logger.Write($"[Proxy table: {Proxy} | Proxy country: {IpInfo.CountryShortName} — {IpInfo.CountryFullName}]\t[Row: {row + 2}]\tАккаунт/донор успешно подключен", LoggerType.Info, true, false, true);
                    return true;
                }

                // Не удалось получить ресурс
                Program.ResetExecutionCounter(Zenno);
                Logger.Write($"Отсутствуют свободные/подходящие аккаунты/доноры", LoggerType.Info, false, true, true, LogColor.Violet);
                return false;
            }
        }

    }
}