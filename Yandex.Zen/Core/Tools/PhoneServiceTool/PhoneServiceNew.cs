using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Yandex.Zen.Core.Tools.Extensions;
using Yandex.Zen.Core.Tools.LoggerTool;
using Yandex.Zen.Core.Tools.LoggerTool.Enums;
using Yandex.Zen.Core.Tools.PhoneServiceTool.Models;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.InterfacesLibrary.ProjectModel;
using ZennoLab.InterfacesLibrary.SmsService.Enums;

namespace Yandex.Zen.Core.Tools.PhoneServiceTool
{
    public class PhoneServiceNew
    {
        #region=========================================================
        private Instance Browser { get => ServicesComponents.Instance; }
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
        public string GetPhone(out string jobId, int timeToSecondsWaitPhone)
        {
            string phoneLog, phone;

            var stopwatch = new Stopwatch();

            stopwatch.Start();
            {
                while (true)
                {
                    jobId = ZennoPoster.Sms.GetNumber(Params.Dll, out phone, Params.NetworkService, "any", null, Params.Country);

                    if (phone == "No numbers" && timeToSecondsWaitPhone < stopwatch.ElapsedMilliseconds / 60)
                    {
                        Logger.Write($"[{nameof(timeToSecondsWaitPhone)}:{timeToSecondsWaitPhone}]\tНомеров нет", LoggerType.Warning, true, true, true, LogColor.Yellow);
                        return null;
                    }
                    else if (phone == "No numbers")
                    {
                        Thread.Sleep(500);
                        continue;
                    }
                    else
                    {
                        phoneLog = $"[{nameof(Params.Dll)}:{Params.Dll}]\t[{nameof(jobId)}:{jobId}]\t[{nameof(phone)}:{phone}]\t";
                        break;
                    }
                }
            }
            stopwatch.Stop();

            // Проверка получения номера и логирование
            if (jobId == "-1" || string.IsNullOrWhiteSpace(phone) is false && Regex.IsMatch(phone, @"[0-9]{5,}(?=$)") is false)
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
        /// <param name="jobId"></param>
        /// <param name="minutesWaitSmsCode"></param>
        /// <param name="heReSendSmsCode"></param>
        /// <param name="attemptsReSendCode"></param>
        /// <param name="phoneLog"></param>
        /// <returns></returns>
        public string GetSmsCode(string jobId, int minutesWaitSmsCode, HtmlElement heReSendSmsCode = null, int attemptsReSendCode = 0, string phoneLog = "")
        {
            string smsCodeOrStatus;
            var counterAttemptsReSendCode = default(int);

            while (true)
            {
                smsCodeOrStatus = ZennoPoster.Sms.GetStatus(Params.Dll, jobId, "", minutesWaitSmsCode);

                if (string.IsNullOrWhiteSpace(smsCodeOrStatus))
                {
                    Logger.Write($"{phoneLog}[{nameof(smsCodeOrStatus)}:{smsCodeOrStatus}]\tError reply to SMS code", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    return null;
                }
                else if (smsCodeOrStatus == "Error")
                {
                    Logger.Write($"{phoneLog}[{nameof(smsCodeOrStatus)}:{smsCodeOrStatus}]\t[{nameof(minutesWaitSmsCode)}:{minutesWaitSmsCode}]\tLimit min wait sms code", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    return null;
                }
                else if (smsCodeOrStatus == "Wait")
                {
                    if (++counterAttemptsReSendCode > attemptsReSendCode)
                    {
                        Logger.Write($"{phoneLog}[{nameof(smsCodeOrStatus)}:{smsCodeOrStatus}]\t[{attemptsReSendCode}]\tLimit attempts resend code", LoggerType.Warning, true, true, true, LogColor.Yellow);
                        return null;
                    }
                    else if (!heReSendSmsCode.IsNullOrVoid())
                    {
                        Logger.Write($"{phoneLog}[{nameof(smsCodeOrStatus)}:{smsCodeOrStatus}]\tNew attempt to resend code ", LoggerType.Info, true, false);
                        heReSendSmsCode.Click(Browser.ActiveTab, Rnd.Next(150, 500));
                        continue;
                    }
                }
                else break;
            }

            return smsCodeOrStatus;
        }

        /// <summary>
        /// Отмена номера.
        /// </summary>
        /// <param name="job_id"></param>
        public void CancelPhone(string job_id, string phoneLog = "")
        {
            try
            {
                if (job_id != "-1")
                {
                    var responseCancel = ZennoPoster.Sms.SetStatus(Params.Dll, job_id, SmsServiceStatus.Cancel);
                    Logger.Write($"{phoneLog}[{nameof(responseCancel)}:{responseCancel}]\tPhone number canceled", LoggerType.Info, true, false, true);
                }
            }
            catch (Exception ex)
            {
                Logger.Write($"{phoneLog}[{nameof(ex.Message)}:{ex.Message}]", LoggerType.Warning, true, true, true, LogColor.Yellow);
            }
        }
    }
}
