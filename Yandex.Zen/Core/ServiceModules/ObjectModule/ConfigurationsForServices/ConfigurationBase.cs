using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Enums;
using Yandex.Zen.Core.Toolkit.TableTool.Enums;

namespace Yandex.Zen.Core.ServiceModules.ObjectModule.ConfigurationsForServices
{
    public class ConfigurationBase
    {
        protected int ColLogin = (int)MainTableColumnsEnum.Login;
        protected int ColPassword = (int)MainTableColumnsEnum.Password;
        protected int ColProxy = (int)MainTableColumnsEnum.Proxy;
        protected int ColAnswerQuestion = (int)MainTableColumnsEnum.AnswerQuestion;
        protected int ColAccountPhone = (int)MainTableColumnsEnum.AccountNumberPhone;
        protected int ColChannelPhone = (int)MainTableColumnsEnum.ChannelNumberPhone;

        protected ObjectBase Object { get; set; }
        protected DataManager DataManager { get; set; }

        public ConfigurationBase(DataManager manager, ObjectBase obj)
        {
            DataManager = manager;
            Object = obj;
        }
    }
}
