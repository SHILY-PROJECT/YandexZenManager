using System.IO;
using Yandex.Zen.Core.Toolkit.SmsServiceTool;
using Yandex.Zen.Core.Toolkit.ResourceObject.Models;

namespace Yandex.Zen.Core.Toolkit.ResourceObject.Interfaces
{
    public interface IResourceObject
    {
        TemplateSettingsModel TemplateSettings { get; set; }
        DirectoryInfo Directory { get; set; }
        ProxyModel Proxy { get; set; }
        SmsService SmsService { get; set; }
        CaptchaService CaptchaService { get; set; }
    }
}
