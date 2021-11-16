using Global.ZennoLab.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Enums.Extensions;
using Yandex.Zen.Core.Tools.Extensions;
using Yandex.Zen.Core.Tools;
using Yandex.Zen.Core.Enums.Logger;

namespace Yandex.Zen.Core.Models.CheatActivity
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
                var taskList = TaskHandler.GetTasksList();

                for (int i = 0; i < taskList.Count; i++)
                {
                    if (TaskId == taskList[i].TaskId)
                    {
                        taskList[i] = this;
                    }
                }

                File.WriteAllText(TaskHandler.TaskFile.FullName, JsonConvert.SerializeObject(taskList, Formatting.Indented), Encoding.UTF8);
            }
        }

    }
}
