using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yandex.Zen.Core.Tools.LoggerTool.Models
{
    /// <summary>
    /// Класс для хранения данных времени.
    /// </summary>
    public class TimeData
    {
        /// <summary>
        /// Дата и время лога (автоматически заполняемое).
        /// </summary>
        public string DateTime { get; set; }

        /// <summary>
        /// Дата и время в формате unixtime (автоматически заполняемое).
        /// </summary>
        public int UnixTime { get; set; }

        public TimeData()
        {
            DateTime = $"{System.DateTime.Now:yyyy-MM-dd   HH-mm-ss}";
            UnixTime = (int)(System.DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
        }
    }
}
