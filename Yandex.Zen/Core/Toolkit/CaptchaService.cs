using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace Yandex.Zen.Core.Toolkit
{
    public class CaptchaService
    {
        private string _logMessage = string.Empty;
        private string _serviceDll;

        public string LogMessage { get => _logMessage; }

        /// <summary>
        /// Название библиотеки сервиса.
        /// </summary>
        public string ServiceDll
        {
            get
            {
                if (_serviceDll is null)
                    throw new Exception($"'{nameof(ServiceDll)}' - dll name not set");
                return _serviceDll;
            }
            set
            {
                if (!Regex.IsMatch(value, @"[0-9a-zA-Z]+\.dll.*?"))
                    throw new Exception("Captcha service is not formatted correctly");
                _serviceDll = value;
            }
        }

        /// <summary>
        /// Отправка капчи на распознание.
        /// </summary>
        public string Recognizing(HtmlElement htmlElementImgCaptcha)
        {
            TryRecognize(htmlElementImgCaptcha, out var result);
            return result;
        }

        /// <summary>
        /// Отправка капчи на распознание.
        /// </summary>
        public bool TryRecognize(HtmlElement htmlElementImgCaptcha, out string captchaResult)
        {
            _logMessage = string.Empty;

            try
            {
                // Скачивание капчи в байтики для отправки на распознавание
                var btImg = new System.Net.WebClient().DownloadData(htmlElementImgCaptcha.GetAttribute("src"));

                //if (logger) Logger.Write($"Отправка капчи на разгадывание", LoggerType.Info, true, false, true, LogColor.Default);

                // Отправка капчи на распознание
                var captchaResponse = ZennoPoster.CaptchaRecognition(ServiceDll, Convert.ToBase64String(btImg), "");

                // Получение результата распознавания
                captchaResult = captchaResponse.Split(new[] { "-|-" }, StringSplitOptions.None)[0];

                // Проверка результата распознавания
                if (string.IsNullOrWhiteSpace(captchaResult))
                {
                    //if (logger) Logger.Write($"'{nameof(captchaResponse)}' - response is void", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    _logMessage = $"'{nameof(captchaResponse)}' - response is void";
                    return false;
                }

                //if (logger) Logger.Write($"Результат разгадывания: {captchaResult}", LoggerType.Info, true, false, true, LogColor.Default);
                _logMessage = $"Результат разгадывания: {captchaResult}";
                return true;
            }
            catch (Exception ex)
            {
                //if (logger) Logger.Write($"'{nameof(ex.Message)}:{ex.Message}' - exception error", LoggerType.Warning, true, true, true, LogColor.Yellow);
                _logMessage = $"'{nameof(ex.Message)}:{ex.Message}' - exception error";
            }

            captchaResult = null;
            return false;
        }
    }
}
