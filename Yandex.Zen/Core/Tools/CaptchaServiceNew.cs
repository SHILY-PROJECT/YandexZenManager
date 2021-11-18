using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Tools.LoggerTool;
using Yandex.Zen.Core.Tools.LoggerTool.Enums;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace Yandex.Zen.Core.Tools
{
    public class CaptchaServiceNew
    {
        public string ServiceDll { get; set; }

        public CaptchaServiceNew(string dllName)
        {
            ServiceDll = dllName;
        }

        public CaptchaServiceNew(ILocalVariable dllName) : this(dllName.Value) { }


        /// <summary>
        /// Отправка капчи на распознание (ошибки логирует автоматически).
        /// </summary>
        /// <param name="htmlElementImgCaptcha"></param>
        /// <returns>Возвращение результата распознавания.</returns>
        public string Recognize(HtmlElement htmlElementImgCaptcha)
        {
            string captchaResult;

            try
            {
                // Скачивание капчи в байтики для отправки на распознавание
                var btImg = new System.Net.WebClient().DownloadData(htmlElementImgCaptcha.GetAttribute("src"));

                Logger.Write($"Отправка капчи на распознавание", LoggerType.Info, true, false, true, LogColor.Default);

                // Отправка капчи на распознание
                var responseCaptcha = ZennoPoster.CaptchaRecognition(ServiceDll, Convert.ToBase64String(btImg), "");

                // Получение результата распознавания
                captchaResult = responseCaptcha.Split(new[] { "-|-" }, StringSplitOptions.None)[0];

                // Проверка результата распознавания
                if (string.IsNullOrWhiteSpace(captchaResult))
                {
                    //throw new Exception($"[CaptchaResponse: {responseCaptcha}]\tПустой результат разгадывания капчи");
                    Logger.Write($"[Captcha response: {responseCaptcha}]\tПустой результат распознавания капчи", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    return null;
                }

                Logger.Write($"Результат распознавания: {captchaResult}", LoggerType.Info, true, false, true, LogColor.Default);
            }
            catch (Exception ex)
            {
                Logger.Write($"[Exception message: {ex.Message}]\tУпало исключение во время распознавания капчи", LoggerType.Warning, true, true, true, LogColor.Yellow);
                return null;
            }

            return captchaResult;
        }
    }
}
