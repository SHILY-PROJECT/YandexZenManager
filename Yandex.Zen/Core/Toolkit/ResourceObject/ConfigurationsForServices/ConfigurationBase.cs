using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Enums;
using Yandex.Zen.Core.Toolkit.ResourceObject;

namespace Yandex.Zen.Core.Toolkit.ResourceObject.ConfigurationsForServices
{
    public class ConfigurationBase
    {
        protected int ColLogin = (int)TableColumnsEnum.Login;
        protected int ColPassword = (int)TableColumnsEnum.Password;
        protected int ColProxy = (int)TableColumnsEnum.Proxy;
        protected int ColAnswerQuestion = (int)TableColumnsEnum.AnswerQuestion;
        protected int ColAccountPhone = (int)TableColumnsEnum.AccountNumberPhone;
        protected int ColChannelPhone = (int)TableColumnsEnum.ChannelNumberPhone;

        protected ResourceObjectBase Object { get; set; }
        protected DataManager DataManager { get; set; }

        public ConfigurationBase(DataManager manager, ResourceObjectBase obj)
        {
            DataManager = manager;
            Object = obj;
        }
    }
}
