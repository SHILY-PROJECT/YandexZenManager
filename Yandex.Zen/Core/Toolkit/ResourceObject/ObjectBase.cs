using Yandex.Zen.Core.Toolkit.ResourceObject.Interfaces;
using Yandex.Zen.Core.Toolkit.ResourceObject.Models;
using Yandex.Zen.Core.Toolkit.SmsServiceTool;

namespace Yandex.Zen.Core.Toolkit.ResourceObject
{
    public abstract class ObjectBase : IObject
    {
        private static readonly object _locker = new object();
        protected DataManager DataManager { get; set; }

        public ProxyModel ProxyData { get; set; }
        public SmsService SmsService { get; set; }
        public CaptchaService CaptchaService { get; set; }

        public ObjectBase(DataManager manager)
        {
            DataManager = manager;
        }

        public bool TryConfigure(int row)
        {
            throw new System.NotImplementedException();
        }
    }
}
