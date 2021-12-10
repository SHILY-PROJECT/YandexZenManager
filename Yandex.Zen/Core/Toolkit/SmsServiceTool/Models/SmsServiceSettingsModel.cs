namespace Yandex.Zen.Core.Toolkit.SmsServiceTool.Models
{
    public class SmsServiceSettingsModel
    {
        public int TimeToSecondsWaitPhone { get; private set; }
        public int MinutesWaitSmsCode { get; private set; }
        public int AttemptsReSendSmsCode { get; private set; }

        public SmsServiceSettingsModel(int timeToSecondsWaitPhone, int minutesWaitSmsCode, int attemptsReSendSmsCode)
        {
            TimeToSecondsWaitPhone = timeToSecondsWaitPhone;
            MinutesWaitSmsCode = minutesWaitSmsCode;
            AttemptsReSendSmsCode = attemptsReSendSmsCode;
        }
    }
}
