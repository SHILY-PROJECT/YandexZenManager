using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Yandex.Zen.Core.Toolkit.Extensions;
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
        private Instance Browser { get => ProjectComponents.Browser; }
        private Random Rnd { get; set; } = new Random();
        #endregion======================================================

        public PhoneSettingsModel Settings { get; private set; }
        public PhoneServiceParamsModel Params { get; private set; }
        public PhoneDataModel Data { get; private set; }

        public PhoneServiceNew(string serviceDllAndCountry, PhoneSettingsModel phoneSettings)
        {
            Data = new PhoneDataModel();
            Params = new PhoneServiceParamsModel(serviceDllAndCountry);
            Settings = phoneSettings;
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

            var secondsWaitPhone = Settings.TimeToSecondsWaitPhone;
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
            string smsCodeOrStatus;

            var counterAttemptsReSendCode = default(int);
            var minutesWaitSmsCode = Settings.MinutesWaitSmsCode;
            var attemptsReSendCode = Settings.AttemptsReSendSmsCode;

            while (true)
            {
                smsCodeOrStatus = ZennoPoster.Sms.GetStatus(Params.Dll, Data.JobID, "", minutesWaitSmsCode);

                if (string.IsNullOrWhiteSpace(smsCodeOrStatus))
                {
                    Logger.Write($"[{nameof(smsCodeOrStatus)}:{smsCodeOrStatus}]\tError reply to SMS code", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    return false;
                }
                else if (smsCodeOrStatus == "Error")
                {
                    Logger.Write($"[{nameof(smsCodeOrStatus)}:{smsCodeOrStatus}]\t[{nameof(minutesWaitSmsCode)}:{minutesWaitSmsCode}]\tLimit min wait sms code", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    return false;
                }
                else if (smsCodeOrStatus == "Wait")
                {
                    if (++counterAttemptsReSendCode > attemptsReSendCode)
                    {
                        Logger.Write($"[{nameof(smsCodeOrStatus)}:{smsCodeOrStatus}]\t[{attemptsReSendCode}]\tLimit attempts resend code", LoggerType.Warning, true, true, true, LogColor.Yellow);
                        return false;
                    }
                    else if (!heReSendSmsCode.IsNullOrVoid())
                    {
                        Logger.Write($"[{nameof(smsCodeOrStatus)}:{smsCodeOrStatus}]\tNew attempt to resend code ", LoggerType.Info, true, false);
                        heReSendSmsCode.Click(Browser.ActiveTab, Rnd.Next(150, 500));
                        continue;
                    }
                }
                else break;
            }

            Data.SmsCodeOrStatus = smsCodeOrStatus;

            return true;
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
