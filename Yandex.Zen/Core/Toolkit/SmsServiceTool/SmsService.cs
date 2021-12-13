using System;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.InterfacesLibrary.SmsService.Enums;
using Yandex.Zen.Core.Toolkit.BrowserCustomizer;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using Yandex.Zen.Core.Toolkit.SmsServiceTool.Models;

namespace Yandex.Zen.Core.Toolkit.SmsServiceTool
{
    public class SmsService
    {
        [ThreadStatic] private static bool _statusGetNumberPhone;
        [ThreadStatic] private static bool _statusGetSmsCode;

        public SmsServiceSettingsModel Settings { get; set; }
        public SmsServiceParamsDataModel Params { get; set; }
        public SmsServiceDataModel Data { get; set; } = new SmsServiceDataModel();

        public SmsService() { }

        public SmsService(SmsServiceSettingsModel smsServiceSettings, SmsServiceParamsDataModel smsServiceParams)
        {
            Params = smsServiceParams;
            Settings = smsServiceSettings;
        }

        /// <summary>
        /// Получение номера телефона (полученные номер телефона хранится в данных сервиса - 'Data').
        /// </summary>
        public bool TryGetPhoneNumber()
        {
            GetPhoneNumber();
            return _statusGetNumberPhone;
        }

        /// <summary>
        /// Получение номера телефона (полученные номер телефона хранится в данных сервиса - 'Data').
        /// </summary>
        public void GetPhoneNumber()
        {
            _statusGetNumberPhone = false;

            var stopwatch = new Stopwatch();
            var secondsWaitPhone = Settings.TimeToSecondsWaitPhone;
            var dll = Params.Dll;
            var networkService = Params.NetworkService;
            var country = Params.Country;
            stopwatch.Start();

            while (true)
            {
                var jobID = ZennoPoster.Sms.GetNumber
                (
                    serviceDll: dll,
                    service: networkService,
                    param: country,
                    oper: "any",
                    number: out string phone,
                    forward: null
                );

                if (!string.IsNullOrWhiteSpace(phone)&& Regex.IsMatch(phone, @"[0-5]{5,}"))
                {
                    Logger.Write($"[{nameof(dll)}:{dll}]\t[{nameof(jobID)}:{jobID}]\t[{nameof(phone)}:{phone}]\t" +
                        $"[{stopwatch.ElapsedMilliseconds / 1000} sec]\tPhone number received successfully", LoggerType.Info, true, false, true, LogColor.Blue);
                    
                    Data.JobID = jobID;
                    Data.NumberPhone = phone;
                    Data.NumberPhoneForServiceView = phone.Contains("+") ? phone : $"+{phone}";

                    _statusGetNumberPhone = true;
                    break;
                }
                else if (phone.Equals("No numbers", StringComparison.OrdinalIgnoreCase) && secondsWaitPhone < (stopwatch.ElapsedMilliseconds / 1000))
                {
                    Logger.Write($"[{nameof(secondsWaitPhone)}:{secondsWaitPhone}]\tWaiting limit getting a number phone reached", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    return;
                }
                else if (phone.Equals("No numbers", StringComparison.OrdinalIgnoreCase))
                {
                    Thread.Sleep(500);
                    continue;
                }
                else
                {
                    Logger.Write($"[{nameof(jobID)}:{jobID}]\tUnknown error while getting phone number", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    return;
                }
            }
        }

        /// <summary>
        /// Получение SMS-код (лог пишется автоматически).
        /// </summary>
        public bool TryGetSmsCode(bool cancelNumberPhoneIfСodeIsNotReceived)
        {
            GetSmsCode(cancelNumberPhoneIfСodeIsNotReceived);
            return _statusGetSmsCode;
        }

        /// <summary>
        /// Получение SMS-код (лог пишется автоматически).
        /// </summary>
        public void GetSmsCode(bool cancelNumberPhoneIfСodeIsNotReceived)
        {
            _statusGetSmsCode = default;

            var minWaitSmsCode = Settings.MinutesWaitSmsCode;
            var jobID = Data.JobID;
            var phone = Data.NumberPhone;
            var dll = Params.Dll;

            var smsCodeOrStatus = ZennoPoster.Sms.GetStatus(dll, jobID, "", minWaitSmsCode);
            var logCode = $"[{nameof(smsCodeOrStatus)}:{smsCodeOrStatus}]\t";

            if (Regex.IsMatch(smsCodeOrStatus, @"[0-9]{4,}"))
            {
                Logger.Write($"{logCode}[{nameof(phone)}:{phone}]\tSMS code received successfully", LoggerType.Info, true, false, true, LogColor.Default);

                _statusGetSmsCode = true;
                Data.SmsCodeOrStatus = smsCodeOrStatus;

                return;
            }
            else if (string.IsNullOrWhiteSpace(smsCodeOrStatus)) Logger.Write($"Response to the SMS code request is empty or null", LoggerType.Warning, true, true, true, LogColor.Yellow);
            else if ("Error".Equals(smsCodeOrStatus, StringComparison.OrdinalIgnoreCase)) Logger.Write($"{logCode}[{nameof(minWaitSmsCode)}:{minWaitSmsCode}]\tStatus SMS code is error", LoggerType.Warning, true, true, true, LogColor.Yellow);
            else if ("Wait".Equals(smsCodeOrStatus, StringComparison.OrdinalIgnoreCase)) Logger.Write($"{logCode}SMS code waiting limit reached", LoggerType.Warning, true, true, true, LogColor.Yellow);
            else Logger.Write($"{logCode}Unknown error while getting SMS code", LoggerType.Warning, true, true, true, LogColor.Yellow);

            if (cancelNumberPhoneIfСodeIsNotReceived) CancelPhoneNumber();
        }

        /// <summary>
        /// Отмена номера.
        /// </summary>
        /// <param name="jobID"></param>
        public void CancelPhoneNumber()
        {
            var jobID = Data.JobID;
            var dll = Params.Dll;

            try
            {
                if (!jobID.Equals("-1"))
                {
                    var responseCancel = ZennoPoster.Sms.SetStatus(dll, jobID, SmsServiceStatus.Cancel);
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
