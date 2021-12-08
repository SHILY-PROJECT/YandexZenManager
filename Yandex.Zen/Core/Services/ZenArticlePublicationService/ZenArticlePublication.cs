using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;
using ZennoLab.InterfacesLibrary.Enums.Log;
using Global.ZennoExtensions;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
using Yandex.Zen.Core.Toolkit.BrowserCustomizer;
using Yandex.Zen.Core.Enums;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using Yandex.Zen.Core.Toolkit;
using Yandex.Zen.Core.Toolkit.BrowserCustomizer;
using Yandex.Zen.Core.Toolkit.BrowserCustomizer.Enums;
using Yandex.Zen.Core.Services.ZenArticlePublicationService.Models;
using Yandex.Zen.Core.Toolkit.BrowserCustomizer.Enums;

namespace Yandex.Zen.Core.Services.ZenArticlePublicationService
{
    public class ZenArticlePublication : ServicesDataAndComponents
    {
        private static readonly object _locker = new object();

        private List<Article> _articlesList;
        private DirectoryInfo _articlesDirectory;

        private readonly bool _launchIsAllowed;
        private readonly string _percentageRecommendedTagsForPublicationUse;

        private int _numbPublicationInCurrentRun;
        private int _numbPublicationFull;
        private int _counterPublicationFull;
        private int _counterPublicationInOneRun;

        /// <summary>
        /// Конструктор для скрипта (настройка лога, проверка и установка прочих данных).
        /// </summary>
        public ZenArticlePublication()
        {
            AccountsTable = Zenno.Tables["AccountsForPosting"];

            _articlesList = new List<Article>();

            _numbPublicationFull = int.Parse(Zenno.Variables["cfgNumbPublicationFull"].Value);
            _numbPublicationInCurrentRun = int.Parse(Zenno.Variables["cfgNumbPublicationInOneRun"].Value);
            _percentageRecommendedTagsForPublicationUse = Zenno.Variables["cfgPercentageRecommendedTagsForPublicationUse"].Value;

            lock (_locker) _launchIsAllowed = ResourceHandler();
        }

        /// <summary>
        /// Запуск скрипта.
        /// </summary>
        public void Start()
        {
            if (!_launchIsAllowed) return;

            Posting();
        }

