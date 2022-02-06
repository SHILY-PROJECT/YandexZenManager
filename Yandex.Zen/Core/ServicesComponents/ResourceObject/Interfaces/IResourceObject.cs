using System.IO;
using Yandex.Zen.Core.ServicesComponents.ResourceObject.Models;
using Yandex.Zen.Core.Toolkit;
using Yandex.Zen.Core.Toolkit.SmsServiceTool;

namespace Yandex.Zen.Core.ServicesComponents.ResourceObject.Interfaces
{
    public interface IResourceObject
    {
        DirectoryInfo Directory { get; }
        ProxyModel ProxyData { get; set; }
        SmsService SmsService { get; set; }
        CaptchaService CaptchaService { get; set; }
    }
}
