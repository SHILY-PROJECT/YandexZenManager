using System;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.Enums.Log;
using Yandex.Zen.Core.Models.ResourceModels;
using Yandex.Zen.Core.Toolkit;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using Yandex.Zen.Core.Toolkit.BrowserCustomizer;
using Yandex.Zen.Core.Toolkit.BrowserCustomizer.Models;
using Yandex.Zen.Core.Toolkit.BrowserCustomizer.Enums;
using Yandex.Zen.Core.Services.PublicationManagerSecondWindService.Models;
using Yandex.Zen.Core.Toolkit.SmsServiceTool;

namespace Yandex.Zen.Core.Services.CommonComponents
{
    public class AuthorizationNew
    {
        [ThreadStatic] private static BrowserBusySettingsModel _settingsMode;
        private static Random Rnd { get; set; } = new Random();


        #region [ВНЕШНИЕ РЕСУРСЫ]===================================================
        private static DataManager Data { get => DataManager.Data; }
        private static Instance Browser { get => Data.Browser; }
        private static ResourceBaseModel Account { get => Data.Resource; }
        private static CaptchaService CaptchaService { get => Account.CaptchaService; }
        private static SmsService SmsService { get => Account.SmsService; }

        #endregion =================================================================


        public static void AuthNew(out bool statusAuth)
        {
            _settingsMode = Browser.BrowserGetCurrentBusySettings();

            HE xFieldLogin = new HE("//input[@name='login']", "Логин");
            HE xFieldPass = new HE("//input[contains(@type, 'password')]", "Пароль");
            HE xButtonSubmit = new HE("//button[@type='submit']", "Подтвердить вход");
            HE xFormChangePass = new HE("//div[contains(@class, 'change-password-page') and contains(@class, 'passp-auth-screen')]", "Доступ к аккаунту ограничен");
            HE xButtonChangePassNext = new HE("//div[contains(@data-t, 'submit-change-pwd')]/descendant::button[contains(@data-t, 'action')]", "Подтвердить смену пароля");
            
            HE xFieldAnswer = new HE("//input[contains(@name, 'answer')]", "Ответа на контрольный вопрос");
            HE xButtonAnswer = new HE("//div[contains(@data-t, 'submit-check-answer')]/button", "Подтвердить ввод ответа на контрольный вопрос");


            var log = new LogSettings(false, true, true);
            var attemptsAuth = 0;

            while (true)
            {
                if (++attemptsAuth > 3)
                {
                    Logger.Write($"Слишком много ошибок в время авторизации", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    Logger.ErrorAnalysis(true, true, true, new List<string> { Browser.ActiveTab.URL });
                    statusAuth = false;
                    return;
                }

                Browser.ActiveTab.Navigate("https://passport.yandex.ru/auth?origin=home_yandexid&retpath=https%3A%2F%2Fyandex.ru&backpath=https%3A%2F%2Fyandex.ru", "https://yandex.ru/", true);

                if (!xFieldLogin.TryFindElement(3, log)) continue;
                else xFieldLogin.SetValue(Account.Login, LevelEmulation.SuperEmulation, Rnd.Next(250, 500));

                if (!xButtonSubmit.TryFindElement(3, log)) continue;
                else xButtonSubmit.Click(Rnd.Next(250, 500));

                if (!xFieldPass.TryFindElement(3, log)) continue;
                else xFieldPass.SetValue(Account.Password, LevelEmulation.SuperEmulation, Rnd.Next(250, 500));

                if (!xButtonSubmit.TryFindElement(3, log)) continue;
                else xButtonSubmit.Click(Rnd.Next(250, 500));

                // Ограничение доступа и смена пароля
                if (xFormChangePass.TryFindElement(3, null))
                {
                    Logger.Write("Восстановление доступа", LoggerType.Info, true, false, true);

                    if (!xButtonChangePassNext.TryFindElement(3, log)) continue;
                    else xButtonChangePassNext.Click(Rnd.Next(250, 500));

                    // Разгадывание капчи
                    Browser.UseTrafficMonitoring = true;
                    if (!TryRecognizeCaptcha()) continue;
                    Browser.UseTrafficMonitoring = false;

                    if (!xFieldAnswer.TryFindElement(3, log)) continue;
                    else xFieldAnswer.SetValue(Account.AnswerQuestion, LevelEmulation.SuperEmulation, Rnd.Next(250, 500));

                    if (!xButtonAnswer.TryFindElement(3, log)) continue;
                    else xButtonAnswer.Click(Rnd.Next(250, 500));

                    // Привязка телефона
                    if (!TryBindNumberPhone())
                    {
                        statusAuth = false;
                        return;
                    }

                }





            }
        }

        /// <summary>
        /// Разгадывание капчи.
        /// </summary>
        /// <returns></returns>
        private static bool TryRecognizeCaptcha()
        {
            HE xFieldCaptcha = new HE("//input[contains(@name, 'captcha_answer')]", "Поле капчи");
            HE xImgCaptcha = new HE("//div[@class='captcha__container']/descendant::img[@src!='']", "Изображение капчи");
            HE xButtonCaptchaNext = new HE("//div[contains(@data-t, 'submit-captcha')]/button", "Подтвердить ввод капчи");

            var log = new LogSettings(false, true, true);
            var attempts = 0;

            while (true)
            {
                if (++attempts > 3)
                {
                    Logger.Write("Слишком много ошибок во время разгадывания капчи", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    return false;
                }

                if (!xImgCaptcha.TryFindElement(3, log)) continue;
                if (!xFieldCaptcha.TryFindElement(3, log)) continue;
                if (!xButtonCaptchaNext.TryFindElement(3, log)) continue;

                // Разгадывание и ввод капчи
                if (CaptchaService.TryRecognize(xImgCaptcha.Element, out var result))
                {
                    xFieldCaptcha.SetValue(result, LevelEmulation.SuperEmulation, Rnd.Next(250, 500));
                    xButtonCaptchaNext.Click(Rnd.Next(250, 500));
                }
                else continue;

                // Проверка ответа разгадывания
                var btBody = Browser.ActiveTab.GetTraffic()
                    .Where(x => x.Url.Contains("registration-validations/checkHuman"))
                    .LastOrDefault()?.ResponseBody;
                
                if (btBody != null)
                {
                    var responseBody = Encoding.UTF8.GetString(btBody, 0, btBody.Length);
                    var body = Regex.Match(responseBody, "(?<=\"status\":\").*?(?=\")").Value;

                    if (body.Equals("ok", StringComparison.OrdinalIgnoreCase))
                    {
                        Logger.Write("Капча успешно разгадана", LoggerType.Info, true, false, true);
                        return true;
                    }
                    else if (body.Equals("error", StringComparison.OrdinalIgnoreCase))
                    {
                        Logger.Write("Капча разгадана неверно", LoggerType.Warning, true, false, true);
                        continue;
                    }
                    else
                    {
                        Logger.Write($"Неизвестная ошибка: '{nameof(responseBody)}:{responseBody}'", LoggerType.Warning, true, false, true);
                        return false;
                    }
                }
                else
                {
                    Logger.Write("В трафике не найден ответ от 'yandex' на разгадывание капчи", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    return false;
                }
            }
        }

        /*
         * todo доделать привязку номера
         */
        private static bool TryBindNumberPhone()
        {
            HE xFieldPhone = new HE("//input[contains(@name, 'phone')]", "Номер телефона");
            HE xButtonPhone = new HE("//div[contains(@data-t, 'submit-send-code')]/button", "Подтвердить ввод телефона");

            var log = new LogSettings(false, true, true);
            var attempts = 0;

            if (!xFieldPhone.TryFindElement(3, log)) return false;
            if (!xButtonPhone.TryFindElement(3, log)) return false;

            while (true)
            {
                if (++attempts > 3)
                {
                    Logger.Write("Слишком много ошибок во время привязки телефона", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    return false;
                }

                
            }
        }
    }
}