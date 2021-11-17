using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Tools;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.InterfacesLibrary.SmsService.Enums;
using ZennoLab.InterfacesLibrary.Enums.Http;
using Global.ZennoExtensions;
using System.Threading;
using System.Text.RegularExpressions;
using System.IO;
using Global.ZennoLab.Json;
using Yandex.Zen.Core.Enums.Extensions;
using Yandex.Zen.Core.Tools.Extensions;
using Yandex.Zen.Core.Models.TableHandler;
using Yandex.Zen.Core.Enums;
using Yandex.Zen.Core.Enums.ZenChannelCreationAndDesign;
using Yandex.Zen.Core.Models.ZenChannelCreationAndDesign.ChannelSettings.DataModels;
using Yandex.Zen.Core.Enums.CheatActivity;
using Yandex.Zen.Core.Models.CheatActivity;
using Yandex.Zen.Core.Tools.Macros;
using System.Diagnostics;
using Yandex.Zen.Core.Tools.LoggerTool;
using Yandex.Zen.Core.Tools.LoggerTool.Enums;

namespace Yandex.Zen.Core.Services
{
    public class CheatActivity : ServicesComponents
    {
        private static readonly object _locker = new object();

        private static IZennoTable _articlesAndStatisticsTable;

        private readonly CommentsSourceEnum _commentsSource;

        private readonly bool _launchIsAllowed;
        private readonly bool _modifyArticleUrl;        
        private readonly int _limitGoToArticleForOneAccount;
        private readonly string _commonCommentsPathFile;
        private readonly string _fileNameIndividualForAllArticlesOfAccount;
        private readonly string _totalTransitionsSettings;
        private readonly string _secondsWatchArticleSettings;
        private readonly string _totalLikesSettings;
        private readonly string _totalCommentsSettings;

        private ArticleBasicModel _article;

        /// <summary>
        /// Конструктор для скрипта (настройка лога, проверка и установка прочих данных).
        /// </summary>
        public CheatActivity()
        {
            AccountsTable = Zenno.Tables["AccountsForCheatActivity"];
            _articlesAndStatisticsTable = Zenno.Tables["ArticlesAndStatistics"];

            // Проверка наличия аккаунтов для накручивания
            if (AccountsTable.RowCount == 0)
            {
                Program.StopTemplate(Zenno, $"Таблица с аккаунтами пуста");
                return;
            }

            // Проверка наличия статей накрутки
            if (_articlesAndStatisticsTable.RowCount == 0)
            {
                Program.StopTemplate(Zenno, $"Таблица со статьями и статистикой пуста");
                return;
            }

            // Источник комментариев
            new Dictionary<string, CommentsSourceEnum>
            {
                { "Использовать комментарии из общего файла", CommentsSourceEnum.CommonFile},
                { "Использовать индивидуальные комментарии относительно статей аккаунта", CommentsSourceEnum.IndividualForAllArticlesOfAccount},
                { "Использовать индивидуальные комментарии для каждой статьи", CommentsSourceEnum.IndividualForEachArticle }
            }
            .TryGetValue(Zenno.Variables["cfgCommentsSourceForCheatActivity"].Value, out _commentsSource);

            // Настройка привязки номера
            BindingPhoneToAccountIfRequaid = Zenno.Variables["cfgBindingPhoneIfRequiredForCheatActivity"].Value.Contains("Привязывать номер");

            // Получение данных из настроек для формирования задания
            _totalTransitionsSettings = Zenno.Variables["cfgTotalTransitionsForCheatActivity"].Value;
            _secondsWatchArticleSettings = Zenno.Variables["cfgTimeWatchArticleForCheatActivity"].Value;
            _totalLikesSettings = Zenno.Variables["cfgTotalLikesForCheatActivity"].Value;
            _totalCommentsSettings = Zenno.Variables["cfgTotalCommentsForCheatActivity"].Value;
            _limitGoToArticleForOneAccount = Zenno.Variables["cfgTransitionsLimitForOneAccountForCheatActivity"].Value.ExtractNumber();

            _commonCommentsPathFile = Zenno.ExecuteMacro(Zenno.Variables["cfgSharedCommentsFileForCheatActivity"].Value);
            _fileNameIndividualForAllArticlesOfAccount = Zenno.Variables["cfgFileNameIndividualForAllArticlesOfAccountForCheatActivity"].Value;
            _modifyArticleUrl = bool.Parse(Zenno.Variables["cfgModifyArticleUrlForCheatActivity"].Value);

            lock (_locker)
                _launchIsAllowed = ResourceHandler();
        }

