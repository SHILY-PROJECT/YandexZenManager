using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using Yandex.Zen.Core.Toolkit.ObjectModule.Models;
using Yandex.Zen.Core.Toolkit.ObjectModule.Interfaces;
using Yandex.Zen.Core.Toolkit.SmsServiceTool;

namespace Yandex.Zen.Core.Toolkit.ObjectModule
{
    public class AccountModel : IAccount
    {
        public AccountModel()
        {

        }

        public IProfile Profile { get; set; }
        public SmsService SmsService { get; set; }
        public CaptchaService CaptchaService { get; set; }
        public DirectoryInfo Directory { get; set; }
        public ProxyModel ProxyData { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string AnswerQuestion { get; set; }
        public string PhoneNumber { get; set; }
        public string CurrentMessageInTable { get; set; }
        public Uri WebSite { get; set; }
        public ChannelModel Channel { get; set; }
        public TemplateSettingsModel Settings { get; set; }

        public bool TryConfigure(int row)
        {
            throw new NotImplementedException();
        }
    }
}
