﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Yandex.Zen.Core.Toolkit.BrowserCustomizer;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using Yandex.Zen.Core.Toolkit.PhoneServiceTool.Models;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.InterfacesLibrary.ProjectModel;
using ZennoLab.InterfacesLibrary.SmsService.Enums;

namespace Yandex.Zen.Core.Toolkit.PhoneServiceTool
{
    public class PhoneServiceNew
    {
        #region=========================================================
        private Instance Browser { get => ProjectComponents.Project.Browser; }
        private Random Rnd { get; set; } = new Random();
        #endregion======================================================

        public PhoneSettingsModel SettingsFromZennoVariables { get; private set; }
        public PhoneServiceParamsModel Params { get; private set; }
        public PhoneDataModel Data { get; private set; }

        public PhoneServiceNew(string serviceDllAndCountry, PhoneSettingsModel phoneSettings)
        {
            Data = new PhoneDataModel();
            Params = new PhoneServiceParamsModel(serviceDllAndCountry);
            SettingsFromZennoVariables = phoneSettings;
        }

        public PhoneServiceNew(ILocalVariable serviceDllAndCountry, PhoneSettingsModel phoneSettings) : this(serviceDllAndCountry.Value, phoneSettings) { }


        /// <summary>
        /// Получение номера (Ошибки логируются автоматически).
        /// </summary>
        /// <param name="jobId">Id задания.</param>
        /// <param name="timeToSecondsWaitPhone">Время ожидания номера телефона (в секундах).</param>
        /// <param name="handlingNumberPhoneForRegion">Обработка номера под регионы - обрезание или дополнение символов (true - обрабатывать; иначе false).</param>
        /// <returns></returns>
        public bool GetPhone()
        {
            string log, phone, jobId;

            var secondsWaitPhone = SettingsFromZennoVariables.TimeToSecondsWaitPhone;
            var stopwatch = new Stopwatch();

            stopwatch.Start();
            {
                while (true)
                {
                    jobId = ZennoPoster.Sms.GetNumber(Params.Dll, out phone, Params.NetworkService, "any", null, Params.Country);

                    if (phone == "No numbers" && secondsWaitPhone < stopwatch.ElapsedMilliseconds / 60)
                    {
                        Logger.Write($"[{nameof(secondsWaitPhone)}:{secondsWaitPhone}]\tНомеров нет", LoggerType.Warning, true, true, true, LogColor.Yellow);
                        return false;
                    }
                    else if (phone == "No numbers")
                    {
                        Thread.Sleep(500);
                        continue;
                    }
                    else
                    {
                        log = $"[{nameof(Params.Dll)}:{Params.Dll}]\t[{nameof(jobId)}:{jobId}]\t[{nameof(phone)}:{phone}]\t";
                        break;
                    }
                }
            }
            stopwatch.Stop();

            if (jobId == "-1" || !string.IsNullOrWhiteSpace(phone) && Regex.IsMatch(phone, @"[0-9]{5,}(?=$)") is false)
            {
                Logger.Write($"{log}Во время получения номера что-то пошло не так...", LoggerType.Warning, true, true, true, LogColor.Yellow);
                return false;
            }
            else Logger.Write($"{log}[{stopwatch.ElapsedMilliseconds / 60} sec]\tНомер успешно получен", LoggerType.Info, true, false, true, LogColor.Blue);

            Data.JobID = jobId;
            Data.NumberPhone = phone;
            Data.NumberPhoneForServiceView = phone.Contains("+") ? phone : $"+{phone}";

            return true;
        }