        /// <summary>
        /// Запуск скрипта.
        /// </summary>
        public void Start()
        {
            if (!_launchIsAllowed) return;

            NavigateArticle();
        }

        /// <summary>
        /// Перейти на страницу статьи.
        /// </summary>
        public void NavigateArticle()
        {
            var url = _article.ArticleUrl;

            // Модификация ссылки
            if (_modifyArticleUrl)
            {
                var rid = default(string);
                var counterAttemptsGetFeedId = default(int);

                while (true)
                {
                    if (++counterAttemptsGetFeedId > 3)
                    {
                        Logger.Write("Не удалось получить rid", LoggerType.Warning, true, true, true, LogColor.Yellow);
                        return;
                    }

                    rid = GetFeedId();

                    if (!string.IsNullOrWhiteSpace(rid)) break;

                    Thread.Sleep(3000);
                }

                url += $"?from=feed&utm_referrer=https%3A%2F%2Fzen.yandex.com&rid={rid}&integration=site_desktop&place=layout&secdata=C{TextMacros.GenerateString(23, 30, "abc")}%3D%3D";
            }

            Instance.ActiveTab.Navigate(url, $"https://zen.yandex.{Domain}/", true);


        }

        /// <summary>
        /// Гуляние по странице статьи.
        /// </summary>
        /// <returns></returns>
        private bool ActionWalkOnPage()
        {
            var setTimeInSeconds = _article.Task.SecondsWatchArticle;

            var stopwatch = new Stopwatch();
            var triggerStartStopwatch = default(bool);
            var triggerReverse = default(bool);
            var counter = default(int);

            var xpathButtonsLeftMenu = new[] { "//div[contains(@id, 'article') and contains(@id, 'left')]/descendant::div[contains(@class, 'sticky')]/descendant::button[contains(@class, 'left-column-button')]", "Элементы бокового меню" };
            var xpathArticleControls = new[] { "//div[contains(@class, 'article-header') and contains(@class, 'wrapper')]/descendant::div[contains(@class, 'publisher-controls') and contains(@class, 'is-redesign')]", "Контролы статьи (пользователь, подписка и т.д.)" };
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
                new[] { "Элементы статьи" }
            };
            var xpathAdvertisingRightBlock = new[] { "//div[contains(@class, 'article-right-ad-block') and contains(@class, 'ad-content')]", "Блок рекламы" };
            var xpathCommentsBlock = new[] { "//div[contains(@class, 'article-comments')]/descendant::div[contains(@class, 'is-redesign') and contains(@class, 'without-paddings')]", "Блок комментариев" };

            while (true)
            {
                var heArticleCollection = new List<HtmlElement>();

                // Добавляем элементы в список для обработки
                heArticleCollection.AddRange(Instance.ActiveTab.FindElementsByXPath(xpathButtonsLeftMenu[0]));
                heArticleCollection.AddRange(Instance.ActiveTab.FindElementsByXPath(xpathArticleControls[0]));
                heArticleCollection.AddRange(Instance.ActiveTab.FindElementsByXPath(string.Join("|", xpathElementsArticles[0])));
                heArticleCollection.AddRange(Instance.ActiveTab.FindElementsByXPath(xpathAdvertisingRightBlock[0]));
                heArticleCollection.AddRange(Instance.ActiveTab.FindElementsByXPath(xpathCommentsBlock[0]));

                if (!triggerStartStopwatch)
                {
                    triggerStartStopwatch = true;
                    stopwatch.Start();
                }

                if (heArticleCollection.Count == 0)
                {
                    Logger.Write(_article.Directory, "Не найдено ни одного элемента на странице статьи", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    Logger.ErrorAnalysis(_article.Directory, true, true, true, new List<string>
                    {

                    });
                    return false;
                }
                //throw new Exception("Не найдено ни одного элемента на странице");

                // Переворот коллекции, если элементы все обработаны
                if (counter >= heArticleCollection.Count)
                {
                    heArticleCollection.Reverse();
                    counter = 0;
                    triggerReverse = true;
                }
                else if (triggerReverse)
                {
                    heArticleCollection.Reverse();
                }

                Instance.ActiveTab.FullEmulationMouseMoveAboveHtmlElement(heArticleCollection[counter], Rnd.Next(70, 110));

                if ((stopwatch.ElapsedMilliseconds / 1000) > setTimeInSeconds)
                {
                    stopwatch.Stop();
                    Logger.Write(_article.Directory, "", LoggerType.Info, true, false, true, LogColor.Blue);
                    return true;
                }

                counter++;
            }
        }


