using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using Yandex.Zen.Core.Toolkit.BrowserCustomizer;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.InterfacesLibrary.SmsService.Enums;

namespace Yandex.Zen.Core.Toolkit
{
    [Obsolete]
    public class PhoneService : ServicesDataAndComponents
    {
        public string Dll { get; set; }
        public string YandexService { get; set; }
        public string CountryParam { get; set; }
        public bool StatusGetCountry { get; set; }

        public PhoneService(string serviceDllAndCountry)
        {
            var service = serviceDllAndCountry.Split(new[] { " - " }, StringSplitOptions.None)[0];
            var country = serviceDllAndCountry.Split(new[] { " - " }, StringSplitOptions.None)[1];

            switch (service)
            {
                case "FiveSimSms.dll":
                    {
                        Dll = service;
                        YandexService = "yandex";
                        StatusGetCountry = new Dictionary<string, string>
                        {
                            { "Россия", "&country=russia" },
                            { "USA", "&country=usa" },
                            { "Украина", "&country=ukraine" },
                            { "Канада", "&country=canada" },
                            { "Великобритания", "&country=england" }
                        }
                        .TryGetValue(country, out string countryParam);

                        CountryParam = countryParam;
                    }
                    break;
                case "SmsActivate.dll":
                    {
                        Dll = service;
                        YandexService = "ya";
                        StatusGetCountry = new Dictionary<string, string>
                        {
                            { "Россия", "&country=0" },
                            { "USA", "&country=187" },
                            { "USA (виртуальные)", "&country=12" }
                        }
                        .TryGetValue(country, out string countryParam);

                        CountryParam = countryParam;
                    }
                    break;
            }
        }

        /// <summary>
        /// Получение номера (Ошибки логируются автоматически).
        /// </summary>
        /// <param name="jobId">Id задания.</param>
        /// <param name="timeToSecondsWaitPhone">Время ожидания номера телефона (в секундах).</param>
        /// <param name="handlingNumberPhoneForRegion">Обработка номера под регионы - обрезание или дополнение символов (true - обрабатывать; иначе false).</param>
        /// <returns></returns>
        public static string GetPhone(out string jobId, int timeToSecondsWaitPhone)
        {
            string phoneLog, phone;

            var stopwatch = new Stopwatch();

            stopwatch.Start();

            while (true)
            {
                jobId = ZennoPoster.Sms.GetNumber(ProjectDataStore.PhoneService.Dll, out phone, ProjectDataStore.PhoneService.YandexService, "any", null, ProjectDataStore.PhoneService.CountryParam);

                if (phone == "No numbers" && timeToSecondsWaitPhone < stopwatch.ElapsedMilliseconds / 60)
                {
                    Logger.Write($"[Limit wait: {timeToSecondsWaitPhone} sec]\tНомеров нет", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    return null;
                }
                else if (phone == "No numbers")
                {
                    Thread.Sleep(500);
                    continue;
                }
                else
                {
                    phoneLog = $"[Sms service dll: {ProjectDataStore.PhoneService.Dll}]\t[Sms job id: {jobId}]\t[Phone: {phone}]\t";
                    break;
                }
            }

            stopwatch.Stop();

            // Проверка получения номера и логирование
            if (jobId == "-1" || !string.IsNullOrWhiteSpace(phone) && !Regex.IsMatch(phone, @"[0-9]{5,}(?=$)"))
            {
                Logger.Write($"{phoneLog}Во время получения номера что-то пошло не так...", LoggerType.Warning, true, true, true, LogColor.Yellow);
                return null;
            }
            else Logger.Write($"{phoneLog}[{stopwatch.ElapsedMilliseconds / 60} sec]\tНомер успешно получен: {phone}", LoggerType.Info, true, false, true, LogColor.Blue);

            return phone.Contains("+") ? phone : $"+{phone}";
        }

        /// <summary>
        /// Получение sms кода (ошибки логирует автоматически).
        /// </summary>
        /// <param name="job_id"></param>
        /// <param name="minutesWaitSmsCode"></param>
        /// <param name="heReSendSmsCode"></param>
        /// <param name="attemptsReSendCode"></param>
        /// <param name="phoneLog"></param>
        /// <returns></returns>
        public static string GetSmsCode(string job_id, int minutesWaitSmsCode, HtmlElement heReSendSmsCode = null, int attemptsReSendCode = 0, string phoneLog = "")
        {
            string sms_code;
            var counterAttemptsReSendCode = default(int);

            while (true)
            {
                sms_code = ZennoPoster.Sms.GetStatus(ProjectDataStore.PhoneService.Dll, job_id, "", minutesWaitSmsCode);

                if (string.IsNullOrWhiteSpace(sms_code))
                {
                    Logger.Write($"{phoneLog}[Reply to SMS code request: {sms_code}]\tЧто-то пошло не так...", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    return null;
                }
                else if (sms_code == "Error")
                {
                    Logger.Write($"{phoneLog}[Limit wait sms code: {minutesWaitSmsCode} min]\t[Status sms code: {sms_code}]\tSms код не пришел и что-то пошло не так...", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    return null;
                }
                else if (sms_code == "Wait")
                {
                    if (++counterAttemptsReSendCode > attemptsReSendCode)
                    {
                        Logger.Write($"{phoneLog}[Status sms code: {sms_code}]\t[Limit attempts resend code: {attemptsReSendCode}]\tSms код не пришел. Достигнут лимит попыток", LoggerType.Warning, true, true, true, LogColor.Yellow);
                        return null;
                    }
                    else if (!heReSendSmsCode.IsNullOrVoid())
                    {
                        Logger.Write($"{phoneLog}[Status sms code: {sms_code}]\tПовторная попытка отправки sms кода", LoggerType.Info, true, false);
                        heReSendSmsCode.Click(Instance.ActiveTab, Rnd.Next(150, 500));
                        continue;
                    }
                }
                else break;
            }

            return sms_code;
        }

        /// <summary>
        /// Отмена номера.
        /// </summary>
        /// <param name="job_id"></param>
        public static void CancelPhone(string job_id, string phoneLog = "")
        {
            try
            {
                if (job_id != "-1")
                {
                    var responseCancel = ZennoPoster.Sms.SetStatus(ProjectDataStore.PhoneService.Dll, job_id, SmsServiceStatus.Cancel);
                    Logger.Write($"{phoneLog}[Response cancel: {responseCancel}]\tНомер отменен", LoggerType.Info, true, false, true);
                }
            }
            catch (Exception exPhoneCancel)
            {
                Logger.Write($"{phoneLog}[Exception message: {exPhoneCancel.Message}]", LoggerType.Warning, true, true, true, LogColor.Yellow);
            }
        }
    }
}
