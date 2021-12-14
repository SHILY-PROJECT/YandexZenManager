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
                var btImg = new System.Net.WebClient().DownloadData(htmlElementImgCaptcha.GetAttribute("src"));
                var captchaResponse = ZennoPoster.CaptchaRecognition(ServiceDll, Convert.ToBase64String(btImg), "");
                captchaResult = captchaResponse.Split(new[] { "-|-" }, StringSplitOptions.None)[0];

                if (string.IsNullOrWhiteSpace(captchaResult))
                {
                    _logMessage = $"'{nameof(captchaResponse)}' - response is void";
                    return false;
                }

                _logMessage = $"Результат разгадывания: {captchaResult}";
                return true;
            }
            catch (Exception ex)
            {
                _logMessage = $"'{nameof(ex.Message)}:{ex.Message}' - exception error";
            }

            captchaResult = null;
            return false;
        }
    }
}
