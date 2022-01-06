﻿using System.IO;
using System.Text;
using Global.ZennoLab.Json;
using Yandex.Zen.Core.Toolkit;

namespace Yandex.Zen.Core.Services.ActivityManagerService.Models
{
    public class TaskItemModel
    {
        private readonly object _locker = new object();

        public string TaskId { get; set; }
        public bool GoToArticle { get; set; }
        public int SecondsWatchArticle { get; set; }
        public bool Like { get; set; }
        public bool Comment { get; set; }
        public bool TaskIsComplete { get; set; }

        /// <summary>
        /// Сохранить состояние текущей задачи.
        /// </summary>
        public void SaveState()
        {
            lock (_locker)
            {
                var taskList = TaskHandler_obsolete.GetTasksList();

                for (int i = 0; i < taskList.Count; i++)
                {
                    if (TaskId == taskList[i].TaskId)
                    {
                        taskList[i] = this;
                    }
                }

                File.WriteAllText(TaskHandler_obsolete.TaskFile.FullName, JsonConvert.SerializeObject(taskList, Formatting.Indented), Encoding.UTF8);
            }
        }

    }
}