        private bool ActionLikeArticle()
        {
            var heCollection = Instance.ActiveTab.FindElementsByXPath("//div[contains(@class, 'bottom-block-redesign')]/descendant::button");

            if (heCollection.Count < 3)
                throw new Exception("В коллекции меньше трех элементов");

            var heLike = heCollection.First();

            if (heLike.GetAttribute("aria-pressed").Contains("false"))
            {
                heLike.Click();
                Zenno.SendInfoToLog("Лайк поставлен");
            }
            else
            {
                Zenno.SendInfoToLog("Лайк уже стоит");
            }

            return true;
        }

        /// <summary>
        /// Оставление комментария под статьей.
        /// </summary>
        /// <returns></returns>
        private bool ActionSendCommentArticle()
        {
            var xpathFieldComment = new[] { "//textarea[contains(@class, 'comment-editor') and not(@tabindex)]" , "Поле - Написать комментарий"};
            var xpathButtonSendComment = new[] { "//div[contains(@class, 'comment-editor') and contains(@class, 'editor-controls')]/descendant::button[contains(@class, 'send')]", "Кнопка - Отправить комментарий" };

            // Получение элементов
            var heFieldComment = Instance.FuncGetFirstHe(xpathFieldComment, false, true);
            var heButtonSendComment = Instance.FuncGetFirstHe(xpathButtonSendComment, false, true);

            // Проверка элементов
            if (new[]{heFieldComment, heButtonSendComment}.Any(x => x.IsNullOrVoid()))
            {
                var heElements = new List<string>();

                if (heFieldComment.IsNullOrVoid()) heElements.Add(xpathFieldComment.XPathToStandardView());
                if (heButtonSendComment.IsNullOrVoid()) heElements.Add(xpathButtonSendComment.XPathToStandardView());

                Logger.ErrorAnalysis(true, true, true, new List<string>
                {
                    Instance.ActiveTab.URL,
                    string.Join(Environment.NewLine, heElements),
                    string.Empty
                });

                return false;
            }

            // Написание и отправка комментария
            heFieldComment.Click(Instance.ActiveTab, Rnd.Next(500, 1000));
            heFieldComment.SetValue(Instance.ActiveTab, _article.CommentData.CommentText, LevelEmulation.SuperEmulation, Rnd.Next(500, 1500));
            heButtonSendComment.Click(Instance.ActiveTab, Rnd.Next(500, 1000));

            _article.CommentData.WriteToUsedCommentsLog();

            return true;
        }

        /// <summary>
        /// Получение идентификатора (rid) ленты. 
        /// </summary>
        /// <returns></returns>
        private string GetFeedId()
        {
            var response = ZennoPoster.HTTP.Request
            (
                HttpMethod.GET, "https://zen.yandex.ru/", proxy: Instance.GetProxy(), Encoding: "utf-8", respType: ResponceType.BodyOnly,
                Timeout: 30000, UserAgent: Zenno.Profile.UserAgent, cookieContainer: Zenno.Profile.CookieContainer
            );

            var rid = Regex.Match(response, "(?<=\"rid\":\").*?(?=\")").Value;

            return !string.IsNullOrEmpty(rid) ? rid : null;
        }

