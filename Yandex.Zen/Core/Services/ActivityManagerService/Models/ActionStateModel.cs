using System;

namespace Yandex.Zen.Core.Services.ActivityManagerService.Models
{
    public class ActionStateModel
    {
        public bool GoToArticle { get; set; }
        public bool Likes { get; set; }
        public bool Comments { get; set; }
        public int SecondsWatchArticle { get; set; }

        public ActionStateModel()
        {
            Array.ForEach(new[] { GoToArticle, Likes, Comments }, x => x = default);
            SecondsWatchArticle = default;
        }
    }
}
