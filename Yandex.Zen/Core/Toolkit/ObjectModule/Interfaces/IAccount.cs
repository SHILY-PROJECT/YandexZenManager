using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Toolkit.ObjectModule.Models;
using Yandex.Zen.Core.Toolkit.SmsServiceTool;

namespace Yandex.Zen.Core.Toolkit.ObjectModule.Interfaces
{
    public interface IAccount : IObject
    {
        IProfile Profile { get; set; }
        TemplateSettingsModel Settings { get; set; }
        DirectoryInfo Directory { get; set; }      
        string Login { get; set; }
        string Password { get; set; }
        string AnswerQuestion { get; set; }
        string PhoneNumber { get; set; }
        string CurrentMessageInTable { get; set; }
        Uri WebSite { get; set; }
        ChannelModel Channel { get; set; }
    }
}
