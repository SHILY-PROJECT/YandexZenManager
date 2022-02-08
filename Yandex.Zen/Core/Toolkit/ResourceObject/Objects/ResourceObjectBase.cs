using System.IO;
using Yandex.Zen.Core.Interfaces;
using Yandex.Zen.Core.Toolkit.SmsServiceTool;
using Yandex.Zen.Core.Toolkit.ResourceObject.Interfaces;
using Yandex.Zen.Core.Toolkit.ResourceObject.Models;

namespace Yandex.Zen.Core.Toolkit.ResourceObject.Objects
{
    public abstract class ResourceObjectBase : IResourceObject
    {
        protected static readonly object Locker = new object();

        public ResourceObjectBase(IDataManager manager)
        {
            Manager = manager;
        }

        protected IDataManager Manager { get; set; }

        public TemplateSettingsModel TemplateSettings { get; set; }
        public DirectoryInfo Directory { get; set; }
        public ProxyModel Proxy { get; set; }
        public SmsService SmsService { get; set; }
        public CaptchaService CaptchaService { get; set; }
    }
}