        /// <summary>
        /// Начало публикации постов.
        /// </summary>
        private void Posting()
        {
            // xPath для добавления публикаций
            var xpathButtonAddPublication = new[]
            {
                new[]{ "//div[contains(@class, 'create-post') and contains(@class, 'zen-header')]/descendant::div[contains(@class, 'zen-header') and contains(@class, 'add-button')]", "Кнопка - Добавить публикацию (верхняя)" },
                new[]{ "//div[contains(@class, 'publications-groups-view') and contains(@class, 'btn-wrapper')]/descendant::button[contains(@class, 'base')]", "Кнопка - Добавить публикацию (нижняя)" }
            }
            .ToList().GetLine(LineOptions.Random);

            var xpathMenuPublication = new[] { "//div[contains(@class, 'base') and contains(@class, 'new-publication-dropdown')]", "Меню - Типы публикации" };
            var xpathChildButtonArticle = new[] { ".//div[contains(@class, 'article')]/ancestor::button[contains(@class, 'new-publication-dropdown')]", "Кнопка - Статья" };

            var xpathButtonCloseHelper = new[] { "//div[contains(@class, 'help-popup')]/descendant::div[contains(@role, 'button') and contains(@class, 'close')]", "Кнопка - Закрыть помощника" };
            var xpathButtonShowProfile = new[] { "//div[contains(@class, 'content') and contains(@class, 'notification-popup')]/descendant::button[contains(@data-action, 'show-profile')]", "Кнопка - Оформить канал" };
            var xpathFieldDescriptionChannel = new[] { "//span[contains(@data-lego, 'react') and contains(@class, 'textarea')]/descendant::textarea[contains(@class, 'control')]", "Поле - Описание канала" };
            var xpathFieldSite = new[] { "//div[contains(@class, 'profile-input')]/descendant::input[contains(@class, 'textinput')]", "Поле - Сайт профиля" };
            var xpathElementMessageStatus = new[] { "//p[contains(@class, 'profile-direct-messages') and contains(@class, 'status_type')]", "Элемент, где нужно проверить маркер разрешающий получать сообщения" };
            var xpathButtonMessageOn = new[] { "//p[contains(@class, 'messenger-edit-link')]/descendant::a[contains(@class, 'edit-link')]", "Кнопка - Включить сообщения" };

            while (true)
            {
                // Открыть меню с добавлением публикаций
                Instance.FuncGetFirstHe(xpathButtonAddPublication[0], xpathButtonAddPublication[1], true, true, 5).Click(Instance.ActiveTab, Rnd.Next(500, 1000));

                // Закрыть окно помощника, если оно открыто
                Instance.FuncGetFirstHe(xpathButtonCloseHelper[0], xpathButtonCloseHelper[1], false, false).Click(Instance.ActiveTab, Rnd.Next(150, 500));

                // Создание публикации
                var statusMakePublication = MakePublication(_articlesList[0]);

                // Проверка формы заполнения профиля после 3-х публикаций
                var heButtonShowProfile = Instance.FuncGetFirstHe(xpathButtonShowProfile[0], xpathButtonShowProfile[1], false, false);

                if (!heButtonShowProfile.IsNullOrVoid())
                {
                    // Заполнение поля с описанием канала
                    Instance.FuncGetFirstHe(xpathFieldDescriptionChannel[0], xpathFieldDescriptionChannel[1], true, true, 5).SetValue(Instance.ActiveTab, DescriptionChannel, LevelEmulation.SuperEmulation, Rnd.Next(500, 1000));
                    Logger.Write($"Описание канала успешно установлено", LoggerType.Info, true, false, true);

                    // Заполнение поля с сайтом
                    Instance.FuncGetFirstHe(xpathFieldSite[0], xpathFieldSite[1]).SetValue(Instance.ActiveTab, InstagramUrl, LevelEmulation.SuperEmulation, Rnd.Next(500, 1000));
                    Logger.Write($"Сайт канала успешно установлен", LoggerType.Info, true, false, true);

                    // Включение личных сообщений
                    var heElementForCheckMessageStatus = Instance.FuncGetFirstHe(xpathElementMessageStatus[0], xpathElementMessageStatus[1], true, true);

                    if (!heElementForCheckMessageStatus.GetAttribute("class").Contains("allowed"))
                    {
                        Instance.FuncGetFirstHe(xpathButtonMessageOn[0], xpathButtonMessageOn[1]).Click(Instance.ActiveTab, Rnd.Next(500, 1000));
                        Logger.Write($"Получение сообщений успешно включено", LoggerType.Info, true, false, true);
                    }
                    else Logger.Write($"Получение сообщений уже включено", LoggerType.Info, true, false, false);


                }

                // Счетчик публикаций
                if (_counterPublicationInOneRun >= _numbPublicationInCurrentRun)
                {
                    var numbOfPublicationsMade = Logger.GetAccountLog(LogFilter.ArticlePublicationUnixtime).Count;
                    Logger.Write($"[Всего публикаций сделано: {numbOfPublicationsMade} из {_numbPublicationFull}]\tУспешное завершение", LoggerType.Info, true, false, true, LogColor.Green);
                }
            }
        }