        /// <summary>
        /// Получение номера телефона.
        /// </summary>
        public bool GetNumberPhone()
        {
            var stopwatch = new Stopwatch();
            var secondsWaitPhone = SettingsFromZennoVariables.TimeToSecondsWaitPhone;
            stopwatch.Start();

            while (true)
            {
                var jobID = ZennoPoster.Sms.GetNumber
                (
                    serviceDll: Params.Dll,
                    service: Params.NetworkService,
                    param: Params.Country,
                    oper: "any",
                    number: out string phone,
                    forward: null
                );

                if (string.IsNullOrWhiteSpace(phone) is false && Regex.IsMatch(phone, @"[0-5]{5,}"))
                {
                    Logger.Write($"[{nameof(Params.Dll)}:{Params.Dll}]\t[{nameof(jobID)}:{jobID}]\t[{nameof(phone)}:{phone}]\t" +
                        $"[{stopwatch.ElapsedMilliseconds / 60} sec]\tНомер успешно получен", LoggerType.Info, true, false, true, LogColor.Blue);
                    Data.JobID = jobID;
                    Data.NumberPhone = phone;
                    Data.NumberPhoneForServiceView = phone.Contains("+") ? phone : $"+{phone}";
                    return true;
                }
                else if (phone.Equals("No numbers", StringComparison.OrdinalIgnoreCase) && secondsWaitPhone < (stopwatch.ElapsedMilliseconds / 60))
                {
                    Logger.Write($"[{nameof(secondsWaitPhone)}:{secondsWaitPhone}]\tНомеров нет", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    return false;
                }
                else if (phone.Equals("No numbers", StringComparison.OrdinalIgnoreCase))
                {
                    Thread.Sleep(500);
                    continue;
                }
                else
                {
                    Logger.Write($"[{nameof(jobID)}:{jobID}]\t[{new StackTrace().GetFrame(0).GetMethod().Name}]\tUnknown error", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    return false;
                }
            }
        }

        /// <summary>
        /// Получение sms кода (ошибки логирует автоматически).
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="minutesWaitSmsCode"></param>
        /// <param name="heReSendSmsCode"></param>
        /// <param name="attemptsReSendCode"></param>
        /// <param name="phoneLog"></param>
        /// <returns></returns>
        public bool GetSmsCode(HtmlElement heReSendSmsCode = null)
        {
            var counterAttemptsReSendCode = default(int);
            var attemptsReSendCode = SettingsFromZennoVariables.AttemptsReSendSmsCode;
            var minutesWaitSmsCode = SettingsFromZennoVariables.MinutesWaitSmsCode;
            var jobID = Data.JobID;
            var phone = Data.NumberPhone;

            while (true)
            {
                var smsCodeOrStatus = ZennoPoster.Sms.GetStatus(Params.Dll, jobID, "", minutesWaitSmsCode);
                var logCode = $"[{nameof(smsCodeOrStatus)}:{smsCodeOrStatus}]";

                if (Regex.IsMatch(smsCodeOrStatus, @"[0-9]{4,}"))
                {
                    Logger.Write($"[{nameof(Params.Dll)}:{Params.Dll}]\t[{nameof(jobID)}:{jobID}]\t[{nameof(phone)}:{phone}]\t{logCode}\tКод успешно получен", LoggerType.Info, true, false, true, LogColor.Default);
                    Data.SmsCodeOrStatus = smsCodeOrStatus;
                    return true;
                }
                else if (string.IsNullOrWhiteSpace(smsCodeOrStatus))
                {
                    Logger.Write($"{logCode}\tError reply to SMS code", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    return false;
                }
                else if (smsCodeOrStatus.Equals("Error", StringComparison.OrdinalIgnoreCase))
                {
                    Logger.Write($"{logCode}\t[{nameof(minutesWaitSmsCode)}:{minutesWaitSmsCode}]\tLimit min wait sms code", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    return false;
                }
                else if (smsCodeOrStatus.Equals("Wait", StringComparison.OrdinalIgnoreCase))
                {
                    if (++counterAttemptsReSendCode > attemptsReSendCode)
                    {
                        Logger.Write($"{logCode}\t[{attemptsReSendCode}]\tLimit attempts resend code", LoggerType.Warning, true, true, true, LogColor.Yellow);
                        return false;
                    }
                    else if (!heReSendSmsCode.IsNullOrVoid())
                    {
                        Logger.Write($"{logCode}\tNew attempt to resend code ", LoggerType.Info, true, false);
                        heReSendSmsCode.Click(Browser.ActiveTab, Rnd.Next(150, 500));
                        continue;
                    }
                }
                else
                {
                    Logger.Write($"[{nameof(jobID)}:{jobID}]\t{logCode}\t[{new StackTrace().GetFrame(0).GetMethod().Name}]\tUnknown error", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    return false;
                }
            }
        }

        /// <summary>
        /// Отмена номера.
        /// </summary>
        /// <param name="jobID"></param>
        public void CancelPhone()
        {
            try
            {
                if (Data.JobID != "-1")
                {
                    var responseCancel = ZennoPoster.Sms.SetStatus(Params.Dll, Data.JobID, SmsServiceStatus.Cancel);
                    Logger.Write($"[{nameof(responseCancel)}:{responseCancel}]\tPhone number canceled", LoggerType.Info, true, false, true);
                }
            }
            catch (Exception ex)
            {
                Logger.Write($"[{nameof(ex.Message)}:{ex.Message}]", LoggerType.Warning, true, true, true, LogColor.Yellow);
            }
        }
    }
}
