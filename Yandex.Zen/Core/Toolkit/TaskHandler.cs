using Global.ZennoLab.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Services.CheatActivityService.Models;
using Yandex.Zen.Core.Toolkit.Extensions;
using Yandex.Zen.Core.Toolkit.Extensions.Enums;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;

namespace Yandex.Zen.Core.Toolkit
{
    public static class TaskHandler
    {
        [ThreadStatic]
        public static FileInfo TaskFile;

        [ThreadStatic]
        private static DirectoryInfo AccountArticleDirectory;

        [ThreadStatic]
        private static string ArticleUrl;

        /// <summary>
        /// Установка TaskFile для получения и создания задач для дальнейшей работы.
        /// </summary>
        /// <param name="accountArticleDirectory"></param>
        public static void ConfigureTaskHandler(ArticleBasicModel accountArticle, string totalGoToArticleSettings, string secondsWatchArticleSettings, string totalLikesSettings, string totalCommentsSettings)
        {
            AccountArticleDirectory = accountArticle.Directory;
            TaskFile = new FileInfo(Path.Combine(accountArticle.Directory.FullName, "_logger", $"all_tasks.ini"));
            ArticleUrl = accountArticle.ArticleUrl;

            if (!TaskFile.Exists) TaskFile.Create();

            if (GetTasksList().Count == 0)
            {
                var totalGoToArticle = totalGoToArticleSettings.ExtractNumber();
                var totalLikes = totalLikesSettings.ExtractNumber();
                var totalComments = totalCommentsSettings.ExtractNumber();

                CreateTasks(totalGoToArticle, totalLikes, totalComments, secondsWatchArticleSettings);
            }
        }

        /// <summary>
        /// Получение задачи из списка и добавление её в ресурсы.
        /// </summary>
        /// <param name="taskList"></param>
        /// <returns></returns>
        public static TaskItemModel GetTask()
        {
            if (TaskFile == null)
            {
                Logger.Write(AccountArticleDirectory, $"Не установлен \"TaskFile\". Используйте \"TaskHandler -> ConfigureTaskHandler\" для установки \"TaskFile\"", LoggerType.Info, true, true, true);
                return null;
            }

            // Получение всех задач
            var taskList = GetTasksList();

            // Фильтрация незавершенных задач
            var taskNotCompleteList = taskList.Where(x => x.TaskIsComplete == false).ToList();

            if (taskNotCompleteList.Count == 0)
            {
                Logger.Write(AccountArticleDirectory, $"[{AccountArticleDirectory.FullName}]\tВсе задачи по текущей статье завершены: {ArticleUrl}", LoggerType.Info, true, true, true);
                /*
                    todo - Добавить сохранение данных о завершенной задачи в таблицу (считать завершенной задачей, если общее количество задач попадает в нижние границы диапазона настроек)
                */
                return null;
            }

            // Фильтрация свободных задач
            taskNotCompleteList = taskNotCompleteList.Where(x => !ProjectDataStore.CurrentObjectsOfAllThreadsInWork.Any(res => res == x.TaskId)).ToList();

            if (taskNotCompleteList.Count == 0)
            {
                Logger.Write(AccountArticleDirectory, $"[{AccountArticleDirectory.FullName}]\tВсе задачи по текущей статье заняты другими потоками: {ArticleUrl}", LoggerType.Info, true, true, true);
                return null;
            }

            var taskItem = taskList.GetLine(LineOptions.Random);

            ProjectDataStore.CurrentObjectCache.Add(taskItem.TaskId);
            ProjectDataStore.CurrentObjectsOfAllThreadsInWork.Add(taskItem.TaskId);

            return taskItem;
        }

        /// <summary>
        /// Получение всего списка задач.
        /// </summary>
        /// <returns></returns>
        public static List<TaskItemModel> GetTasksList() =>
            JsonConvert.DeserializeObject<List<TaskItemModel>>(File.ReadAllText(TaskFile.FullName, Encoding.UTF8)) ?? new List<TaskItemModel>();

        /// <summary>
        /// Создание списка задач.
        /// </summary>
        private static void CreateTasks(int totalGoToArticle, int totalLikes, int totalComments, string secondsWatchArticleSettings)
        {
            var taskList = new List<TaskItemModel>();

            totalLikes = totalGoToArticle >= totalLikes ? totalLikes : totalGoToArticle;
            totalComments = totalGoToArticle >= totalComments ? totalComments : totalGoToArticle;

            while (totalGoToArticle-- > 0)
            {
                taskList.Add(new TaskItemModel
                {
                    TaskId = ServicesDataAndComponents.Rnd.Next(100000000, 999999999).ToString(),
                    GoToArticle = true,
                    SecondsWatchArticle = secondsWatchArticleSettings.ExtractNumber(),
                    Like = totalLikes != 0,
                    Comment = totalComments != 0,
                    TaskIsComplete = false
                });

                if (totalLikes != 0) totalLikes--;
                if (totalComments != 0) totalComments--;
            }

            File.WriteAllText(TaskFile.FullName, JsonConvert.SerializeObject(taskList, Formatting.Indented), Encoding.UTF8);
        }

    }
}
