namespace Yandex.Zen.Core.Toolkit.TableTool.Enums
{
    /// <summary>
    /// Столбцы.
    /// </summary>
    public static class TableColumnEnum
    {
        /// <summary>
        /// Столбцы для инстаграмной имплементации.
        /// </summary>
        public enum Inst
        {
            Profile = 0,
            StatusAll = 1,
            InstaUrl = 2,
            ZenChannel = 3,
            ChannelName = 4,
            FirstAndLastName = 5,
            Login = 6,
            Password = 7,
            PhoneNumber = 8,
            Answer = 9,
            Proxy = 10,
            AccountDatetimeCreated = 11,
            ChannelDatetimeCreated = 12,
            DatetimeLastWalkingOnZen = 13,
            DatetimeLastPublicationArticle = 14
        }

        /// <summary>
        /// Столбцы для статистики накрутки.
        /// </summary>
        public enum StatisticsCheatActivity
        {
            Login = 0,
            ArticleUrl = 1,
            IndividualComments = 2,
            WatchTime = 3,
            Transitions = 4,
            Likes = 5,
            Comments = 6,
            ProcessStatus = 7
        }

        /// <summary>
        /// Столбцы для таблицы режима PostingSecondWind.
        /// </summary>
        public enum PostingSecondWind
        {
            Profile,
            Login,
            Password,
            AnswerQuestion,
            AccountNumberPhone,
            ChannelNumberPhone,
            Proxy,
            ChannelUrl,
            IndexationStatus
        }
    }
}
