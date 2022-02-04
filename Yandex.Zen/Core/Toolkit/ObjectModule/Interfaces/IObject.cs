using Yandex.Zen.Core.Toolkit.ObjectModule.Models;
using Yandex.Zen.Core.Toolkit.SmsServiceTool;

namespace Yandex.Zen.Core.Toolkit.ObjectModule.Interfaces
{
    public interface IObject : IConfiguration
    {
        ProxyModel ProxyData { get; set; }
        SmsService SmsService { get; set; }
        CaptchaService CaptchaService { get; set; }
    }
}
