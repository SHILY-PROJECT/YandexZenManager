using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Tools;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.InterfacesLibrary.SmsService.Enums;
using ZennoLab.InterfacesLibrary.Enums.Http;
using Global.ZennoExtensions;
using System.Threading;
using System.Text.RegularExpressions;
using System.IO;
using Yandex.Zen.Core.Enums.Logger;
using Yandex.Zen.Core.Enums.Extensions;
using Yandex.Zen.Core.Tools.Extensions;
using Yandex.Zen.Core.Tools.Macros;
using Yandex.Zen.Core.Models.TableHandler;
using Yandex.Zen.Core.Enums;

namespace Yandex.Zen.Core.ServicesCommonComponents
{
    public class Authorization : ServiceComponents
    {
        [ThreadStatic]
        private static InstanceSettings.BusySettings BusyMode = InstanceSettings.BusySettings.GetCurrentBusySettings();

        /// <summary>
        /// Стартовая страница - авторизация.
        /// </summary>
        /// <returns></returns>
        public static bool Auth(bool bindingPhoneToAccountIfRequaid, string[] checkByXpathOfElement)
        {
            InstanceSettings.BusySettings.SetDefaultBusySettings();

            if (instance.ActiveTab.IsBusy)
                instance.ActiveTab.WaitDownloading();

            var xpathFieldLogin = new[] { "//input[@name='login']", "Поле - Логин" };
            var xpathFieldPassword = new[] { "//input[contains(@type, 'password')]", "Поле - Пароль" };
            var xpathButtonSubmit = new[] { "//button[@type='submit']", "Кнопка - Войти" };
            var xpathChangePassword = new[] { "//div[contains(@class, 'ChangePassword')]/descendant::div[contains(@class, 'info-wrap')]", "Доступ к аккаунту ограничен" };
            var xpathButtonChangePasswordNext = new[] { "//div[contains(@data-t, 'submit-gocaptcha')]/descendant::button[contains(@data-t, 'action')]", "Кнопка - Далее на форме смены пароля" };
            var xpathButtonSkipPhone = new[] { "//div[contains(@data-t, 'phone_skip')]/descendant::button", "Кнопка - Пропустить привязку номера" };
            var xpathAuthAccountList = new[] { "//div[contains(@class, 'passp-auth')]/descendant::div[contains(@class, 'passp-auth-content')]/descendant::div[contains(@class, 'AuthAccountList') and contains(@data-t, 'account') and not(contains(@data-t, 'item'))]", "Форма - Выберите учетную запись (список аккаунтов)" };
            var xpathItemAccount = new[] { ".//div[contains(@data-t, 'account-list-item')]/descendant::div[contains(@class, 'AuthAccountListItem-inner')]", "Кнопка - Аккаунт из списка" };

            var statusBindingPhoneToAccount = default(bool);
            var counterIterationsZenYandexMedia = 0;
            var sourceUrl = instance.ActiveTab.URL;

            while (true)
            {
                if (counterIterationsZenYandexMedia != 0)
                    instance.ActiveTab.Navigate(sourceUrl, true);

                if (++counterIterationsZenYandexMedia > 3)
                {
                    Logger.Write($"Достигнут лимит попыток авторизации", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    return false;
                }

                // Определение типа авторизации
                if (!instance.FuncGetFirstHe(xpathFieldLogin, false, false, 3).IsNullOrVoid())
                {
                    // Полная авторизация
                    try
                    {
                        instance.FuncGetFirstHe(xpathFieldLogin).SetValue(instance.ActiveTab, Login, LevelEmulation.SuperEmulation, rnd.Next(150, 500));
                        instance.FuncGetFirstHe(xpathButtonSubmit).Click(instance.ActiveTab, rnd.Next(150, 500));
                        instance.FuncGetFirstHe(xpathFieldPassword).SetValue(instance.ActiveTab, Password, LevelEmulation.SuperEmulation, rnd.Next(150, 500));
                        instance.FuncGetFirstHe(xpathButtonSubmit).Click(instance.ActiveTab, rnd.Next(150, 500));
                    }
                    catch { continue; }
                }
                else if (!instance.FuncGetFirstHe(xpathAuthAccountList).IsNullOrVoid())
                {
                    // Авторизация, когда нужно выбрать аккаунт из списка
                    var heAccountFormList = instance.FuncGetFirstHe(xpathAuthAccountList).FindChildByXPath(xpathItemAccount[0], 0);

                    if (heAccountFormList.IsNullOrVoid())
                    {
                        Logger.Write($"Не найдена форма с листом аккаунтов", LoggerType.Info, true, true, true, LogColor.Yellow);
                        Logger.ErrorAnalysis(true, true, true, new List<string>
                        {
                            instance.ActiveTab.URL,
                            $"Не найдена форма с листом аккаунтов...",
                            xpathAuthAccountList.XPathToStandardView(),
                            string.Empty
                        });
                        continue;
                    }
                    else heAccountFormList.Click(instance.ActiveTab, rnd.Next(1000, 1500));

                    // Ввод пароля и вход
                    instance.FuncGetFirstHe(xpathFieldPassword).SetValue(instance.ActiveTab, Password, LevelEmulation.SuperEmulation, rnd.Next(1000, 1500));
                    instance.FuncGetFirstHe(xpathButtonSubmit).Click(instance.ActiveTab, rnd.Next(1000, 1500));
                }
                else continue;

                // Проверка на наличие формы смены пароля
                if (!instance.FuncGetFirstHe(xpathChangePassword, false, false, 5).IsNullOrVoid())
                {
                    if (!bindingPhoneToAccountIfRequaid)
                    {
                        Logger.Write($"[Binding phone: {bindingPhoneToAccountIfRequaid}]\tДоступ к аккаунту ограничен, требуется привязка номера. Завершение работы потока", LoggerType.Info, true, false, true);
                        //Logger.LoggerWrite($"[Skip binding phone]", LoggerType.Info, true, false, false);
                        return false;
                    }

                    // Привязка номера к аккаунту
                    Logger.Write($"[Binding phone: {bindingPhoneToAccountIfRequaid}]\tДоступ к аккаунту ограничен, требуется привязка номера. Переход к привязке номера к аккаунту", LoggerType.Info, true, false, true);

                    // Получение для перехода к разгадыванию капчи
                    var heButtonChangePasswordNext = instance.FuncGetFirstHe(xpathButtonChangePasswordNext, false, true);

                    if (heButtonChangePasswordNext.IsNullOrVoid())
                    {
                        Logger.Write($"На форме \"Доступ к аккаунту ограничен\" не найдена кнопка \"Далее\"", LoggerType.Warning, true, true, true, LogColor.Yellow);
                        Logger.ErrorAnalysis(true, true, true, new List<string>
                        {
                            instance.ActiveTab.URL,
                            xpathButtonChangePasswordNext.XPathToStandardView(),
                            string.Empty
                        });
                        return false;
                    }
                    else heButtonChangePasswordNext.Click(instance.ActiveTab, rnd.Next(1500, 2000));

                    var xpathFieldCaptcha = new[] { "//input[contains(@name, 'captcha_answer')]", "Поле - Разгадка капчи" };
                    var xpathImgCaptcha = new[] { "//div[@class='captcha__container']/descendant::img[@src!='']", "Изображение капчи" };
                    var xpathButtonCaptchaNext = new[] { "//div[contains(@data-t, 'submit')]", "Кнопка - Далее" };
                    var xpathFieldAnswer = new[] { "//input[contains(@name, 'answer')]", "Поле - Ответа на контрольный вопрос" };
                    var xpathBottonSubmitAnswer = new[] { "//div[contains(@data-t, 'submit')]", "Кнопка - Далее" };
                    var xpathFieldPhone = new[] { "//input[contains(@name, 'phone')]", "Поле - Номер" };
                    var xpathButtonPhoneNext = new[] { "//div[contains(@data-t, 'submit')]", "Кнопка - Далее после ввода телефона" };
                    var xpathFieldSmsCode = new[] { "//input[contains(@name, 'phoneCode')]", "Поле - Ввод sms кода" };
                    var xpathButtonSmsCodeNext = new[] { "//div[contains(@class, 'PhoneConfirmationCode')]/descendant::button[contains(@data-t, 'action')]", "Кнопка - Далее после ввода sms кода" };
                    var xpathButtonReSendCode = new[] { "//div[contains(@class, 'PhoneConfirmationCode')]/descendant::button[contains(@data-t, 'retry-to-request-code')]", "Кнопка - Повторный запрос кода" };
                    var xpathRefreshedPassword = new[] { "//input[contains(@data-t, 'password') and not(contains(@data-t, 'confirm'))]", "Поле - Пароль" };
                    var xpathRefreshedPasswordConfirm = new[] { "//input[contains(@data-t, 'password_confirm')]", "Поле - Подтвердить пароль" };
                    var xpathRefreshedPasswordNext = new[] { "//div[contains(@data-t, 'commit-password')]/descendant::button", "Кнопка - Далее после ввода пароля" };
                    var xpathFormaChangePasswordIsGood = new[] { "//div[contains(@data-t, 'change-password')]", "Форма - Пароль был успешно изменён" };
                    var xpathButtonSubmitFinish = new[] { "//div[contains(@data-t, 'submit-finish')]/descendant::a[contains(@data-t, 'action')]", "Кнопка - Далее на форме успешного изменения пароля" };

                    var refreshedPassword = TextMacros.GenerateString(15, "abcd");
                    var refreshPage = default(bool);
                    var counterAttemptBindingPhone = default(int);

                    instance.UseTrafficMonitoring = true;

                    // Обработка капчи
                    while (true)
                    {
                        // Счетчик попыток
                        if (++counterAttemptBindingPhone > 3)
                        {
                            Logger.Write($"Не удалось привязать номер к аккаунту. Достигнут лимит попыток", LoggerType.Warning, true, true, true, LogColor.Yellow);
                            return false;
                        }

                        // Разгадывание капчи
                        var heFieldCaptcha = instance.FuncGetFirstHe(xpathFieldCaptcha, false, true, 5);
                        var heImgCaptcha = instance.FuncGetFirstHe(xpathImgCaptcha, false, true, 5);

                        // Проверка наличия капчи и поля для её ввода
                        if (heFieldCaptcha.IsNullOrVoid() || heImgCaptcha.IsNullOrVoid() || string.IsNullOrWhiteSpace(heImgCaptcha.GetAttribute("src")))
                        {
                            Logger.Write($"Не найдено поле капчи, либо сама капча. Рефреш страницы", LoggerType.Warning, true, true, true, LogColor.Yellow);

                            var heElements = new List<string>();

                            if (heFieldCaptcha.IsNullOrVoid()) heElements.Add(xpathFieldCaptcha.XPathToStandardView());
                            if (heImgCaptcha.IsNullOrVoid()) heElements.Add(xpathImgCaptcha.XPathToStandardView());

                            Logger.ErrorAnalysis(true, true, true, new List<string>
                            {
                                instance.ActiveTab.URL,
                                string.Join(Environment.NewLine, heElements),
                                string.Empty
                            });

                            refreshPage = true;
                            break;
                        }

                        // Отправка капчи на распознание
                        var captchaResult = CaptchaService.Recognize(heImgCaptcha);

                        // Проверка результата распознания
                        if (string.IsNullOrWhiteSpace(captchaResult))
                        {
                            heImgCaptcha.Click(instance.ActiveTab, rnd.Next(1000, 1500));
                            continue;
                        }

                        // Ввод капчи
                        heFieldCaptcha.SetValue(instance.ActiveTab, captchaResult, LevelEmulation.SuperEmulation, rnd.Next(500, 1000));
                        instance.FuncGetFirstHe(xpathButtonCaptchaNext, false, true).Click(instance.ActiveTab, rnd.Next(1000, 2000));

                        // Проверка наличия поля контрольного вопроса
                        if (instance.FuncGetFirstHe(xpathFieldAnswer, false, false, 5).IsNullOrVoid())
                        {
                            var statusCaptcha = default(string);
                            var traffic = instance.ActiveTab.GetTraffic().Where(x => x.Url.Contains("registration-validations/checkHuman")).ToList();

                            if (traffic.Count != 0)
                            {
                                var responseBody = traffic.Last().ResponseBody;
                                statusCaptcha = Regex.Match(Encoding.UTF8.GetString(responseBody, 0, responseBody.Length), "(?<=\"status\":\").*?(?=\")").Value;
                                heImgCaptcha = instance.FuncGetFirstHe(xpathFieldCaptcha, false, false);

                                if (statusCaptcha == "error")
                                {
                                    Logger.Write($"[Status: {statusCaptcha}]\tКапча введена неверно: {captchaResult}", LoggerType.Info, true, true, true, LogColor.Yellow);
                                    heImgCaptcha.Click(instance.ActiveTab, rnd.Next(1000, 1500));
                                }
                                else if (statusCaptcha == "ok")
                                {
                                    Logger.Write($"[Status: {statusCaptcha}]\tКапча успешно разгадана, но поле контрольного вопроса не найдено", LoggerType.Info, true, true, true, LogColor.Yellow);

                                    // Проверка наличия капчи
                                    if (!heImgCaptcha.IsNullOrVoid())
                                    {
                                        heImgCaptcha.Click(instance.ActiveTab, rnd.Next(1000, 1500));
                                        continue;
                                    }

                                    refreshPage = true;
                                    break;
                                }
                                else Logger.Write($"[Response body: {responseBody}\t|\tStatus: {statusCaptcha}]\tСтатус капчи не определен", LoggerType.Info, true, true, true, LogColor.Yellow);
                            }
                        }
                        else
                        {
                            Logger.Write($"Капча успешно введена", LoggerType.Info, true, false, true);
                            break;
                        }
                    }

                    instance.UseTrafficMonitoring = false;
                    instance.ActiveTab.NavigateTimeout = 30;

                    // Рефреш странички
                    if (refreshPage) continue;

                    // Ввод ответа на контрольный вопрос и переход к полю ввода номера
                    instance.FuncGetFirstHe(xpathFieldAnswer, false).SetValue(instance.ActiveTab, Answer, LevelEmulation.SuperEmulation, rnd.Next(500, 1000));
                    instance.FuncGetFirstHe(xpathBottonSubmitAnswer, false).Click(instance.ActiveTab, rnd.Next(2000, 3000));

                    // Обработка номера
                    var heFieldPhone = instance.FuncGetFirstHe(xpathFieldPhone, false, true, 10);
                    var heButtonPhoneNext = instance.FuncGetFirstHe(xpathButtonPhoneNext, false, true);

                    // Проверка наличия поля для ввода телефона (рефреш, если не нашли)
                    if (new[] { heFieldPhone, heButtonPhoneNext }.Any(x => x.IsNullOrVoid()))
                    {
                        Logger.Write($"Не найдено поле для ввода телефона, либо кнопка далее. Рефреш страницы", LoggerType.Info, true, true, true, LogColor.Yellow);

                        var elements = new List<string>();

                        if (heFieldPhone.IsNullOrVoid()) elements.Add(xpathFieldPhone.XPathToStandardView());
                        if (heButtonPhoneNext.IsNullOrVoid()) elements.Add(xpathButtonPhoneNext.XPathToStandardView());

                        Logger.ErrorAnalysis(true, true, true, new List<string>
                        {
                            instance.ActiveTab.URL,
                            string.Join(Environment.NewLine, elements),
                            string.Empty
                        });

                        continue;
                    }

                    // Получение номера
                    Phone = PhoneService.GetPhone(out string job_id, TimeToSecondsWaitPhone);

                    // Выход из метода, если не удалось получить номер
                    if (string.IsNullOrWhiteSpace(Phone)) return false;

                    var phoneLog = $"[Sms service dll: {Program.PhoneService.Dll}]\t[Sms job id: {job_id}]\t[Phone: {Phone}]\t";

                    // Ввод номера
                    heFieldPhone.SetValue(instance.ActiveTab, Phone, LevelEmulation.SuperEmulation, rnd.Next(500, 1000));
                    heButtonPhoneNext.Click(instance.ActiveTab, rnd.Next(150, 500));

                    // Получение элементов для ввода и отправки sms кода
                    var heFieldSmsCode = instance.FuncGetFirstHe(xpathFieldSmsCode, false, true, 5);
                    var heButtonSmsCodeNext = instance.FuncGetFirstHe(xpathButtonSmsCodeNext, false, true);
                    var heButtonReSendCode = instance.FuncGetFirstHe(xpathButtonReSendCode, false, true);

                    // Проверка наличия полученных элементов для обработки номера (если нет, то отмена номера и выход из метода)
                    if (new[] { heFieldSmsCode, heButtonSmsCodeNext, heButtonReSendCode }.Any(x => x.IsNullOrVoid()))
                    {
                        PhoneService.CancelPhone(job_id, phoneLog);

                        var heElements = new List<string>();

                        if (heFieldSmsCode.IsNullOrVoid()) heElements.Add(xpathFieldSmsCode.XPathToStandardView());
                        if (heButtonSmsCodeNext.IsNullOrVoid()) heElements.Add(xpathButtonSmsCodeNext.XPathToStandardView());
                        if (heButtonReSendCode.IsNullOrVoid()) heElements.Add(xpathButtonReSendCode.XPathToStandardView());

                        Logger.ErrorAnalysis(true, true, true, new List<string>
                        {
                            instance.ActiveTab.URL,
                            string.Join(Environment.NewLine, heElements),
                            string.Empty
                        });

                        return false;
                    }

                    // Получение sms кода
                    var sms_code = PhoneService.GetSmsCode(job_id, MinutesWaitSmsCode, heButtonReSendCode, AttemptsReSendSmsCode, phoneLog);

                    // Проверка наличия sms кода (если кода нет нет, то отмена номера и выход из метода)
                    if (string.IsNullOrWhiteSpace(sms_code))
                    {
                        PhoneService.CancelPhone(job_id, phoneLog);
                        return false;
                    }

                    Logger.Write($"{phoneLog}Код успешно получен: {sms_code}", LoggerType.Info, true, false, true, LogColor.Blue);

                    // Ввод номера и переход к следующему шагу
                    heFieldSmsCode.SetValue(instance.ActiveTab, sms_code, LevelEmulation.SuperEmulation, rnd.Next(3000, 5000));
                    //heButtonSmsCodeNext.Click(instance.ActiveTab, rnd.Next(1000, 2000));

                    // Смена пароля
                    var heRefreshedPassword = instance.FuncGetFirstHe(xpathRefreshedPassword, false, true, 10);
                    var heRefreshedPasswordConfirm = instance.FuncGetFirstHe(xpathRefreshedPasswordConfirm, false, true);
                    var heRefreshedPasswordNext = instance.FuncGetFirstHe(xpathRefreshedPasswordNext, false, true);

                    // Проверка наличия элементов
                    if (new[] { heRefreshedPassword, heRefreshedPasswordConfirm, heRefreshedPasswordNext }.Any(x => x.IsNullOrVoid()))
                    {
                        Logger.Write($"Не элементы для установки нового пароля (деньги за текущий номер, скорее всего, проёбаны)", LoggerType.Warning, true, true, true, LogColor.Yellow);

                        var heElements = new List<string>();

                        if (heRefreshedPassword.IsNullOrVoid()) heElements.Add(xpathRefreshedPassword.XPathToStandardView());
                        if (heRefreshedPasswordConfirm.IsNullOrVoid()) heElements.Add(xpathRefreshedPasswordConfirm.XPathToStandardView());
                        if (heRefreshedPasswordNext.IsNullOrVoid()) heElements.Add(xpathRefreshedPasswordNext.XPathToStandardView());

                        Logger.ErrorAnalysis(true, true, true, new List<string>
                        {
                            instance.ActiveTab.URL,
                            string.Join(Environment.NewLine, heElements),
                            string.Empty
                        });

                        return false;
                        /*
                            todo - Добавить запрос на принятие нового sms кода
                        */
                    }

                    // Заполнение нового пароля и переход дальше
                    heRefreshedPassword.SetValue(instance.ActiveTab, refreshedPassword, LevelEmulation.SuperEmulation, rnd.Next(1000, 1500));
                    heRefreshedPasswordConfirm.SetValue(instance.ActiveTab, refreshedPassword, LevelEmulation.SuperEmulation, rnd.Next(1000, 1500));
                    heRefreshedPasswordNext.Click(instance.ActiveTab, rnd.Next(2000, 3000));

                    // Бэкап данных
                    Logger.MakeBackupData(new List<string>
                    {
                        $"Refreshed password: {refreshedPassword}"
                    },
                    true);

                    // Получение формы для проверки
                    var heFormaChangePasswordIsGood = instance.FuncGetFirstHe(xpathFormaChangePasswordIsGood, false, true, 10);
                    var heButtonSubmitFinish = instance.FuncGetFirstHe(xpathButtonSubmitFinish, false, true);

                    // Проверка формы
                    if (heFormaChangePasswordIsGood.IsNullOrVoid() && heButtonSubmitFinish.IsNullOrVoid())
                    {
                        Logger.Write($"Не найдена форма с уведомлением о успешном изменении пароля (деньги за текущий номер, скорее всего, проёбаны)", LoggerType.Warning, true, true, true, LogColor.Yellow);
                        Logger.ErrorAnalysis(true, true, true, new List<string>
                        {
                            instance.ActiveTab.URL,
                            xpathFormaChangePasswordIsGood.XPathToStandardView(),
                            xpathButtonSubmitFinish.XPathToStandardView(),
                            string.Empty
                        });

                        return false;
                        /*
                            todo - Добавить действия, если нет формы с успешным изменениям пароля
                        */
                    }
                    else
                    {
                        statusBindingPhoneToAccount = true;
                        Logger.Write($"[Старый пароль: {Password}]\t[Новый пароль: {refreshedPassword}]\tПароль был успешно изменен", LoggerType.Info, true, false, true, LogColor.Blue);
                    }

                    // Бэкап данных
                    Logger.MakeBackupData(new List<string>
                    {
                        $"Account phone: {Phone}"
                    },
                    true);

                    // Сохранение результата в таблицу режима и общую таблицу
                    TableHandler.WriteToCellInSharedAndMode(TableColumnEnum.Inst.Login, Login, new List<InstDataItem>
                    {
                        new InstDataItem(TableColumnEnum.Inst.Password, refreshedPassword),
                        new InstDataItem(TableColumnEnum.Inst.PhoneNumber, Phone)
                    });

                    // Сохранение профиля
                    ProfileWorker.SaveProfile(true);

                    // Завершение авторизации
                    if (heButtonSubmitFinish.IsNullOrVoid())
                    {
                        Logger.Write($"На форме с уведомлением о успешном изменении пароля не найдена кнопка \"Далее\"", LoggerType.Warning, true, true, true, LogColor.Yellow);
                        Logger.ErrorAnalysis(true, true, true, new List<string>
                        {
                            instance.ActiveTab.URL,
                            "На форме с уведомлением о успешном изменении пароля не найдена кнопка \"Далее\"",
                            xpathButtonSubmitFinish.XPathToStandardView(),
                            string.Empty
                        });
                        return false;
                    }
                    else heButtonSubmitFinish.Click(instance.ActiveTab, rnd.Next(1500, 2000));

                    return CheckAuthorization(checkByXpathOfElement, statusBindingPhoneToAccount);
                }
                else
                {
                    var heButtonSkipPhone = instance.FuncGetFirstHe(xpathButtonSkipPhone, false, false, 10);
                    var heCheckElement = instance.FuncGetFirstHe(checkByXpathOfElement, false, false, 10);

                    if (heButtonSkipPhone.IsNullOrVoid() && heCheckElement.IsNullOrVoid())
                    {
                        Logger.Write($"Что-то пошло не так во время авторизации: {instance.ActiveTab.URL}", LoggerType.Warning, true, true, true, LogColor.Yellow);

                        var heElements = new List<string>();

                        if (heButtonSkipPhone.IsNullOrVoid()) heElements.Add(xpathButtonSkipPhone.XPathToStandardView());
                        if (heCheckElement.IsNullOrVoid()) heElements.Add(checkByXpathOfElement.XPathToStandardView());

                        Logger.ErrorAnalysis(true, true, true, new List<string>
                        {
                            instance.ActiveTab.URL,
                            string.Join(Environment.NewLine, heElements),
                            string.Empty
                        });

                        ProfileWorker.SaveProfile(true);

                        return false;
                    }

                    instance.ActiveTab.NavigateTimeout = 30;

                    if (!heButtonSkipPhone.IsNullOrVoid())
                        heButtonSkipPhone.Click(instance.ActiveTab, rnd.Next(1000, 2000));

                    return CheckAuthorization(checkByXpathOfElement, statusBindingPhoneToAccount);
                }
            }
        }

        /// <summary>
        /// Проверка авторизации.
        /// </summary>
        /// <param name="checkByXpathOfElement"></param>
        /// <param name="statusBindingPhoneToAccount"></param>
        /// <returns></returns>
        private static bool CheckAuthorization(string[] checkByXpathOfElement, bool statusBindingPhoneToAccount)
        {
            var endLog = statusBindingPhoneToAccount ? " (с привязкой номера к аккаунту)" : "";
            var counterAttempts = default(int);

            while (true)
            {
                // Счетчик количества попыток
                if (++counterAttempts > 5)
                {
                    Logger.Write($"Что-то пошло не так во время авторизации: {instance.ActiveTab.URL}", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    Logger.ErrorAnalysis(true, true, true, new List<string>
                    {
                        instance.ActiveTab.URL,
                        $"Что-то пошло не так во время авторизации...",
                        checkByXpathOfElement.XPathToStandardView(),
                        string.Empty
                    });
                    return false;
                }

                // Успешный выход из метода, если нужный элемент обнаружен
                if (!instance.FuncGetFirstHe(checkByXpathOfElement, false, false, 5).IsNullOrVoid())
                {
                    Logger.Write($"Успешная авторизация аккаунта{endLog}", LoggerType.Info, true, false, true, LogColor.Blue);

                    InstanceSettings.BusySettings.SetBusySettings(BusyMode);
                    ProfileWorker.SaveProfile(true);

                    return true;
                }
            }
        }

    }
}