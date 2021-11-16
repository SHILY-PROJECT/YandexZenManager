using Yandex.Zen.Core.Enums.CheatActivity;

namespace Yandex.Zen.Core.Models.CheatActivity
{
    public class ArticleTableStatisticsModel
    {
        public int TotalGoToArticle { get; set; }
        public int TotalLikes { get; set; }
        public int TotalComments { get; set; }
        public int TotalSecondsWatchArticle { get; set; }
        public ArticleProcessStatusEnum ArticleProcessStatus { get; set; }
    }
}
