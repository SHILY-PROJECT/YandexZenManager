using System;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.Enums.Log;
using Yandex.Zen.Core.Toolkit;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using Yandex.Zen.Core.Toolkit.BrowserCustomizer;
using Yandex.Zen.Core.Toolkit.BrowserCustomizer.Models;
using Yandex.Zen.Core.Toolkit.BrowserCustomizer.Enums;
using Yandex.Zen.Core.Toolkit.SmsServiceTool;
using ZennoLab.InterfacesLibrary.Enums.Http;
using ZennoLab.InterfacesLibrary.ProjectModel;
using Yandex.Zen.Core.ServiceModules.ObjectModule;
using Yandex.Zen.Core.Interfaces;

namespace Yandex.Zen.Core.ServiceСomponents
{
    public class AuthorizationModule : IAuthorizationModule
    {
        private bool _isSuccesss;

        public bool IsSuccesss { get => _isSuccesss; }

        private BrowserBusySettingsModel SettingsMode { get; set; }
        private DataManager_new DataManager { get; set; }
        private IZennoPosterProjectModel Zenno { get => DataManager.Zenno; }
        private Instance Browser { get => DataManager.Browser; }
        private ObjectBase Object { get => DataManager.Object; }
        private CaptchaService CaptchaService { get => Object.CaptchaService; }
        private SmsService SmsService { get => Object.SmsService; }
        private Random Rnd { get; } = new Random();
        private bool EndExecution { get; set; }

        public AuthorizationModule(DataManager_new manager)
        {
            DataManager = manager;
        }

        /// <summary>
        /// Авторизация.
        /// </summary>
        public void Authorization() => Authorization(out _);

