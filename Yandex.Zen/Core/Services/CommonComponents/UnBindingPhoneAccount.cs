using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Yandex.Zen.Core.Toolkit;
using Yandex.Zen.Core.Toolkit.BrowserCustomizer;
using Yandex.Zen.Core.Toolkit.BrowserCustomizer.Enums;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.Enums.Http;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.InterfacesLibrary.SmsService.Enums;

namespace Yandex.Zen.Core.Services.CommonComponents
{
    public class UnBindingPhoneAccount : Obsolete_ServicesDataAndComponents
    {
        /// <summary>
        /// Отвязать номер от аккаунта яндекс.
        /// </summary>
        /// <returns></returns>
        public static bool UnBinding()
        {
            HtmlElement heShowPhoneIsHidden;
            string job_id = string.Empty, sms_code = string.Empty, statusRetryGet = string.Empty;

            var refferer = Instance.ActiveTab.URL;

            var xpathShowPhone = "//div[contains(@class, 'p-mails') and contains(@class, 'p-mails')]/descendant::div[contains(@class, 'Section-arrow')]";
            var xpathShowPhoneIsHidden = "//div[contains(@class, 'p-mails') and contains(@class, 'p-mails') and contains(@class, 'isHidden')]/descendant::div[contains(@class, 'Section-arrow')]";
            var xpathEditPhone = "//div[contains(@class, 'p-mails') and contains(@class, 'p-mails')]/descendant::a[contains(@data-t, 'phones-edit-sub-link') and @href!='']";
            var xpathButtonRemove = "//div[contains(@class, 'yasms-secure-phone')]/descendant::button[contains(@data-operation, 'delete')]";
            var xpathButtonRemoveConfirm = "//div[contains(@class, 'yasms-phone-clearfix')]/descendant::button[contains(@class, 'yasms-remove')]";

            var xpathFieldSmsCode = new[] { "//form[@method='post']/descendant::input[contains(@name, 'code')]", "Поле - Sms код" };
            var xpathFieldPassword = new[] { "//form[@method='post']/descendant::input[contains(@name, 'password')]", "Поле - Пароль" };
            var xpathButtonConfirm = new[] { "//form[@method='post']/descendant::button[contains(@class, 'confirm')]", "Кнопка - Подтвердить удаление номера" };
            var xpathButtonReSendSmsCode = new[] { "//form[@method='post']/descendant::span[contains(@class, 'resend-code') and contains(@class, 'yasms-control-link')]", "Кнопка - Повторная отправка кода" };
            var xpathButtonCloseGoodWindow = new[] { "//div[contains(@class, 'yasms-popup-remove')]/descendant::a[contains(@class, 'popup-close')]", "Кнопка - Закрыть гуд окно" };

            var counterAttempts = 0;

            while (true)
            {
                if (++counterAttempts > 3)
                {
                    Logger.Write($"Не удалось отвязать номер. Достигнут лимит попыток", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    return false;
                }

                try
                {
                    Instance.ActiveTab.Navigate("https://passport.yandex.ru/profile", refferer, true);

                    // Проверяем наличие нужного элемента и раскрытие блока с редактированием номера, а так же, переход к самому редактированию
                    Instance.FindFirstElement(xpathShowPhone, "Кнопка - Раскрыть информацию о номере", true, true, 7);

                    heShowPhoneIsHidden = Instance.FindFirstElement(xpathShowPhoneIsHidden, "Кнопка - Раскрыть информацию о номере", false, false);

                    if (!heShowPhoneIsHidden.IsNullOrVoid()) heShowPhoneIsHidden.Click(Instance.ActiveTab, Rnd.Next(500, 1000));

                    Instance.FindFirstElement(xpathEditPhone, "Кнопка - Изменить список").Click(Instance.ActiveTab, Rnd.Next(500, 1000));

                    // Удаление номера
                    Instance.FindFirstElement(xpathButtonRemove, "Кнопка - Удалить номер", true, true, 7).Click(Instance.ActiveTab, Rnd.Next(150, 500));

                    // Получение ID задания
                    var fileInfoLogAccount = Logger.GetLogAccountFileInfo(ObjectDirectory.FullName);

                    if (!fileInfoLogAccount.Exists)
                    {
                        Logger.Write($"Невозможно отвязать номер от аккаунта. Лога аккаунта не существует", LoggerType.Warning, true, true, true, LogColor.Yellow);
                        return false;
                    }
                    else
                    {
                        var logList = File.ReadAllLines(fileInfoLogAccount.FullName, Encoding.UTF8).Where(x => Regex.IsMatch(x, @"(?<=\[AccountLinking:).*?(?=])")).ToList();

                        if (logList.Count == 0)
                        {
                            Logger.Write($"Невозможно отвязать номер от аккаунта. Лога аккаунта пуст", LoggerType.Warning, true, true, true, LogColor.Yellow);
                            return false;
                        }
                        else job_id = Regex.Match(logList.Last(), @"(?<=\[JobId:).*?(?=])").Value;

                        try
                        {
                            // Запрос на повторное принятие sms кода
                            statusRetryGet = ZennoPoster.Sms.SetStatus(StateKeeper.PhoneService.Dll, job_id, SmsServiceStatus.RetryGet, null, StateKeeper.PhoneService.CountryParam);

                            // Подтверждение удаления номера
                            Instance.FindFirstElement(xpathButtonRemoveConfirm, "Кнопка - Да, точно удалить", true, true, 7).Click(Instance.ActiveTab, Rnd.Next(150, 500));

                            // Получение кода
                            sms_code = Obsolete_PhoneService.GetSmsCode(job_id, 1, Instance.FindFirstElement(xpathButtonReSendSmsCode, true, true), 3);
                        }
                        catch (Exception ex)
                        {
                            Logger.Write($"[Exception message: {ex.Message}]\tУпало исключение. Не удалось отвязать номер", LoggerType.Warning, true, true, true, LogColor.Yellow);
                            return false;
                        }
                    }

                    Instance.FindFirstElement(xpathFieldSmsCode).SetValue(Instance.ActiveTab, sms_code, LevelEmulation.SuperEmulation, Rnd.Next(150, 500));
                    Instance.FindFirstElement(xpathFieldPassword).SetValue(Instance.ActiveTab, Password, LevelEmulation.SuperEmulation, Rnd.Next(150, 500));
                    Instance.FindFirstElement(xpathButtonConfirm).Click(Instance.ActiveTab, Rnd.Next(2000, 3000));

                    var heButtonCloseGoodWindow = Instance.FindFirstElement(xpathButtonCloseGoodWindow, false, false, 5);

                    if (!heButtonCloseGoodWindow.IsNullOrVoid() || !CheckPhoneInPassportYandex())
                    {
                        if (!heButtonCloseGoodWindow.IsNullOrVoid())
                            heButtonCloseGoodWindow.Click(Instance.ActiveTab, Rnd.Next(150, 500));

                        Logger.Write($"[AccountLinking:Untied]\tНомер успешно отвязан", LoggerType.Warning, true, true, true, LogColor.Green);
                    }
                    else
                    {
                        Logger.Write($"[statusRetryGet:{statusRetryGet}]\t[smsCode:{sms_code}]\tНе удалось отвязать номер", LoggerType.Warning, true, true, true, LogColor.Yellow);
                        return false;
                    }

                    return true;
                }
                catch
                {
                    if (!CheckPhoneInPassportYandex())
                    {
                        Logger.Write($"[AccountLinking:Untied]\tНомер успешно отвязан", LoggerType.Warning, true, true, true, LogColor.Green);
                        return true;
                    }

                    continue;
                }
            }
        }

        /// <summary>
        /// Проверка аккаунта на привязаный номер.
        /// </summary>
        /// <returns>true - номер привязан; иначе - false.</returns>
        public static bool CheckPhoneInPassportYandex()
        {
            var httpResponse = ZennoPoster.HTTP.Request
            (
                HttpMethod.GET, "https://passport.yandex.ru/profile", proxy: Instance.GetProxy(), Encoding: "utf-8", respType: ResponceType.BodyOnly,
                Timeout: 30000, UserAgent: Zenno.Profile.UserAgent, cookieContainer: Zenno.Profile.CookieContainer
            );

            return !string.IsNullOrWhiteSpace(Regex.Match(httpResponse, "(?<=\"number\":\").*?(?=\")").Value);
        }
    }
}
