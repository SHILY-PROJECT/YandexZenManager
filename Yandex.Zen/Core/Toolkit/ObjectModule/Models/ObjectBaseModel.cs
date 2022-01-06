using System;
using System.IO;
using Yandex.Zen.Core.Enums;
using Yandex.Zen.Core.Toolkit.SmsServiceTool;

namespace Yandex.Zen.Core.Toolkit.ObjectModule.Models
{
    public class ObjectBaseModel
    {
        public ObjectTypeEnum Type { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string AnswerQuestion { get; set; }
        public string PhoneNumber { get; set; }
        public string CurrentMessageInTable { get; set; }
        public Uri WebSite { get; set; }
        public ChannelDataModel Channel { get; set; }
        public DirectoryInfo Directory { get; set; }
        public ProfileDataModel ProfileData { get; set; }
        public ProxyDataModel ProxyData { get; set; }
        public CaptchaService CaptchaService { get; set; }
        public SmsService SmsService { get; set; }
        public ObjectSettingsModel Settings { get; set; }
    }
}
