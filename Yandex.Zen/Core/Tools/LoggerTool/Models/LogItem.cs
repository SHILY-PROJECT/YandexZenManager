using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Enums;
using Yandex.Zen.Core.Services;
using Yandex.Zen.Core.Tools.LoggerTool.Enums;

namespace Yandex.Zen.Core.Tools.LoggerTool.Models
{
    public class LogItem
    {
        /// <summary>
        /// Тип лога (автоматически заполняемое).
        /// </summary>
        public string LogType { get; set; }

        /// <summary>
        /// Дата и время лога (автоматически заполняемое).
        /// </summary>
        public TimeData Time { get; set; }

        /// <summary>
        /// Режим работы шаблона (автоматически заполняемое).
        /// </summary>
        public ProgramModeEnum TemplateMode { get; set; }

        /// <summary>
        /// Данные о текущим ресурсе в работе (автоматически заполняемое).
        /// </summary>
        public ResourceData Resource { get; set; }

        /// <summary>
        /// Сообщение лога (нужно заполнять)
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Информация анализа ошибки.
        /// </summary>
        public ErrorAnalysisData ErrorAnalysis { get; set; }

        public LogItem(string message, LoggerType loggerType)
        {
            Message = message;
            TemplateMode = Program.ProgramMode;
            LogType = loggerType.ToString().ToUpper();
            Time = new TimeData();
            Resource = new ResourceData();
        }

        public LogItem(string message, LoggerType loggerType, ErrorAnalysisData errorAnalysisData)
        {
            Message = message;
            TemplateMode = Program.ProgramMode;
            LogType = loggerType.ToString().ToUpper();
            ErrorAnalysis = new ErrorAnalysisData(errorAnalysisData) ?? null;
            Time = new TimeData();
            Resource = new ResourceData();
        }
    }
}
