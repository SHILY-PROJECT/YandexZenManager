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
        private string _serviceDll;

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
                if (Regex.IsMatch(value, @"[0-9a-zA-Z]+\.dll.*?") is false)
                    throw new Exception("Captcha service is not formatted correctly");
                _serviceDll = value;
            }
        }

        /// <summary>
        /// Отправка капчи на распознание (ошибки логирует автоматически).
        /// </summary>
        /// <param name="htmlElementImgCaptcha"></param>
        /// <returns>Возвращение результата распознавания.</returns>
        public bool TryRecognizing(HtmlElement htmlElementImgCaptcha, out string captchaResult)
        {
            try
            {
                // Скачивание капчи в байтики для отправки на распознавание
                var btImg = new System.Net.WebClient().DownloadData(htmlElementImgCaptcha.GetAttribute("src"));

                Logger.Write($"Отправка капчи на распознавание", LoggerType.Info, true, false, true, LogColor.Default);

                // Отправка капчи на распознание
                var captchaResponse = ZennoPoster.CaptchaRecognition(ServiceDll, Convert.ToBase64String(btImg), "");

                // Получение результата распознавания
                captchaResult = captchaResponse.Split(new[] { "-|-" }, StringSplitOptions.None)[0];

                // Проверка результата распознавания
                if (string.IsNullOrWhiteSpace(captchaResult))
                {
                    Logger.Write($"'{nameof(captchaResponse)}' - response is void", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    return false;
                }

                Logger.Write($"Результат распознавания: {captchaResult}", LoggerType.Info, true, false, true, LogColor.Default);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Write($"'{nameof(ex.Message)}:{ex.Message}' - exception error", LoggerType.Warning, true, true, true, LogColor.Yellow);
            }

            captchaResult = null;
            return false;
        }
    }
}