        /// <summary>
        /// Получение, обработка и установка данных перед запуском.
        /// </summary>
        /// <returns></returns>
        private bool ResourceHandler()
        {
            // Получение статьи и задания для обработки
            var numbArticlesAndStatistics = _articlesAndStatisticsTable.RowCount;

            for (int row = 0; row < numbArticlesAndStatistics + 1; row++)
            {
                if (row >= numbArticlesAndStatistics)
                {
                    Program.ResetExecutionCounter(Zenno);
                    Logger.Write($"Отсутствуют свободные/подходящие статьи для обработки", LoggerType.Info, false, true, true, LogColor.Violet);
                    return false;
                }

                // Создание экземпляра статьи для обработки
                _article = new ArticleBasicModel(_articlesAndStatisticsTable, row, _totalTransitionsSettings, _secondsWatchArticleSettings, _totalLikesSettings, _totalCommentsSettings);

                // Проверка статуска обработки
                if (_article.ArticleProcessStatus == ArticleProcessStatusEnum.Done) continue;

                // Проверка на наличия ресурса и его занятость
                if (ResourceIsAvailable(_article.Login, row)) continue;

                // Проверка директории аккаунта
                if (!_article.Directory.Exists && CreateFolderResourceIfNotExist)
                {
                    _article.Directory.Create();
                    Logger.Write(_article.Directory, $"[{_article.Directory.FullName}]\tПапка создана автоматически. Заполните её всеми необходимыми данными", LoggerType.Info, true, false, true);
                }
                else if (!_article.Directory.Exists)
                {
                    Logger.Write(_article.Directory, $"[{_article.Directory.FullName}]\tОтсутствует папка. Создайте её и заполните всеми необходимыми данными", LoggerType.Info, false, true, true, LogColor.Yellow);
                    continue;
                }

                // Проверка наличия ссылки на статью
                if (_article.ArticleUrl == null)
                {
                    Logger.Write(_article.Directory, $"[Row: {row + 2}]\tОтсутствует ссылка на статью", LoggerType.Info, false, true, true, LogColor.Yellow);
                    continue;
                }

                // Проверка наличия задачи
                if (_article.Task == null) continue;

                // Получение комментариев для статьи
                var commentsPathFile = default(string);

                switch (_commentsSource)
                {
                    case CommentsSourceEnum.CommonFile:
                        commentsPathFile = _commonCommentsPathFile;
                        break;
                    case CommentsSourceEnum.IndividualForAllArticlesOfAccount:
                        commentsPathFile = _fileNameIndividualForAllArticlesOfAccount;
                        break;
                    case CommentsSourceEnum.IndividualForEachArticle:
                        commentsPathFile = _articlesAndStatisticsTable.GetCell((int)TableColumnEnum.StatisticsCheatActivity.IndividualComments, row);
                        break;
                }

                // Проверка наличия имени файла с комментариями
                if (string.IsNullOrWhiteSpace(commentsPathFile))
                {
                    Logger.Write(_article.Directory, $"[Источник комментариев: {_commentsSource}]\tНе указан файл с комментариями", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    return false;
                }
                else commentsPathFile = Path.GetExtension(commentsPathFile).ToLower() == ".txt" ? commentsPathFile : $"{commentsPathFile}.txt";

                if (!Regex.IsMatch(commentsPathFile, @"[a-zA-Z]:\\"))
                    commentsPathFile = Path.Combine(_article.Directory.FullName, commentsPathFile);

                var commentsFile = new FileInfo(commentsPathFile);

                // Проверка файла на существование
                if (!commentsFile.Exists)
                {
                    Logger.Write(_article.Directory, $"[Источник комментариев: {_commentsSource}]\t[{commentsFile.Exists}]\tНе существует файла с комментариями", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    return false;
                }

                // Получение комментариев
                var commentsList = GetCommentsListFromFile(commentsFile);

                // Проверка наличия комментариев
                if (commentsList.Count == 0)
                {
                    Logger.Write(_article.Directory, $"[Источник комментариев: {_commentsSource}]\t[{commentsFile.FullName}]\tНе удалось найти комментарии в файле. Проверьте правильность заполнения, перед каждым комментарием должен стоять макрос-разделитель: [-COMMENT-]", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    return false;
                }

                // Получение уникального комментария
                _article.CommentData = new CommentData(_article, commentsList);

                // Успешное получение ресурса
                Logger.Write(_article.Directory, $"[Row: {row + 2}]\tСтатья для обработки успешно получена", LoggerType.Info, true, false, true);
                _articlesAndStatisticsTable.SetCell((int)TableColumnEnum.StatisticsCheatActivity.ProcessStatus, row, _article.ArticleProcessStatus.ToString());
                break;
            }

            // Получение аккаунта, который будет обрабатывать статью/выполнять задание
            var accountsCount = AccountsTable.RowCount;

            for (int row = 0; row < accountsCount; row++)
            {
                // Получение аккаунта, настройка до.лога, информация о директории и файле описания аккаунта
                Login = AccountsTable.GetCell((int)TableColumnEnum.Inst.Login, row);
                ObjectDirectory = new DirectoryInfo($@"{Zenno.Directory}\Accounts\{Login}");

                Logger.SetCurrentObjectForLogText(Login, ObjectTypeEnum.Account);

                // Проверка на наличия ресурса и его занятость
                if (!ResourceIsAvailable(Login, row)) continue;

                // Получение пароля
                Password = AccountsTable.GetCell((int)TableColumnEnum.Inst.Password, row);

                if (string.IsNullOrWhiteSpace(Password))
                {
                    Logger.Write($"[Row: {row + 2}]\tОтсутствует пароль", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    continue;
                }

                // Получение ответа на секретный вопрос
                if (BindingPhoneToAccountIfRequaid)
                {
                    Answer = AccountsTable.GetCell((int)TableColumnEnum.Inst.Answer, row);

                    if (string.IsNullOrWhiteSpace(Answer))
                    {
                        Logger.Write($"[Row: {row + 2}]\tОтсутствует ответ на контрольный вопрос", LoggerType.Warning, true, true, true, LogColor.Yellow);
                        continue;
                    }
                }

                // Проверка директории на существование (создать, если требуется)
                if (!ObjectDirectory.Exists && CreateFolderResourceIfNotExist)
                {
                    ObjectDirectory.Create();
                    Logger.Write($"[{ObjectDirectory.FullName}]\tПапка создана автоматически. Заполните её всеми необходимыми данными", LoggerType.Info, true, false, true);
                }
                else if (!ObjectDirectory.Exists)
                {
                    Logger.Write($"[{ObjectDirectory.FullName}]\tОтсутствует папка. Создайте её и заполните всеми необходимыми данными", LoggerType.Info, false, true, true, LogColor.Yellow);
                    continue;
                }

                // Получение и загрузка профиля
                if (!ProfileWorker.LoadProfile(true)) continue;

                // Получение прокси
                if (!SetProxy((int)TableColumnEnum.Inst.Proxy, row, true)) continue;

                // Успешное получение ресурса
                Program.CurrentObjectCache.Add(Login);
                Program.CurrentObjectsOfAllThreadsInWork.Add(Login);
                Logger.Write($"[Proxy table: {Proxy} | Proxy country: {IpInfo.CountryShortName} — {IpInfo.CountryFullName}]\t[Row: {row + 2}]\tАккаунт успешно подключен", LoggerType.Info, true, false, true);
                return true;
            }

            // Не удалось получить ресурс
            Program.ResetExecutionCounter(Zenno);
            Logger.Write($"Отсутствуют свободные/подходящие аккаунты", LoggerType.Info, false, true, true, LogColor.Violet);
            return false;
        }

        /// <summary>
        /// Получение комментариев из файла.
        /// </summary>
        /// <param name="fileWithComments"></param>
        /// <returns></returns>
        private List<string> GetCommentsListFromFile(FileInfo fileWithComments)
        {
            var comments = default(List<string>);

            lock (_locker)
                comments = File.ReadAllText(fileWithComments.FullName, Encoding.UTF8).Split(new[] { "[-COMMENT-]" }, StringSplitOptions.None).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

            for (int i = 0; i < comments.Count; i++)
            {
                var trigger = default(bool);
                var temp = comments[i].Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();

                while (true)
                {
                    if (string.IsNullOrWhiteSpace(temp[0]))
                    {
                        temp.RemoveAt(0);
                        trigger = true;
                    }

                    var lastIndex = temp.Count - 1;

                    if (string.IsNullOrWhiteSpace(temp[lastIndex]))
                    {
                        temp.RemoveAt(lastIndex);
                        trigger = true;
                    }
                    else break;
                }

                if (trigger)
                    comments[i] = string.Join(Environment.NewLine, temp);
            }

            return comments;
        }

    }
}
