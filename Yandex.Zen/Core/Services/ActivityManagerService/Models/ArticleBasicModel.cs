using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZennoLab.InterfacesLibrary.ProjectModel;
using Global.ZennoExtensions;
using Global.ZennoLab.Json;
using ZennoLab.InterfacesLibrary.Enums.Log;
using Yandex.Zen.Core.Toolkit;
using Yandex.Zen.Core.Services.ActivityManagerService.Enums;
using Yandex.Zen.Core.Toolkit.TableTool.Enums;

namespace Yandex.Zen.Core.Services.ActivityManagerService.Models
{
    public class ArticleBasicModel
    {
        private static readonly object _locker = new object();

        private readonly FileInfo _actionsFile;

        public string Login { get; set; }
        public string ArticleUrl { get; set; }
        public DirectoryInfo Directory { get; set; }
        public ArticleProcessStatusEnum ArticleProcessStatus { get; set; }
        public ActionStateModel ActionState { get; set; }
        public TaskItemModel Task { get; set; }
        public CommentData CommentData { get; set; }

        public ArticleBasicModel(IZennoTable table, int row, string totalGoToArticleSettings, string secondsWatchArticleSettings, string totalLikesSettings, string totalCommentsSettings)
        {
            var zenno = ServicesDataAndComponents_obsolete.Zenno;

            Login = table.GetCell((int)TableColumnEnum.StatisticsCheatActivity.Login, row);
            Login = !string.IsNullOrWhiteSpace(Login) ? Login : null;

            if (Login == null) return;

            Directory = new DirectoryInfo(Path.Combine(zenno.Directory, "Accounts", Login));
            _actionsFile = new FileInfo(Path.Combine(Directory.FullName, "_logger", $"actions.log"));

            ArticleUrl = table.GetCell((int)TableColumnEnum.StatisticsCheatActivity.ArticleUrl, row);
            ArticleUrl = !string.IsNullOrWhiteSpace(ArticleUrl) ? ArticleUrl : null;

            if (ArticleUrl == null) return;

            var statistics = GetStatisticsFromTable(table, row);

            if (statistics.ArticleProcessStatus == ArticleProcessStatusEnum.Done) return;

            // Настройка и получение задачи
            ActionState = new ActionStateModel();
            TaskHandler.ConfigureTaskHandler(this, totalGoToArticleSettings, secondsWatchArticleSettings, totalLikesSettings, totalCommentsSettings);
            Task = TaskHandler.GetTask();
        }

        private ArticleTableStatisticsModel GetStatisticsFromTable(IZennoTable table, int row)
        {
            var statistics = new ArticleTableStatisticsModel();

            var sumSeconds = 0;
            var watchTime = table.GetCell((int)TableColumnEnum.StatisticsCheatActivity.WatchTime, row);
            var statusMin = int.TryParse(watchTime.Split(new[] { " min" }, StringSplitOptions.None)[0], out int min);
            var statusSec = int.TryParse(watchTime.Split(new[] { " min" }, StringSplitOptions.None)[1].Split(new[] { " sec" }, StringSplitOptions.None)[0], out int sec);

            if (statusMin) sumSeconds += min * 60;
            if (statusSec) sumSeconds += sec;

            statistics.TotalSecondsWatchArticle = sumSeconds;

            var statusTransitions = int.TryParse(table.GetCell((int)TableColumnEnum.StatisticsCheatActivity.Transitions, row), out int transitions);
            statistics.TotalGoToArticle = statusTransitions ? transitions : 0;

            var statusLikes = int.TryParse(table.GetCell((int)TableColumnEnum.StatisticsCheatActivity.Likes, row), out int likes);
            statistics.TotalLikes = statusLikes ? likes : 0;

            var statusComments = int.TryParse(table.GetCell((int)TableColumnEnum.StatisticsCheatActivity.Comments, row), out int comments);
            statistics.TotalComments = statusComments ? comments : 0;

            var statusArticleState = Enum.TryParse(table.GetCell((int)TableColumnEnum.StatisticsCheatActivity.ArticleUrl, row), out ArticleProcessStatusEnum articleState);
            statistics.ArticleProcessStatus = statusArticleState ? articleState : ArticleProcessStatusEnum.InProcess;

            return statistics;
        }

        public void Save(IZennoTable table)
        {
            lock (SyncObjects.TableSyncer)
            {
                for (int row = 0; row < table.RowCount;)
                {
                    if (Login == table.GetCell((int)TableColumnEnum.StatisticsCheatActivity.Login, row))
                    {
                        // Обновляем данные
                        var statistics = GetStatisticsFromTable(table, row);
                        var allTasks = TaskHandler.GetTasksList();

                        var totalLikes = allTasks.Where(x => x.Like == true).Count();
                        var totalComments = allTasks.Where(x => x.Comment == true).Count();
                        var totalGoToArticle = allTasks.Where(x => x.GoToArticle == true).Count();

                        // Добавляем результат потока
                        statistics.TotalSecondsWatchArticle += ActionState.SecondsWatchArticle;
                        statistics.TotalGoToArticle += ActionState.GoToArticle ? 1 : 0;
                        statistics.TotalLikes += ActionState.Likes ? 1 : 0;
                        statistics.TotalComments += ActionState.Comments ? 1 : 0;

                        // Сохранение времени просмотра
                        var time = TimeSpan.FromSeconds(statistics.TotalSecondsWatchArticle);
                        table.SetCell((int)TableColumnEnum.StatisticsCheatActivity.WatchTime, row, $"{time.Minutes} min {time.Seconds} sec");

                        // Сохранение количества переходов по ссылке
                        table.SetCell((int)TableColumnEnum.StatisticsCheatActivity.Transitions, row, statistics.TotalGoToArticle.ToString());

                        // Сохранение количества лайков
                        table.SetCell((int)TableColumnEnum.StatisticsCheatActivity.Likes, row, statistics.TotalLikes.ToString());

                        // Сохранение количества комментариев
                        table.SetCell((int)TableColumnEnum.StatisticsCheatActivity.Comments, row, statistics.TotalComments.ToString());

                        // Сохранение состояния
                        table.SetCell((int)TableColumnEnum.StatisticsCheatActivity.ProcessStatus, row,
                            statistics.TotalLikes >= totalLikes && statistics.TotalComments >= totalComments && statistics.TotalGoToArticle >= totalGoToArticle ?
                            ArticleProcessStatusEnum.Done.ToString() : ArticleProcessStatusEnum.InProcess.ToString());

                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Получение лога совершенных действий из файла.
        /// </summary>
        /// <param name="loginFilter"></param>
        /// <returns></returns>
        public List<ActionModel> GetActionsList(string loginFilter = "") => !string.IsNullOrWhiteSpace(loginFilter) ?
            JsonConvert.DeserializeObject<List<ActionModel>>($"[{string.Join(",", File.ReadAllLines(_actionsFile.FullName, Encoding.UTF8))}]").Where(x => x.Login == loginFilter).ToList() :
            JsonConvert.DeserializeObject<List<ActionModel>>($"[{string.Join(",", File.ReadAllLines(_actionsFile.FullName, Encoding.UTF8))}]");

        /// <summary>
        /// Запись действия в лог лист совершенных действий.
        /// </summary>
        /// <param name="accountAction"></param>
        public void WriteToActionList(ActionModel accountAction)
        {
            lock (_locker)
                File.AppendAllLines(_actionsFile.FullName, new[] { JsonConvert.SerializeObject(accountAction, Formatting.None) }, Encoding.UTF8);
        }
    }
}
