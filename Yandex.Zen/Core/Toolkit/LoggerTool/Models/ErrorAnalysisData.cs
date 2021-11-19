using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yandex.Zen.Core.Toolkit.LoggerTool.Models
{
    /// <summary>
    /// Класс для хранения данных анализа ошибки.
    /// </summary>
    public class ErrorAnalysisData
    {
        public string OtherInfo { get; set; }
        public bool SaveDomText { get; set; }
        public bool SaveSourceText { get; set; }
        public bool Screenshot { get; set; }
        public string Dir { get; set; }

        public ErrorAnalysisData()
        {

        }

        public ErrorAnalysisData(ErrorAnalysisData errorAnalysisData)
        {
            Dir = errorAnalysisData.Dir;
            OtherInfo = errorAnalysisData.OtherInfo;
            SaveDomText = errorAnalysisData.SaveDomText;
            SaveSourceText = errorAnalysisData.SaveSourceText;
            Screenshot = errorAnalysisData.Screenshot;
        }
    }
}
