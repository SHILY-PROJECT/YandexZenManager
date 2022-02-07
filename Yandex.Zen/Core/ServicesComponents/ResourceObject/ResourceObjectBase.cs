using System.IO;
using Yandex.Zen.Core.Interfaces;
using Yandex.Zen.Core.Toolkit.SmsServiceTool;
using Yandex.Zen.Core.Toolkit;
using Yandex.Zen.Core.ServicesComponents.ResourceObject.Models;
using Yandex.Zen.Core.ServicesComponents.ResourceObject.Interfaces;

namespace Yandex.Zen.Core.ServicesComponents.ResourceObject
{
    public abstract class ResourceObjectBase : IResourceObject
    {
        protected static readonly object Locker = new object();
        protected IDataManager Manager { get; set; }

        public ResourceObjectBase(IDataManager manager)
        {
            Manager = manager;
        }

        public TemplateSettingsModel Settings { get; set; }
        public DirectoryInfo Directory { get; set; }
        public ProxyModel Proxy { get; set; }
        public SmsService SmsService { get; set; }
        public CaptchaService CaptchaService { get; set; }
    }
}
