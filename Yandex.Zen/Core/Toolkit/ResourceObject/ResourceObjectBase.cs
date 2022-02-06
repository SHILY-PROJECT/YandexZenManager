using System.IO;
using Yandex.Zen.Core.Toolkit.SmsServiceTool;
using Yandex.Zen.Core.Toolkit.ResourceObject.Models;
using Yandex.Zen.Core.Toolkit.ResourceObject.Interfaces;

namespace Yandex.Zen.Core.Toolkit.ResourceObject
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
