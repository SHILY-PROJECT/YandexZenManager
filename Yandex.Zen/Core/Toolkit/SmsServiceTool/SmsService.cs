using System;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.SmsService.Enums;
using Yandex.Zen.Core.Toolkit.SmsServiceTool.Models;
using Yandex.Zen.Core.Toolkit.LoggerTool;

namespace Yandex.Zen.Core.Toolkit.SmsServiceTool
{
    public class SmsService
    {
        private string _logMessage = string.Empty;
        private bool _statusGetNumberPhone;
        private bool _statusGetSmsCode;
        private int _counterAttemptsGetSmsCode;

        /// <summary>
        /// Настройки для работы (время запроса номера, попытки запроса sms кода и т.д.).
        /// </summary>
        public SmsServiceSettingsModel Settings { get; set; }

        /// <summary>
        /// Параметры для получения номера (dll, страна и т.д.).
        /// </summary>
        public SmsServiceParamsDataModel Params { get; set; }

        /// <summary>
        /// Данные полученные в ходе работы (id задания, номер и т.д.).
        /// </summary>
        public SmsServiceDataModel Data { get; set; } = new SmsServiceDataModel();

        /// <summary>
        /// Информация для лога (может быть как гуд ответ, так и бэд)
        /// </summary>
        public string LogMessage { get => _logMessage; }


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
            _logMessage = string.Empty;
            _statusGetNumberPhone = false;

            var stopwatch = new Stopwatch();
            var secondsWaitPhone = Settings.TimeToSecondsWaitPhone;
            var dll = Params.Dll;
            var networkService = Params.NetworkService;
            var country = Params.Country;
            stopwatch.Start();

            try
            {
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

                    if (!string.IsNullOrWhiteSpace(phone) && Regex.IsMatch(phone, @"[0-9]{5,}"))
                    {
                        Data.JobID = jobID;
                        Data.NumberPhone = phone;
                        Data.NumberPhoneForServiceView = phone.Contains("+") ? phone : $"+{phone}";

                        _logMessage = $"[{nameof(dll)}:{dll}]\t[{nameof(jobID)}:{jobID}]\t[{nameof(phone)}:{phone}]\t[{stopwatch.ElapsedMilliseconds / 1000} sec]\tPhone number received successfully";
                        _statusGetNumberPhone = true;
                        return;
                    }
                    else if (phone.Equals("No numbers", StringComparison.OrdinalIgnoreCase))
                    {
                        if ((stopwatch.ElapsedMilliseconds / 1000) < secondsWaitPhone)
                        {
                            Thread.Sleep(500);
                            continue;
                        }
                        _logMessage = $"[{nameof(secondsWaitPhone)}:{secondsWaitPhone}]\tWaiting limit getting a number phone reached";
                        return;
                    }
                    else
                    {
                        _logMessage = $"[{nameof(jobID)}:{jobID}]\tUnknown error while getting phone number";
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                _logMessage = ex.FormatException();
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
            _logMessage = string.Empty;
            _statusGetSmsCode = default;
            _counterAttemptsGetSmsCode += 1;

            var minWaitSmsCode = Settings.MinutesWaitSmsCode;
            var jobID = Data.JobID;
            var phone = Data.NumberPhone;
            var dll = Params.Dll;

            var smsCodeOrStatus = ZennoPoster.Sms.GetStatus(dll, jobID, "", minWaitSmsCode);
            var logCode = $"[{nameof(smsCodeOrStatus)}:{smsCodeOrStatus}]\t";

            if (Regex.IsMatch(smsCodeOrStatus, @"[0-9]{4,}"))
            {
                _logMessage = $"{logCode}[{nameof(phone)}:{phone}]\tSMS code received successfully";
                _statusGetSmsCode = true;

                Data.SmsCodeOrStatus = smsCodeOrStatus;

                return;
            }
            else if (string.IsNullOrWhiteSpace(smsCodeOrStatus)) _logMessage = $"Response to the SMS code request is empty or null";
            else if ("Error".Equals(smsCodeOrStatus, StringComparison.OrdinalIgnoreCase)) _logMessage = $"{logCode}[{nameof(minWaitSmsCode)}:{minWaitSmsCode}]\tStatus SMS code is error";
            else if ("Wait".Equals(smsCodeOrStatus, StringComparison.OrdinalIgnoreCase)) _logMessage = $"{logCode}SMS code waiting limit reached";
            else _logMessage = $"{logCode}Unknown error while getting SMS code";

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
                    _logMessage = $"[{nameof(responseCancel)}:{responseCancel}]\tPhone number canceled";
                }
            }
            catch (Exception ex)
            {
                _logMessage = $"[{nameof(ex.Message)}:{ex.Message}]";
            }
        }
    }
}
