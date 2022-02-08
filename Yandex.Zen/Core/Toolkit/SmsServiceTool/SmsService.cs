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

        public SmsService()
        {
        
        }

        public SmsService(string serviceDllAndCountry, int timeToSecondsWaitPhone, int minutesWaitSmsCode, int attemptsReSendSmsCode)
        {
            ServiceParams = new SmsServiceParamsDataModel(serviceDllAndCountry);
            WaitPhoneTimeOfSeconds = timeToSecondsWaitPhone;
            WaitSmsCodeOfMinutes = minutesWaitSmsCode;
            AttemptsReSendSmsCode = attemptsReSendSmsCode;
        }

        public int WaitPhoneTimeOfSeconds { get; set; }
        public int WaitSmsCodeOfMinutes { get; set; }
        public int AttemptsReSendSmsCode { get; set; }

        /// <summary>
        /// Параметры для получения номера (dll, страна и т.д.).
        /// </summary>
        public SmsServiceParamsDataModel ServiceParams { get; set; }

        /// <summary>
        /// Данные полученные в ходе работы (id задания, номер и т.д.).
        /// </summary>
        public SmsServiceDataModel Data { get; set; } = new SmsServiceDataModel();

        /// <summary>
        /// Информация для лога (может быть как гуд ответ, так и бэд)
        /// </summary>
        public string LogMessage { get => _logMessage; }

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
            string jobID, phone;

            _logMessage = string.Empty;
            _statusGetNumberPhone = false;

            var stopwatch = new Stopwatch();
            var dll = ServiceParams.Dll;
            var networkService = ServiceParams.NetworkService;
            var country = ServiceParams.Country;

            stopwatch.Start();

            try
            {
                while (true)
                {
                    jobID = ZennoPoster.Sms.GetNumber
                    (
                        serviceDll: dll,
                        service: networkService,
                        param: country,
                        oper: "any",
                        number: out phone,
                        forward: null
                    );

                    if (!phone.Equals("No numbers", StringComparison.OrdinalIgnoreCase) ||
                         stopwatch.ElapsedMilliseconds / 1000 >= WaitPhoneTimeOfSeconds) break;

                    Thread.Sleep(500);
                }
                
                switch (phone)
                {
                    case string p when (!string.IsNullOrWhiteSpace(p) && Regex.IsMatch(p, @"[0-9]{5,}")):
                    {
                        Data.JobID = jobID;
                        Data.NumberPhone = phone;
                        Data.NumberPhoneForServiceView = phone.Contains("+") ? phone : $"+{phone}";

                        _logMessage = $"[{nameof(ServiceParams.Dll)}:{dll}]\t[{nameof(Data.JobID)}:{jobID}]\t[{nameof(Data.NumberPhone)}:{phone}]\t[{stopwatch.ElapsedMilliseconds / 1000} sec]\tPhone number received successfully";
                        _statusGetNumberPhone = true;
                    }
                    break;

                    case string p when "No numbers".Equals(p, StringComparison.OrdinalIgnoreCase):
                    {
                        _logMessage = $"[{nameof(WaitPhoneTimeOfSeconds)}:{WaitPhoneTimeOfSeconds}]\tWaiting limit getting a number phone reached";
                    }
                    break;

                    default:
                    {
                        _logMessage = $"[{nameof(Data.JobID)}:{jobID}]\tUnknown error while getting phone number";
                    }
                    break;
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

            var jobID = Data.JobID;
            var phone = Data.NumberPhone;
            var dll = ServiceParams.Dll;

            var smsCodeOrStatus = ZennoPoster.Sms.GetStatus(dll, jobID, "", WaitSmsCodeOfMinutes);
            var logCode = $"[{nameof(smsCodeOrStatus)}:{smsCodeOrStatus}]\t";

            switch (smsCodeOrStatus)
            {
                case string s when Regex.IsMatch(s, @"[0-9]{4,}"):
                {
                    _logMessage = $"{logCode}[{nameof(Data.NumberPhone)}:{phone}]\tSMS code received successfully";
                    _statusGetSmsCode = true;

                    Data.SmsCodeOrStatus = s;
                }
                return;

                case string s when string.IsNullOrWhiteSpace(s):
                {
                    _logMessage = $"Response to the SMS code request is empty or null.";
                }
                break;

                case string s when "Error".Equals(s, StringComparison.OrdinalIgnoreCase):
                {
                    _logMessage = $"{logCode}[{nameof(WaitSmsCodeOfMinutes)}:{WaitSmsCodeOfMinutes}]\tStatus SMS code is error.";
                }
                break;

                case string s when "Wait".Equals(s, StringComparison.OrdinalIgnoreCase):
                {
                    _logMessage = $"{logCode}SMS code waiting limit reached.";
                }
                break;

                default:
                {
                    _logMessage = $"{logCode}Unknown error while getting SMS code";
                }
                break;
            }

            if (cancelNumberPhoneIfСodeIsNotReceived) CancelPhoneNumber();
        }

        /// <summary>
        /// Отмена номера.
        /// </summary>
        /// <param name="jobID"></param>
        public void CancelPhoneNumber()
        {
            var jobID = Data.JobID;
            var dll = ServiceParams.Dll;

            try
            {
                if (jobID != "-1")
                {
                    var responseCancel = ZennoPoster.Sms.SetStatus(dll, jobID, SmsServiceStatus.Cancel);
                    _logMessage = $"[{nameof(responseCancel)}:{responseCancel}]\tPhone number canceled";
                }
            }
            catch (Exception ex)
            {
                _logMessage = ex.FormatException();
            }
        }
    }
}
