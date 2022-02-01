using System;

namespace Yandex.Zen.Core.Toolkit.TableTool.Enums
{
    [Obsolete]
    /// <summary>
    /// Столбцы.
    /// </summary>
    public static class TableColumnEnum_obsolete
    {
        [Obsolete]
        /// <summary>
        /// Столбцы для инстаграмной имплементации.
        /// </summary>
        public enum Inst_obsolete
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

        [Obsolete]
        /// <summary>
        /// Столбцы для статистики накрутки.
        /// </summary>
        public enum StatisticsCheatActivity_obsolete
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

        [Obsolete]
        /// <summary>
        /// Столбцы для таблицы режима PostingSecondWind.
        /// </summary>
        public enum PostingSecondWind_obsolete
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
