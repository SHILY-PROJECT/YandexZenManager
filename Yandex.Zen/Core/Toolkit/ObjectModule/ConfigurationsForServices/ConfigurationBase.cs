using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Enums;
using Yandex.Zen.Core.Toolkit.ObjectModule;

namespace Yandex.Zen.Core.Toolkit.ObjectModule.ConfigurationsForServices
{
    public class ConfigurationBase
    {
        protected int ColLogin = (int)TableColumnsEnum.Login;
        protected int ColPassword = (int)TableColumnsEnum.Password;
        protected int ColProxy = (int)TableColumnsEnum.Proxy;
        protected int ColAnswerQuestion = (int)TableColumnsEnum.AnswerQuestion;
        protected int ColAccountPhone = (int)TableColumnsEnum.AccountNumberPhone;
        protected int ColChannelPhone = (int)TableColumnsEnum.ChannelNumberPhone;

        protected ObjectModel Object { get; set; }
        protected DataManager DataManager { get; set; }

        public ConfigurationBase(DataManager manager, ObjectModel obj)
        {
            DataManager = manager;
            Object = obj;
        }
    }
}
