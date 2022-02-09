using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Global.ZennoLab.Json;
using Global.ZennoExtensions;
using ZennoLab.Macros;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using Yandex.Zen.Core.Toolkit.LoggerTool.Models;

namespace Yandex.Zen.Core.Services.ActivityManagerService.Models
{
    public class CommentData
    {
        private readonly FileInfo _usedCommentsFile;

        public string Login { get; private set; }
        public string CommentText { get; set; }
        public TimeData TimeData { get; private set; }

        /// <summary>
        /// Создание объекта комментария (получение комментария для статьи).
        /// </summary>
        /// <param name="accountArticleData"></param>
        /// <param name="comments"></param>
        public CommentData(ArticleBasicModel accountArticleData, List<string> comments)
        {
            CommentText = default;
            Login = accountArticleData.Login;
            _usedCommentsFile = new FileInfo(Path.Combine(accountArticleData.Directory.FullName, "_logger", $"used_comments.log"));

            var usedCommentsList = GetUsedCommentsList();

            for (int i = 0; i < comments.Count; i++)
            {
                var commentText = TextProcessing.Spintax(comments[i], true);

                if (!string.IsNullOrWhiteSpace(commentText) && !usedCommentsList.Any(x => commentText == x.CommentText))
                {
                    CommentText = commentText;

                    Logger.Write(accountArticleData.Directory, "Комментарий успешно получен", LoggerType.Info, true, false, false);

                    return;
                }
            }
        }

        /// <summary>
        /// Получение использованных комментариев.
        /// </summary>
        /// <returns></returns>
        private List<CommentData> GetUsedCommentsList() =>
            JsonConvert.DeserializeObject<List<CommentData>>($"[{string.Join(",", File.ReadAllLines(_usedCommentsFile.FullName, Encoding.UTF8))}]");

        /// <summary>
        /// Запись в лог использованного комментария для текущей статьи (что потом сверять и не использовать повторно).
        /// </summary>
        public void WriteToUsedCommentsLog()
        {
            TimeData = new TimeData();

            var comment = JsonConvert.SerializeObject(this, Formatting.None);

            lock (SyncObjects.InputSyncer)
                File.AppendAllLines(_usedCommentsFile.FullName, new[] { comment }, Encoding.UTF8);
        }
    }
}
