using System.IO;
using Yandex.Zen.Core.Toolkit.SmsServiceTool;
using Yandex.Zen.Core.Toolkit;
using Yandex.Zen.Core.ServicesComponents.ResourceObject.Models;
using Yandex.Zen.Core.ServicesComponents.ResourceObject.Interfaces;

namespace Yandex.Zen.Core.ServicesComponents.ResourceObject
{
    public abstract class ResourceObjectBase : IResourceObject
    {
        protected static readonly object Locker = new object();
        protected DataManager Manager { get; set; }

        public ResourceObjectBase(DataManager manager)
        {
            Manager = manager;
        }

        public DirectoryInfo Directory { get; protected set; }
        public ProxyModel ProxyData { get; set; }
        public SmsService SmsService { get; set; }
        public CaptchaService CaptchaService { get; set; }
    }
}
