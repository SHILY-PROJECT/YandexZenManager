using System;
using System.IO;
using Yandex.Zen.Core.Toolkit.ResourceObject.Models;

namespace Yandex.Zen.Core.Toolkit.ResourceObject.Interfaces
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
