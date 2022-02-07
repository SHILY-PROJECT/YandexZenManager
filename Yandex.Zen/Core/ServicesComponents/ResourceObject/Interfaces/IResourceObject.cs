using System.IO;
using Yandex.Zen.Core.Toolkit;
using Yandex.Zen.Core.Toolkit.SmsServiceTool;
using Yandex.Zen.Core.ServicesComponents.ResourceObject.Models;

namespace Yandex.Zen.Core.ServicesComponents.ResourceObject.Interfaces
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
