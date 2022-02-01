using ZennoLab.InterfacesLibrary.Enums.Log;

namespace Yandex.Zen.Core.Toolkit.LoggerTool.Models
{
    public class LogSettings
    {
        /// <summary>
        /// Главный лог.
        /// </summary>
        public bool General { get; set; }

        /// <summary>
        /// Лог ресурса.
        /// </summary>
        public bool Resource { get; set; }

        /// <summary>
        /// Отправка в лог ZennoPoster.
        /// </summary>
        public bool ZennoPoster { get; set; }

        /// <summary>
        /// Цвет лога в ZennoPoster.
        /// </summary>
        public LogColor LogColor { get; set; }

        /// <summary>
        /// Получение состояние, нужно писать в лог.
        /// </summary>
        public bool IsNeedful => General || Resource || ZennoPoster;

        public LogSettings() { }

        public LogSettings(bool writeToGeneralLog = false, bool writeToResourceLog = false, bool sendToZennoPosterLog = false, LogColor logColor = LogColor.Default)
        {
            General = writeToGeneralLog;
            Resource = writeToResourceLog;
            ZennoPoster = sendToZennoPosterLog;
            LogColor = logColor;
        }
    }
}
