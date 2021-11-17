using Global.ZennoLab.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Tools.LoggerTool;
using Yandex.Zen.Core.Tools.LoggerTool.Enums;
using Yandex.Zen.Core.Tools.LoggerTool.Models;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.Macros;

namespace Yandex.Zen.Core.Models.CheatActivity
{
    public class CommentData
    {
        private static readonly object _locker = new object();

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

            lock (_locker)
                File.AppendAllLines(_usedCommentsFile.FullName, new[] { comment }, Encoding.UTF8);
        }
    }
}
