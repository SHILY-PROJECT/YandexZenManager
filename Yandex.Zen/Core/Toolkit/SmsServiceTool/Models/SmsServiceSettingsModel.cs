using ZennoLab.InterfacesLibrary.ProjectModel;
using Yandex.Zen.Core.Toolkit.Extensions;

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

        //public SmsServiceSettingsModel(ILocalVariable timeToSecondsWaitPhone, ILocalVariable minutesWaitSmsCode, ILocalVariable attemptsReSendSmsCode) :
        //    this(timeToSecondsWaitPhone.ExtractNumber(),
        //         minutesWaitSmsCode.Value.Split(' ')[0].ExtractNumber(),
        //         attemptsReSendSmsCode.Value.Split(' ')[0].ExtractNumber()) { }
    }
}
