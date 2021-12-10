namespace Yandex.Zen.Core.Toolkit.SmsServiceTool.Models
{
    public class SmsServiceSettingsModel
    {
        public int TimeToSecondsWaitPhone { get; set; }
        public int MinutesWaitSmsCode { get; set; }
        public int AttemptsReSendSmsCode { get; set; }

        public SmsServiceSettingsModel() { }

        public SmsServiceSettingsModel(int timeToSecondsWaitPhone, int minutesWaitSmsCode, int attemptsReSendSmsCode)
        {
            TimeToSecondsWaitPhone = timeToSecondsWaitPhone;
            MinutesWaitSmsCode = minutesWaitSmsCode;
            AttemptsReSendSmsCode = attemptsReSendSmsCode;
        }
    }
}
