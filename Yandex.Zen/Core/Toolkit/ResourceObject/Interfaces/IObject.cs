using Yandex.Zen.Core.Toolkit.ResourceObject.Models;
using Yandex.Zen.Core.Toolkit.SmsServiceTool;

namespace Yandex.Zen.Core.Toolkit.ResourceObject.Interfaces
{
    public interface IObject : IConfiguration
    {
        ProxyModel ProxyData { get; set; }
        SmsService SmsService { get; set; }
        CaptchaService CaptchaService { get; set; }
    }
}
