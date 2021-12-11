using Yandex.Zen.Core.Services.ActivityManagerService.Enums;

namespace Yandex.Zen.Core.Services.ActivityManagerService.Models
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
