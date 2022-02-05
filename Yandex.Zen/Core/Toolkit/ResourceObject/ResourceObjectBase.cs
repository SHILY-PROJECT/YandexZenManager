using Yandex.Zen.Core.Toolkit.SmsServiceTool;
using Yandex.Zen.Core.Toolkit.ResourceObject.Models;
using Yandex.Zen.Core.Toolkit.ResourceObject.Interfaces;
using System.IO;

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

        public string Name { get; private set; }
        public DirectoryInfo Directory { get; private set; }
        public ProxyModel ProxyData { get; set; }
        public SmsService SmsService { get; set; }
        public CaptchaService CaptchaService { get; set; }

        public bool TryConfigure(int row)
        {
            throw new System.NotImplementedException();

            //this = (ResourceObjectBase)new AccountModel(Manager);
        }

        public void Configure()
        {
            
        }
    }
}
