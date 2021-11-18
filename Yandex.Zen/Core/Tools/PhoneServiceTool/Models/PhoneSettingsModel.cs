using ZennoLab.InterfacesLibrary.ProjectModel;
using Yandex.Zen.Core.Tools.Extensions;

namespace Yandex.Zen.Core.Tools.PhoneServiceTool.Models
{
    public class PhoneSettingsModel
    {
        public int TimeToSecondsWaitPhone { get; private set; }
        public int MinutesWaitSmsCode { get; private set; }
        public int AttemptsReSendSmsCode { get; private set; }

        public PhoneSettingsModel(int timeToSecondsWaitPhone, int minutesWaitSmsCode, int attemptsReSendSmsCode)
        {
            TimeToSecondsWaitPhone = timeToSecondsWaitPhone;
            MinutesWaitSmsCode = minutesWaitSmsCode;
            AttemptsReSendSmsCode = attemptsReSendSmsCode;
        }

        public PhoneSettingsModel(ILocalVariable timeToSecondsWaitPhone, ILocalVariable minutesWaitSmsCode, ILocalVariable attemptsReSendSmsCode) :
            this (timeToSecondsWaitPhone.ExtractNumber(), minutesWaitSmsCode.Value.Split(' ')[0].ExtractNumber(), attemptsReSendSmsCode.Value.Split(' ')[0].ExtractNumber()) { }
    }
}