        /// <summary>
        /// Авторизация.
        /// </summary>
        /// <param name="isSuccessful"></param>
        public void Authorization(out bool isSuccessful)
        {
            SettingsMode = Browser.BrowserGetCurrentBusySettings();

            var log = new LogSettings(false, true, true);
            var firstStart = true;
            var counterAttempts = 0;

            #region ====[XPATH]=============================================================
            HE xAvatar = new HE("//div[contains(@class, 'desk-notif-card')]/descendant::a[contains(@class, 'avatar')]", "Аватар пользователя");
            HE xFieldLogin = new HE("//input[@name='login']", "Логин");
            HE xFieldPass = new HE("//input[contains(@type, 'password')]", "Пароль");
            HE xButtonSubmit = new HE("//button[@type='submit']", "Подтвердить вход");
            HE xFormChangePass = new HE("//div[contains(@class, 'change-password-page') and contains(@class, 'passp-auth-screen')]", "Доступ к аккаунту ограничен");
            HE xButtonChangePassNext = new HE("//div[contains(@data-t, 'submit-change-pwd')]/descendant::button[contains(@data-t, 'action')]", "Подтвердить смену пароля");
            HE xFieldAnswer = new HE("//input[contains(@name, 'answer')]", "Ответа на контрольный вопрос");
            HE xButtonAnswer = new HE("//div[contains(@data-t, 'submit-check-answer')]/button", "Подтвердить ввод ответа на контрольный вопрос");
            #endregion =====================================================================

            while (true)
            {
                if (firstStart)
                {
                    Browser.ActiveTab.Navigate("https://yandex.ru/", true);

                    if (xAvatar.TryFindElement(3))
                    {
                        Logger.Write("Аккаунт уже авторизирован", LoggerType.Info, true, false, false);
                        isSuccessful = _isSuccesss = true;
                        break;
                    }
                    else firstStart = false;
                }

                if (++counterAttempts > 3)
                {
                    Logger.Write("Слишком много ошибок в время авторизации", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    Logger.ErrorAnalysis(true, true, true, new List<string> { Browser.ActiveTab.URL });
                    isSuccessful = _isSuccesss = false;
                    return;
                }

                Browser.ActiveTab.Navigate("https://passport.yandex.ru/auth?origin=home_yandexid&retpath=https%3A%2F%2Fyandex.ru&backpath=https%3A%2F%2Fyandex.ru", "https://yandex.ru/", true);

                if (!xFieldLogin.TryFindElement(3, log)) continue;
                else xFieldLogin.SetValue(Object.Login, LevelEmulation.SuperEmulation, Rnd.Next(250, 500));

                if (!xButtonSubmit.TryFindElement(3, log)) continue;
                else xButtonSubmit.Click(Rnd.Next(250, 500));

                if (!xFieldPass.TryFindElement(3, log)) continue;
                else xFieldPass.SetValue(Object.Password, LevelEmulation.SuperEmulation, Rnd.Next(250, 500));

                if (!xButtonSubmit.TryFindElement(3, log)) continue;
                else xButtonSubmit.Click(Rnd.Next(250, 500));

                #region ====[ОГРАНИЧЕНИЕ ДОСТУПА И СМЕНА ПАРОЛЯ]===============================
                if (xFormChangePass.TryFindElement(3, null))
                {
                    Logger.Write("Восстановление доступа", LoggerType.Info, true, false, true);

                    if (!xButtonChangePassNext.TryFindElement(3, log)) continue;
                    else xButtonChangePassNext.Click(Rnd.Next(250, 500));

                    #region ====[РАСПОЗНАВАНИЕ КАПЧИ]==================
                    Browser.UseTrafficMonitoring = true;
                    if (!TryRecognizeCaptcha()) continue;
                    Browser.UseTrafficMonitoring = false;

                    if (!xFieldAnswer.TryFindElement(3, log)) continue;
                    else xFieldAnswer.SetValue(Object.AnswerQuestion, LevelEmulation.SuperEmulation, Rnd.Next(250, 500));

                    if (!xButtonAnswer.TryFindElement(3, log)) continue;
                    else xButtonAnswer.Click(Rnd.Next(250, 500));
                    #endregion ========================================

                    #region ====[ПРИВЯЗКА НОМЕРА К АККАУНТУ]===========
                    if (!TryBindNumberPhone())
                    {
                        if (EndExecution is false) continue;
                        isSuccessful = _isSuccesss = false;
                        return;
                    }
                    else
                    {
                        isSuccessful = _isSuccesss = true;
                        break;
                    }
                    #endregion ========================================
                }
                else
                {
                    /*
                     * TODO: Доработать гуд авторизацию, если нет формы восстановления доступа
                     */
                }
                #endregion ==================================================================
            }

            /* TODO: Добавить условие, если пароль был только что привязан
                     Если есть номер в таблице, то не проверять его в настройках
             */
            var phoneBilded = default(bool);

            if (string.IsNullOrWhiteSpace(Object.PhoneNumber) && !(phoneBilded = CheckPhoneNumberBinding()))
            {
                Logger.Write("К аккаунту не привязан номер", LoggerType.Info, true, false, true, LogColor.Yellow);
                isSuccessful = _isSuccesss = false;

                /* 
                 * TODO: -Временно помечать аккаунты, что они авторизированы, но требуется привязка номера.
                 *      - вносить в таблицу: 'AuthYesPhoneNo'
                 * 
                 *       -Идти привязывать номер
                 * 
                 *       !(возможно нужно проверять номер в дзен)
                 * 
                 *       Task level: very-low
                 */
            }
            else if (string.IsNullOrWhiteSpace(Object.PhoneNumber) && phoneBilded)
            {
                Logger.Write("К аккаунту привязан номер, но сам номер отсутствует в таблице", LoggerType.Info, true, false, true);
                isSuccessful = _isSuccesss = false;
                /* 
                 * TODO: Нужно реализовать поиск по файлу лога аккаунта и внести этот номер в таблицу
                 *       Пока мы не реализовали эту логику - вносить в таблицу: 'Search'
                 */
            }
            else
            {
                Logger.Write("К аккаунту привязан номер", LoggerType.Info, true, false, true);
            }

            Browser.BrowserSetBusySettings(SettingsMode);
        }

        /// <summary>
        /// Разгадывание капчи.
        /// </summary>
        /// <returns></returns>
        private bool TryRecognizeCaptcha()
        {
            HE xFieldCaptcha = new HE("//input[contains(@name, 'captcha_answer')]", "Поле капчи");
            HE xImgCaptcha = new HE("//div[@class='captcha__container']/descendant::img[@src!='']", "Изображение капчи");
            HE xButtonCaptchaNext = new HE("//div[contains(@data-t, 'submit-captcha')]/button", "Подтвердить ввод капчи");

            var log = new LogSettings(false, true, true);
            var attempts = 0;
            _ = Browser.ActiveTab.GetTraffic();

            while (true)
            {
                if (++attempts > 3)
                {
                    Logger.Write("Слишком много ошибок во время разгадывания капчи", LoggerType.Warning, true, false, true, LogColor.Yellow);
                    return false;
                }

                if (!xImgCaptcha.TryFindElement(3, log)) continue;
                if (!xFieldCaptcha.TryFindElement(3, log)) continue;
                if (!xButtonCaptchaNext.TryFindElement(3, log)) continue;

                // Разгадывание и ввод капчи
                if (CaptchaService.TryRecognize(xImgCaptcha.Element, out var result))
                {
                    Logger.Write(CaptchaService.LogMessage, LoggerType.Info, true, false, true);
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
                    Logger.Write("В трафике не найден ответ от 'yandex' на разгадывание капчи", LoggerType.Warning, true, false, true, LogColor.Yellow);
                    return false;
                }
            }
        }

        /// <summary>
        /// Привязка номера.
        /// </summary>
        /// <returns></returns>
        private bool TryBindNumberPhone()
        {
            EndExecution = false;
            var log = new LogSettings(false, true, true);

            #region ====[XPATH]=============================================================
            HE xFieldPhone = new HE("//input[contains(@name, 'phone')]", "Номер телефона");
            HE xButtonPhoneNext = new HE("//div[contains(@data-t, 'submit-send-code')]/button", "Подтвердить ввод телефона");

            HE xFieldSmsCode = new HE("//input[contains(@data-t, 'phoneCode')]", "SMS Код");
            HE xButtonSmsCodeNext = new HE("//div[contains(@class, 'PhoneConfirmationCode')]/button[contains(@data-t, 'action')]", "Подтвердить ввод кода");
            HE xButtonSmsCodeReSend = new HE("//button[contains(@data-t, 'retry-to-request-code')]", "Отправить ещё sms код");

            HE xFieldNewPass = new HE("//input[contains(@data-t, 'input-password') and not(contains(@data-t, 'confirm'))]", "Новый пароль");
            HE xFieldNewPassConfirm = new HE("//input[contains(@data-t, 'input-password_confirm')]", "Новый пароль");
            HE xButtonNewPassNext = new HE("//div[contains(@data-t, 'commit-password')]/descendant::button[contains(@data-t, 'action')]", "Пдтвердить новый пароль");
            HE xButtonFinish = new HE("//div[contains(@data-t, 'submit-finish')]/descendant::*[contains(@data-t, 'action')]", "Финиш");
            HE xButtonConfirmAccountDetails = new HE("//div[contains(@data-t, 'check-data-submit')]/descendant::*[contains(@data-t, 'action')]", "Подтвердить данные аккаунта");
            #endregion ======================================================================

            if (!xFieldPhone.TryFindElement(3, log) || !xButtonPhoneNext.TryFindElement(3, log)) return false;

            #region ====[ПОЛУЧЕНИЕ И ВВОД НОМЕРА + ОТПРАВКА КОДА]============
            if (!SmsService.TryGetPhoneNumber())
            {
                Logger.Write(SmsService.LogMessage, LoggerType.Warning, true, false, true, LogColor.Yellow);
                EndExecution = true;
                return false;
            }
            else Logger.Write(SmsService.LogMessage, LoggerType.Info, true, false, true, LogColor.Blue);

            // 
            xFieldPhone.SetValue(SmsService.Data.NumberPhone, LevelEmulation.SuperEmulation, Rnd.Next(250, 500));
            xButtonPhoneNext.Click(Rnd.Next(250, 500));

            if (!xFieldSmsCode.TryFindElement(3, log) ||
                !xButtonSmsCodeNext.TryFindElement(3, log) ||
                !xButtonSmsCodeReSend.TryFindElement(3, log))
            {
                SmsService.CancelPhoneNumber();
                Logger.Write(SmsService.LogMessage, LoggerType.Warning, true, false, true, LogColor.Yellow);
                EndExecution = true;
                return false;
            }
            #endregion =======================================================

            #region ====[ПОЛУЧЕНИЕ И ВВОД КОДА]===============================
            if (!SmsService.TryGetSmsCode(false))
            {
                Logger.Write(SmsService.LogMessage, LoggerType.Warning, true, false, true, LogColor.Yellow);
                SmsService.CancelPhoneNumber();
                Logger.Write(SmsService.LogMessage, LoggerType.Warning, true, false, true, LogColor.Yellow);
                EndExecution = true;
                return false;
            }
            xFieldSmsCode.SetValue(SmsService.Data.SmsCodeOrStatus, LevelEmulation.SuperEmulation, Rnd.Next(1500, 3000));
            #endregion =======================================================

            #region ====[ГЕНЕРАЦИЯ И УСТАНОВКА НОВОГО ПАРОЛЯ]=================
            if (!xFieldNewPass.TryFindElement(3, log) ||
                !xFieldNewPassConfirm.TryFindElement(3, log) ||
                !xButtonNewPassNext.TryFindElement(3, log))
            {
                EndExecution = true;
                return false;
            }

            Object.GenerateNewPassword();
            xFieldNewPass.SetValue(Object.Password, LevelEmulation.SuperEmulation, Rnd.Next(250, 500));
            xFieldNewPassConfirm.SetValue(Object.Password, LevelEmulation.SuperEmulation, Rnd.Next(250, 500));
            xButtonNewPassNext.Click(Rnd.Next(1500, 3000));

            if (!xButtonFinish.TryFindElement(3, log))
            {
                Object.SaveProfile();
                EndExecution = true;
                Logger.Write("Не удалось определить успешность завершения смены пароля", LoggerType.Info, true, false, true, LogColor.Yellow);
                return false;
            }
            else
            {
                xButtonFinish.Click(Rnd.Next(4000, 5000));
                if (xButtonConfirmAccountDetails.TryFindElement(3, log))
                    xButtonConfirmAccountDetails.Click(Rnd.Next(1500, 3000));
                Object.SaveProfile();
                /*
                 * TODO: Сохранить номер аккаунта в таблицу и новый пароль
                 */
                Logger.Write("Пароль успешно изменен/номер успешно привязан к аккаунту", LoggerType.Info, true, false, true, LogColor.Green);
            }
            return true;
            #endregion ========================================================
        }

        /// <summary>
        /// Проверка привязки номера к аккаунту.
        /// </summary>
        /// <returns>true - номер привязан; иначе - false.</returns>
        public bool CheckPhoneNumberBinding()
        {
            var httpResponse = ZennoPoster.HTTP.Request
            (
                HttpMethod.GET, "https://passport.yandex.ru/profile",
                UserAgent: Zenno.Profile.UserAgent,
                proxy: Browser.GetProxy(),
                respType: ResponceType.BodyOnly,
                cookieContainer: Zenno.Profile.CookieContainer
            );
            return !string.IsNullOrWhiteSpace(Regex.Match(httpResponse, "(?<=\"number\":\").*?(?=\")").Value);
        }
    }
}