        /// <summary>
        /// Сделать пост.
        /// </summary>
        /// <returns></returns>
        private bool MakePublication(Article article)
        {
            HtmlElement heFieldTitle;

            // Паттерны макросов
            var rxSimpleImage = new Regex(@"\[IMAGE]");
            var rxNamedImage = new Regex(@"\[IMAGE=[\.a-zA-Z0-9]+]");

            // xPath пути элементов
            var xpathFieldTitle = new[] { "//div[contains(@class, 'title-input')]/descendant::span[@data-offset-key!='']/descendant::*[contains(@data-text, 'true')]", "Поле - Заголовок статьи" };
            var xpathParagraphForFieldText = new[] { "//div[contains(@class, 'editorContainer')]/descendant::div[contains(@data-block, 'true') and contains(@class, 'zen-editor-block-paragraph')]", "Поля - Параграфы" };
            var xpathChildFieldText = new[] { ".//span[@data-offset-key!='']/descendant::*[contains(@data-text, 'true')]", "Поле - Ввод текста статьи" };
            var xpathButtonPublish = new[] { "//button[contains(@class, 'editor-header') and contains(@class, 'edit-btn')]", "Кнопка - Опубликовать" };
            var xpathFormPublicationSettings = new[] { "//form[contains(@class, 'publication-settings')]", "Форма - Настройка публикации" };
            var xpathFormPrePublish = new[] { "//div[contains(@class, 'prepublish-profile') and contains(@role, 'dialog')]", "Форма - PrePublish" };

            var xpathForCheck = string.Join("|", new[]
            {
                "//div[contains(@class, 'create-post') and contains(@class, 'zen-header')]/descendant::div[contains(@class, 'zen-header') and contains(@class, 'add-button')]",
                "//div[contains(@class, 'publications-groups-view') and contains(@class, 'btn-wrapper')]/descendant::button[contains(@class, 'base')]",
                "//div[contains(@class, 'publications-root') and contains(@class, 'right-block')]/descendant::button"
            });
            var xpathButtonFirstPublication = new[] { "//div[contains(@class, 'notification') and contains(@class, 'content')]/descendant::button[contains(@class, 'action-button') and text()!='']", "Кнопка - Ясно, спасибо" };

            var counterAttemptsPublication = 0;

            // Получение и ввод заголовка статьи (вызов события Enter после установки значения)
            heFieldTitle = Instance.FuncGetFirstHe(xpathFieldTitle[0], xpathFieldTitle[1], true, false, 7);
            heFieldTitle.SetValue(Instance.ActiveTab, article.TitleArticle, LevelEmulation.Full, Rnd.Next(500, 1000), false, false, true, Rnd.Next(1000, 1500));

            // Ввод статьи
            foreach (var lineText in article.TextArticle)
            {
                var setText = lineText;

                // Поиск последнего поля для ввода текста
                var heFieldText = Instance.FuncGetHeCollection(xpathParagraphForFieldText[0], xpathParagraphForFieldText[1], true, false, 5).Last().FindChildByXPath(xpathChildFieldText[0], 0);

                // Обработка обычных изображений
                if (rxSimpleImage.IsMatch(setText))
                {
                    UploadImageArticle(article.SimpleImagesList);
                    continue;
                }

                // Обработка именнованных изображений
                if (rxNamedImage.IsMatch(setText))
                {
                    UploadImageArticle(article.NamedImagesList);
                    continue;
                }

                // Обработка пустых строк
                if (string.IsNullOrEmpty(setText))
                {
                    Instance.ActiveTab.KeyEvent("Enter", "press", "", true, Rnd.Next(1000, 1500));
                    continue;
                }

                // Ввод текста (вызов события Enter после установки значения)
                heFieldText.SetValue(Instance.ActiveTab, setText, LevelEmulation.Full, Rnd.Next(500, 1000), false, true, true, Rnd.Next(1000, 1500));
            }

            // Переход к форме публикации статьи
            Instance.FuncGetFirstHe(xpathButtonPublish[0], xpathButtonPublish[1], true, false).Click(Instance.ActiveTab, Rnd.Next(1000, 1500));

            while (true)
            {
                if (++counterAttemptsPublication > 5)
                {
                    Logger.Write($"Достигнут лимит попыток публикации поста", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    return false;
                }

                if (!Instance.FuncGetFirstHe(xpathFormPublicationSettings[0], xpathFormPublicationSettings[1], false, false).IsNullOrVoid())
                {
                    Logger.Write($"Заполнение формы настройки публикации...", LoggerType.Info, false, true, true);

                    FillOnFormPublicationSettings();
                }
                else if (!Instance.FuncGetFirstHe(xpathFormPrePublish[0], xpathFormPrePublish[1], false, false).IsNullOrVoid())
                {
                    Logger.Write($"Обнаружена форма PrePublish. Переход к её заполнению...", LoggerType.Info, false, true, true);

                    FillOnFormPrePublish();

                    continue;
                }

                // Выход из цикла при успешной публикации
                if (!Instance.FuncGetFirstHe(xpathForCheck, "", false, false, 5).IsNullOrVoid()) break;
            }

            Logger.Write($"[Publication unixtime: {Logger.GetUnixtime()}]\t[ArticleDirectory:{article.ArticleDirectory.FullName}]\tСтатья успешно опубликована", LoggerType.Info, true, false, true, LogColor.Blue);

            // Закрываем форму "Вы создали первую публикацию", если она есть
            var heButtonFirstPublication = Instance.FuncGetFirstHe(xpathButtonFirstPublication[0], "", false, false, 0);

            if (!heButtonFirstPublication.IsNullOrVoid())
            {
                heButtonFirstPublication.Click(Instance.ActiveTab, Rnd.Next(500, 1000));

                Logger.Write($"Закрыта форма \"Вы создали первую публикацию\" после обнаружения", LoggerType.Info, true, false, true);
            }

            _articlesList.RemoveAt(0);
            _counterPublicationInOneRun++;
            return true;
        }

        /// <summary>
        /// Заполнение формы публикации и публикация статьи.
        /// </summary>
        private void FillOnFormPublicationSettings()
        {
            var xpathTagItem = new[] { "//div[contains(@class, 'recommended-tags') and contains(@class, 'list')]/descendant::span[contains(@class, 'recommended-tags') and contains(@class, 'item')]/descendant::div[@data-value!='']", "Рекомендуемые теги" };
            var xpathButtonSubmit = new[] { "//div[contains(@class, 'modal-actions')]/descendant::button[contains(@type, 'submit')]", "Кнопка - Опубликовать публикацию" };
            var xpathAddedTags = new[] { "//div[contains(@class, 'tag-input') and contains(@class, 'container')]/descendant::div[contains(@class, 'child')]", "Поле - Добавленные теги" };

            try
            {
                var heButtonSubmit = Instance.FuncGetFirstHe(xpathButtonSubmit[0], xpathButtonSubmit[1], true, true, 5);
                var heTags = Instance.FuncGetHeCollection(xpathTagItem[0], xpathTagItem[1], false, false, 5).ToList();

                // Выбираем случайные теги
                if (heTags.Count != 0)
                {
                    var counterSetTags = CalculateNumberOfTags(heTags);

                    while (counterSetTags-- > 0)
                        Instance.FuncGetHeCollection(xpathTagItem[0], xpathTagItem[1], false, true).ToList().GetLine(LineOptions.Random).Click(Instance.ActiveTab, Rnd.Next(500, 1000));
                }
                else Logger.Write($"Не найдено рекомендованных тегов", LoggerType.Warning, true, true, true, LogColor.Yellow);

                // Проверяем наличие добавленных тегов
                var addedTags = Instance.FuncGetHeCollection(xpathAddedTags[0], xpathAddedTags[1], false, false, 5);

                if (addedTags == null || addedTags.Count == 0)
                {
                    Logger.Write($"Не удалось добавить теги", LoggerType.Warning, true, true, true, LogColor.Yellow);
                }
                else
                {
                    Logger.Write($"[Количество тегов: {addedTags.Count}]\tТеги успешно добавлены", LoggerType.Info, true, false, true);
                }

                heButtonSubmit.Click(Instance.ActiveTab, Rnd.Next(5000, 7000));
            }
            catch { return; }
        }

        /// <summary>
        /// Вычислить количество тегов.
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        private int CalculateNumberOfTags(List<HtmlElement> tags)
        {
            var numbTagsUse = (int)Math.Round((double)tags.Count / 100 * _percentageRecommendedTagsForPublicationUse.ExtractNumber());
            return numbTagsUse < 1 ? 1 : numbTagsUse > 5 ? 5 : numbTagsUse;
        }

        /// <summary>
        /// Обработка формы PrePublish (оформление канала/профиля).
        /// </summary>
        private void FillOnFormPrePublish()
        {
            // xPath для формы PrePublish
            var xpathFormPrePublish = new[] { "//div[contains(@class, 'prepublish-profile') and contains(@role, 'dialog')]", "Форма - PrePublish" };
            var xpathChildSelectorEmail = new[] { ".//label[contains(@class, 'profile-menu-email')]/descendant::div[contains(@class, 'dropdown-picker') and contains(@role, 'button')]", "Селектор - Емейл" };
            var xpathChildSelectorOption = new[] { ".//div[contains(@class, 'select')]/descendant::div[contains(@class, 'option')]", "Option - Доступные емейлы" };
            var xpathChildCurrentEmail = new[] { ".//div[contains(@class, 'output-box')]", "Поле - Текущий установленный емейл" };
            var xpathChildCheckbox = new[] { ".//label[contains(@class, 'checkbox')]", "Чекбоксы" };
            var xpathChildButtonSubmit = new[] { ".//button[contains(@class, 'submit') and contains(@type, 'submit')]", "Кнопка - Готово" };

            var counterAttemptSetEmail = 0;

            while (true)
            {
                // Счетчик попыток
                if (++counterAttemptSetEmail > 5)
                {
                    Logger.Write($"Не удалось установить емейл", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    return;
                }

                // Получаем общий div селектора
                var heSelectorEmail = Instance.FuncGetFirstHe(xpathFormPrePublish, false, false).FindChildByXPath(xpathChildSelectorEmail[0], 0);

                if (heSelectorEmail.IsNullOrVoid())
                {
                    Logger.Write($"Не найден селектор емейл", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    continue;
                }

                // Смотрим текущий емейл
                var heCurrentEmail = heSelectorEmail.FindChildByXPath(xpathChildCurrentEmail[0], 0);

                // Проверяем наличие элемента с текущим емейлом
                if (heCurrentEmail.IsNullOrVoid())
                {
                    Logger.Write($"[XPath: {xpathChildCurrentEmail[0]}]\t[{xpathChildCurrentEmail[1]}]\theCurrentEmail - Не найден элемент", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    continue;
                }

                // Выходим из цикла, если емейл установлен
                if (!string.IsNullOrWhiteSpace(heCurrentEmail.GetAttribute("innerhtml")))
                {
                    Logger.Write($"Установленный емейл: {heCurrentEmail.GetAttribute("innerhtml")}", LoggerType.Info, false, true, true);
                    break;
                }

                // Раскрытие селектора, если он скрыт
                if (!heCurrentEmail.GetAttribute("class").Contains("active"))
                    heCurrentEmail.Click(Instance.ActiveTab, Rnd.Next(150, 500));

                // Выбираем емейл
                heSelectorEmail.FindChildByXPath(xpathChildSelectorOption[0], 0).Click(Instance.ActiveTab, Rnd.Next(1000, 1500));
            }

            // Обработка чекбоксов
            Instance.FuncGetFirstHe(xpathFormPrePublish[0], xpathFormPrePublish[1], true, false, 5).FindChildrenByXPath(xpathChildCheckbox[0]).ToList().ForEach(x =>
            {
                if (!x.GetAttribute("class").Contains("is-checked")) x.Click(Instance.ActiveTab, Rnd.Next(500, 1000));
            });

            Instance.FuncGetFirstHe(xpathChildButtonSubmit[0], xpathChildButtonSubmit[1], true, false).Click(Instance.ActiveTab, Rnd.Next(1000, 1500));
        }

        /// <summary>
        /// Загрузка изображения для статьи.
        /// </summary>
        /// <param name="list"></param>
        private void UploadImageArticle(List<FileInfo> list)
        {
            var xpathButtonAddImg = new[] { "//div[contains(@class, 'side-toolbar')]/descendant::button[contains(@class, 'image')]", "Кнопка - Добавить изображение в статью" };
            var xpathButtonUploadImg = new[] { "//div[contains(@class, 'content') and contains(@class, 'image-popup')]/descendant::button[contains(@class, 'file-button')]", "Кнопка - загрузить изображение" };

            Instance.SetFilesForUpload(list.GetLine(LineOptions.FirstWithRemoved), true);

            try
            {
                var counterAttemptSearchElement = 0;

                while (true)
                {
                    if (++counterAttemptSearchElement > 3)
                        throw new Exception("Элемент добавления изображения не найден");

                    try
                    {
                        // Поиск элемента для добавления изображения
                        var heButtonAddImg = Instance.FuncGetFirstHe(xpathButtonAddImg[0], xpathButtonAddImg[1], true, false, 5);

                        // Проверка активности элемента
                        if (!heButtonAddImg.ParentElement.GetAttribute("style").Contains("none"))
                        {
                            heButtonAddImg.Click(Instance.ActiveTab, Rnd.Next(150, 500));
                            break;
                        }
                        else Instance.ActiveTab.KeyEvent("Enter", "press", "", true, Rnd.Next(1000, 1500));
                    }
                    catch { continue; }
                }

                Instance.FuncGetFirstHe(xpathButtonUploadImg[0], xpathButtonUploadImg[1], true, false, 5).Click(Instance.ActiveTab, Rnd.Next(500, 1000));
            }
            catch (Exception ex)
            {
                Logger.Write($"[Exception message: {ex.Message}]\tНе удалось загрузить изображение", LoggerType.Warning, true, true, true, LogColor.Yellow);
            }
        }

        /// <summary>
        /// Получение, обработка и установка данных перед запуском.
        /// </summary>
        /// <returns></returns>
        private bool ResourceHandler()
        {
            if (_numbPublicationFull == 0)
            {
                Logger.Write($"В настройках выставлено \"0\" публикаций всего (глобальный параметр)", LoggerType.Warning, true, true, true, LogColor.Yellow);
                return false;
            }

            if (_numbPublicationInCurrentRun == 0)
            {
                Logger.Write($"В настройках выставлено \"0\" публикаций за один запуск (локальный параметр)", LoggerType.Warning, true, true, true, LogColor.Yellow);
                return false;
            }

            var channelDescriptionFileName = Zenno.Variables["cfgFileNameDescriptionChannel"].Value;
            var articlesFolderName = Zenno.Variables["cfgNameFolderArticles"].Value;

            if (AccountsTable.RowCount == 0)
            {
                Program.StopTemplate(Zenno, $"Таблица с аккаунтами/донорами пуста");
                return false;
            }

            if (string.IsNullOrWhiteSpace(channelDescriptionFileName))
            {
                Program.StopTemplate(Zenno, $"Не указано имя файла с описанием к аккаунту");
                return false;
            }

            if (string.IsNullOrWhiteSpace(articlesFolderName))
            {
                Program.StopTemplate(Zenno, $"Не указано имя папки со статьями");
                return false;
            }

            lock (SyncObjects.TableSyncer)
            {
                var accountsCount = AccountsTable.RowCount;

                for (int row = 0; row < accountsCount; row++)
                {
                    // Получение аккаунта, настройка до.лога, информация о директории и файле описания аккаунта
                    Login = AccountsTable.GetCell((int)TableColumnEnum.Inst.Login, row);

                    ObjectDirectory = new DirectoryInfo(Path.Combine(Zenno.Directory, "Accounts", Login));
                    ChannelDescription = new FileInfo(Path.Combine(ObjectDirectory.FullName, channelDescriptionFileName));
                    _articlesDirectory = new DirectoryInfo(Path.Combine(ObjectDirectory.FullName, articlesFolderName));

                    Logger.SetCurrentObjectForLogText(Login, ObjectTypeEnum.Account);

                    // Проверка на наличия ресурса и его занятость
                    if (!ResourceIsAvailable(Login, row)) continue;

                    // Получение инстаграм ссылки
                    InstagramUrl = AccountsTable.GetCell((int)TableColumnEnum.Inst.InstaUrl, row);

                    if (string.IsNullOrWhiteSpace(InstagramUrl))
                    {
                        Logger.Write($"[Row: {row + 2}]\tОтсутствует ссылка", LoggerType.Info, true, true, true, LogColor.Yellow);
                        continue;
                    }

                    // Проверка на сделанное количество публикаций
                    var madePublications = Logger.GetAccountLog(LogFilter.ArticlePublicationUnixtime);
                    _counterPublicationFull = madePublications.Count;

                    if (_numbPublicationFull >= _counterPublicationFull)
                    {
                        Logger.Write($"[Публикаций всего задано: {_numbPublicationFull}]\tОбработка не требуется. На аккаунте уже сделано заданное количество публикаций", LoggerType.Info, true, false, true, LogColor.Green);
                        continue;
                    }
                    else
                    {
                        var remainsMakePublications = _numbPublicationFull - _counterPublicationFull;
                        _numbPublicationInCurrentRun = _numbPublicationInCurrentRun > remainsMakePublications ? remainsMakePublications : _numbPublicationInCurrentRun;
                    }

                    // Проверка наличия zen канала
                    ZenChannel = AccountsTable.GetCell((int)TableColumnEnum.Inst.ZenChannel, row);

                    if (string.IsNullOrWhiteSpace(ZenChannel))
                    {
                        Logger.Write($"Канал ещё не создан", LoggerType.Info, false, false, true);
                        continue;
                    }

                    // Проверка директории на существование (создать, если требуется)
                    if (!ResourceDirectoryExists()) continue;

                    // Проверка папки со статьями
                    if (!_articlesDirectory.Exists)
                    {
                        Logger.Write($"[Папка: {_articlesDirectory.FullName}]\tПапки не существует", LoggerType.Warning, true, true, true, LogColor.Yellow);
                        continue;
                    }
                    else
                    {
                        var dirAllArticles = _articlesDirectory.EnumerateDirectories("*", SearchOption.TopDirectoryOnly);

                        if (dirAllArticles.Count() == 0)
                        {
                            Logger.Write($"[Папка: {_articlesDirectory.FullName}]\tПапка пуста", LoggerType.Warning, true, true, true, LogColor.Yellow);
                            continue;
                        }

                        foreach (var articleFolder in dirAllArticles)
                        {
                            var article = new Article(articleFolder);

                            if (!article.IsVoid)
                            {
                                // Проверка стати на актуальность (публиковалась или нет)
                                if (madePublications.Any(x => Regex.Match(x, @"(?<=\[ArticleDirectory:).*?(?=])").Value == article.ArticleDirectory.FullName))
                                {
                                    Logger.Write($"[Папка: {_articlesDirectory.FullName}]\tСтатья уже использовалась. Пропуск", LoggerType.Info, true, false, false);
                                    continue;
                                }

                                _articlesList.Add(article);
                            }
                        }

                        if (_articlesList.Count < _numbPublicationInCurrentRun)
                        {
                            Logger.Write($"Не достаточно ", LoggerType.Warning, true, true, true, LogColor.Yellow);
                            continue;
                        }
                    }

                    // Проверка файла с описанием канала
                    if (!ChannelDescription.Exists)
                    {
                        Logger.Write($"[Row: {row + 2}]\t[Файл: {ChannelDescription.FullName}]\tФайл с описанием канала не найден", LoggerType.Warning, true, true, true, LogColor.Yellow);
                        continue;
                    }
                    else if (File.ReadAllLines(ChannelDescription.FullName, Encoding.UTF8).Length == 0)
                    {
                        Logger.Write($"[Файл: {ChannelDescription.FullName}]\tВ файле отсутствует описание канала", LoggerType.Warning, true, true, true, LogColor.Yellow);
                        continue;
                    }
                    else
                    {
                        DescriptionChannel = string.Join("\n", File.ReadAllLines(ChannelDescription.FullName, Encoding.UTF8));

                        if (string.IsNullOrWhiteSpace(DescriptionChannel))
                        {
                            Logger.Write($"[Файл: {ChannelDescription.FullName}]\tВ файле отсутствует описание канала", LoggerType.Warning, true, true, true, LogColor.Yellow);
                            continue;
                        }
                    }

                    // Получение и загрузка профиля
                    if (!ProfileWorker.LoadProfile(true)) return false;

                    // Получение прокси для регистрации
                    if (!SetProxy((int)TableColumnEnum.Inst.Proxy, row, true)) continue;

                    //ProfileRetrievedFromSharedFolder

                    // Успешное получение ресурса
                    ProjectDataStore.ResourcesCurrentThread.Add(Login);
                    ProjectDataStore.ResourcesAllThreadsInWork.Add(Login);
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
}
