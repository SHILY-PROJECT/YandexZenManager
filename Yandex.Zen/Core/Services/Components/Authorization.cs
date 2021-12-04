using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yandex.Zen.Core.Toolkit;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.Enums.Log;
using System.Text.RegularExpressions;
using Yandex.Zen.Core.Enums.Extensions;
using Yandex.Zen.Core.Toolkit.Extensions;
using Yandex.Zen.Core.Toolkit.Macros;
using Yandex.Zen.Core.Models.TableHandler;
using Yandex.Zen.Core.Enums;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using Yandex.Zen.Core.Toolkit.BrowserCustomizer;

namespace Yandex.Zen.Core.Services.Components
{
    public class Authorization : ServicesComponents
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

            if (Instance.ActiveTab.IsBusy)
                Instance.ActiveTab.WaitDownloading();

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
            var sourceUrl = Instance.ActiveTab.URL;

            while (true)
            {
                if (counterIterationsZenYandexMedia != 0)
                    Instance.ActiveTab.Navigate(sourceUrl, true);

                if (++counterIterationsZenYandexMedia > 3)
                {
                    Logger.Write($"Достигнут лимит попыток авторизации", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    return false;
                }

                // Определение типа авторизации
                if (!Instance.FuncGetFirstHe(xpathFieldLogin, false, false, 3).IsNullOrVoid())
                {
                    // Полная авторизация
                    try
                    {
                        Instance.FuncGetFirstHe(xpathFieldLogin).SetValue(Instance.ActiveTab, Login, LevelEmulation.SuperEmulation, Rnd.Next(150, 500));
                        Instance.FuncGetFirstHe(xpathButtonSubmit).Click(Instance.ActiveTab, Rnd.Next(150, 500));
                        Instance.FuncGetFirstHe(xpathFieldPassword).SetValue(Instance.ActiveTab, Password, LevelEmulation.SuperEmulation, Rnd.Next(150, 500));
                        Instance.FuncGetFirstHe(xpathButtonSubmit).Click(Instance.ActiveTab, Rnd.Next(150, 500));
                    }
                    catch { continue; }
                }
                else if (!Instance.FuncGetFirstHe(xpathAuthAccountList).IsNullOrVoid())
                {
                    // Авторизация, когда нужно выбрать аккаунт из списка
                    var heAccountFormList = Instance.FuncGetFirstHe(xpathAuthAccountList).FindChildByXPath(xpathItemAccount[0], 0);

                    if (heAccountFormList.IsNullOrVoid())
                    {
                        Logger.Write($"Не найдена форма с листом аккаунтов", LoggerType.Info, true, true, true, LogColor.Yellow);
                        Logger.ErrorAnalysis(true, true, true, new List<string>
                        {
                            Instance.ActiveTab.URL,
                            $"Не найдена форма с листом аккаунтов...",
                            xpathAuthAccountList.XPathToStandardView(),
                            string.Empty
                        });
                        continue;
                    }
                    else heAccountFormList.Click(Instance.ActiveTab, Rnd.Next(1000, 1500));

                    // Ввод пароля и вход
                    Instance.FuncGetFirstHe(xpathFieldPassword).SetValue(Instance.ActiveTab, Password, LevelEmulation.SuperEmulation, Rnd.Next(1000, 1500));
                    Instance.FuncGetFirstHe(xpathButtonSubmit).Click(Instance.ActiveTab, Rnd.Next(1000, 1500));
                }
                else continue;

                // Проверка на наличие формы смены пароля
                if (!Instance.FuncGetFirstHe(xpathChangePassword, false, false, 5).IsNullOrVoid())
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
                    var heButtonChangePasswordNext = Instance.FuncGetFirstHe(xpathButtonChangePasswordNext, false, true);

                    if (heButtonChangePasswordNext.IsNullOrVoid())
                    {
                        Logger.Write($"На форме \"Доступ к аккаунту ограничен\" не найдена кнопка \"Далее\"", LoggerType.Warning, true, true, true, LogColor.Yellow);
                        Logger.ErrorAnalysis(true, true, true, new List<string>
                        {
                            Instance.ActiveTab.URL,
                            xpathButtonChangePasswordNext.XPathToStandardView(),
                            string.Empty
                        });
                        return false;
                    }
                    else heButtonChangePasswordNext.Click(Instance.ActiveTab, Rnd.Next(1500, 2000));

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

                    Instance.UseTrafficMonitoring = true;

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
                        var heFieldCaptcha = Instance.FuncGetFirstHe(xpathFieldCaptcha, false, true, 5);
                        var heImgCaptcha = Instance.FuncGetFirstHe(xpathImgCaptcha, false, true, 5);

                        // Проверка наличия капчи и поля для её ввода
                        if (heFieldCaptcha.IsNullOrVoid() || heImgCaptcha.IsNullOrVoid() || string.IsNullOrWhiteSpace(heImgCaptcha.GetAttribute("src")))
                        {
                            Logger.Write($"Не найдено поле капчи, либо сама капча. Рефреш страницы", LoggerType.Warning, true, true, true, LogColor.Yellow);

                            var heElements = new List<string>();

                            if (heFieldCaptcha.IsNullOrVoid()) heElements.Add(xpathFieldCaptcha.XPathToStandardView());
                            if (heImgCaptcha.IsNullOrVoid()) heElements.Add(xpathImgCaptcha.XPathToStandardView());

                            Logger.ErrorAnalysis(true, true, true, new List<string>
                            {
                                Instance.ActiveTab.URL,
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
                            heImgCaptcha.Click(Instance.ActiveTab, Rnd.Next(1000, 1500));
                            continue;
                        }

                        // Ввод капчи
                        heFieldCaptcha.SetValue(Instance.ActiveTab, captchaResult, LevelEmulation.SuperEmulation, Rnd.Next(500, 1000));
                        Instance.FuncGetFirstHe(xpathButtonCaptchaNext, false, true).Click(Instance.ActiveTab, Rnd.Next(1000, 2000));

                        // Проверка наличия поля контрольного вопроса
                        if (Instance.FuncGetFirstHe(xpathFieldAnswer, false, false, 5).IsNullOrVoid())
                        {
                            var statusCaptcha = default(string);
                            var traffic = Instance.ActiveTab.GetTraffic().Where(x => x.Url.Contains("registration-validations/checkHuman")).ToList();

                            if (traffic.Count != 0)
                            {
                                var responseBody = traffic.Last().ResponseBody;
                                statusCaptcha = Regex.Match(Encoding.UTF8.GetString(responseBody, 0, responseBody.Length), "(?<=\"status\":\").*?(?=\")").Value;
                                heImgCaptcha = Instance.FuncGetFirstHe(xpathFieldCaptcha, false, false);

                                if (statusCaptcha == "error")
                                {
                                    Logger.Write($"[Status: {statusCaptcha}]\tКапча введена неверно: {captchaResult}", LoggerType.Info, true, true, true, LogColor.Yellow);
                                    heImgCaptcha.Click(Instance.ActiveTab, Rnd.Next(1000, 1500));
                                }
                                else if (statusCaptcha == "ok")
                                {
                                    Logger.Write($"[Status: {statusCaptcha}]\tКапча успешно разгадана, но поле контрольного вопроса не найдено", LoggerType.Info, true, true, true, LogColor.Yellow);

                                    // Проверка наличия капчи
                                    if (!heImgCaptcha.IsNullOrVoid())
                                    {
                                        heImgCaptcha.Click(Instance.ActiveTab, Rnd.Next(1000, 1500));
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

                    Instance.UseTrafficMonitoring = false;
                    Instance.ActiveTab.NavigateTimeout = 30;

                    // Рефреш странички
                    if (refreshPage) continue;

                    // Ввод ответа на контрольный вопрос и переход к полю ввода номера
                    Instance.FuncGetFirstHe(xpathFieldAnswer, false).SetValue(Instance.ActiveTab, Answer, LevelEmulation.SuperEmulation, Rnd.Next(500, 1000));
                    Instance.FuncGetFirstHe(xpathBottonSubmitAnswer, false).Click(Instance.ActiveTab, Rnd.Next(2000, 3000));

                    // Обработка номера
                    var heFieldPhone = Instance.FuncGetFirstHe(xpathFieldPhone, false, true, 10);
                    var heButtonPhoneNext = Instance.FuncGetFirstHe(xpathButtonPhoneNext, false, true);

                    // Проверка наличия поля для ввода телефона (рефреш, если не нашли)
                    if (new[] { heFieldPhone, heButtonPhoneNext }.Any(x => x.IsNullOrVoid()))
                    {
                        Logger.Write($"Не найдено поле для ввода телефона, либо кнопка далее. Рефреш страницы", LoggerType.Info, true, true, true, LogColor.Yellow);

                        var elements = new List<string>();

                        if (heFieldPhone.IsNullOrVoid()) elements.Add(xpathFieldPhone.XPathToStandardView());
                        if (heButtonPhoneNext.IsNullOrVoid()) elements.Add(xpathButtonPhoneNext.XPathToStandardView());

                        Logger.ErrorAnalysis(true, true, true, new List<string>
                        {
                            Instance.ActiveTab.URL,
                            string.Join(Environment.NewLine, elements),
                            string.Empty
                        });

                        continue;
                    }

                    // Получение номера
                    Phone = PhoneService.GetPhone(out string job_id, TimeToSecondsWaitPhone);

                    // Выход из метода, если не удалось получить номер
                    if (string.IsNullOrWhiteSpace(Phone)) return false;

                    var phoneLog = $"[Sms service dll: {ProjectDataStore.PhoneService.Dll}]\t[Sms job id: {job_id}]\t[Phone: {Phone}]\t";

                    // Ввод номера
                    heFieldPhone.SetValue(Instance.ActiveTab, Phone, LevelEmulation.SuperEmulation, Rnd.Next(500, 1000));
                    heButtonPhoneNext.Click(Instance.ActiveTab, Rnd.Next(150, 500));

                    // Получение элементов для ввода и отправки sms кода
                    var heFieldSmsCode = Instance.FuncGetFirstHe(xpathFieldSmsCode, false, true, 5);
                    var heButtonSmsCodeNext = Instance.FuncGetFirstHe(xpathButtonSmsCodeNext, false, true);
                    var heButtonReSendCode = Instance.FuncGetFirstHe(xpathButtonReSendCode, false, true);

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
                            Instance.ActiveTab.URL,
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
                    heFieldSmsCode.SetValue(Instance.ActiveTab, sms_code, LevelEmulation.SuperEmulation, Rnd.Next(3000, 5000));
                    //heButtonSmsCodeNext.Click(instance.ActiveTab, rnd.Next(1000, 2000));

                    // Смена пароля
                    var heRefreshedPassword = Instance.FuncGetFirstHe(xpathRefreshedPassword, false, true, 10);
                    var heRefreshedPasswordConfirm = Instance.FuncGetFirstHe(xpathRefreshedPasswordConfirm, false, true);
                    var heRefreshedPasswordNext = Instance.FuncGetFirstHe(xpathRefreshedPasswordNext, false, true);

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
                            Instance.ActiveTab.URL,
                            string.Join(Environment.NewLine, heElements),
                            string.Empty
                        });

                        return false;
                        /*
                            todo - Добавить запрос на принятие нового sms кода
                        */
                    }

                    // Заполнение нового пароля и переход дальше
                    heRefreshedPassword.SetValue(Instance.ActiveTab, refreshedPassword, LevelEmulation.SuperEmulation, Rnd.Next(1000, 1500));
                    heRefreshedPasswordConfirm.SetValue(Instance.ActiveTab, refreshedPassword, LevelEmulation.SuperEmulation, Rnd.Next(1000, 1500));
                    heRefreshedPasswordNext.Click(Instance.ActiveTab, Rnd.Next(2000, 3000));

                    // Бэкап данных
                    Logger.MakeBackupData(new List<string>
                    {
                        $"Refreshed password: {refreshedPassword}"
                    },
                    true);

                    // Получение формы для проверки
                    var heFormaChangePasswordIsGood = Instance.FuncGetFirstHe(xpathFormaChangePasswordIsGood, false, true, 10);
                    var heButtonSubmitFinish = Instance.FuncGetFirstHe(xpathButtonSubmitFinish, false, true);

                    // Проверка формы
                    if (heFormaChangePasswordIsGood.IsNullOrVoid() && heButtonSubmitFinish.IsNullOrVoid())
                    {
                        Logger.Write($"Не найдена форма с уведомлением о успешном изменении пароля (деньги за текущий номер, скорее всего, проёбаны)", LoggerType.Warning, true, true, true, LogColor.Yellow);
                        Logger.ErrorAnalysis(true, true, true, new List<string>
                        {
                            Instance.ActiveTab.URL,
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
                            Instance.ActiveTab.URL,
                            "На форме с уведомлением о успешном изменении пароля не найдена кнопка \"Далее\"",
                            xpathButtonSubmitFinish.XPathToStandardView(),
                            string.Empty
                        });
                        return false;
                    }
                    else heButtonSubmitFinish.Click(Instance.ActiveTab, Rnd.Next(1500, 2000));

                    return CheckAuthorization(checkByXpathOfElement, statusBindingPhoneToAccount);
                }
                else
                {
                    var heButtonSkipPhone = Instance.FuncGetFirstHe(xpathButtonSkipPhone, false, false, 10);
                    var heCheckElement = Instance.FuncGetFirstHe(checkByXpathOfElement, false, false, 10);

                    if (heButtonSkipPhone.IsNullOrVoid() && heCheckElement.IsNullOrVoid())
                    {
                        Logger.Write($"Что-то пошло не так во время авторизации: {Instance.ActiveTab.URL}", LoggerType.Warning, true, true, true, LogColor.Yellow);

                        var heElements = new List<string>();

                        if (heButtonSkipPhone.IsNullOrVoid()) heElements.Add(xpathButtonSkipPhone.XPathToStandardView());
                        if (heCheckElement.IsNullOrVoid()) heElements.Add(checkByXpathOfElement.XPathToStandardView());

                        Logger.ErrorAnalysis(true, true, true, new List<string>
                        {
                            Instance.ActiveTab.URL,
                            string.Join(Environment.NewLine, heElements),
                            string.Empty
                        });

                        ProfileWorker.SaveProfile(true);

                        return false;
                    }

                    Instance.ActiveTab.NavigateTimeout = 30;

                    if (!heButtonSkipPhone.IsNullOrVoid())
                        heButtonSkipPhone.Click(Instance.ActiveTab, Rnd.Next(1000, 2000));

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
                    Logger.Write($"Что-то пошло не так во время авторизации: {Instance.ActiveTab.URL}", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    Logger.ErrorAnalysis(true, true, true, new List<string>
                    {
                        Instance.ActiveTab.URL,
                        $"Что-то пошло не так во время авторизации...",
                        checkByXpathOfElement.XPathToStandardView(),
                        string.Empty
                    });
                    return false;
                }

                // Успешный выход из метода, если нужный элемент обнаружен
                if (!Instance.FuncGetFirstHe(checkByXpathOfElement, false, false, 5).IsNullOrVoid())
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