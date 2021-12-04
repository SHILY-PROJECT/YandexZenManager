using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Services.CheatActivityService.Enums;
using Yandex.Zen.Core.Toolkit.LoggerTool.Models;

namespace Yandex.Zen.Core.Services.CheatActivityService.Models
{
    public class ActionModel
    {
        public string Login { get; set; }
        public AccountActionTypeEnum Action { get; set; }
        public TimeData TimeData { get; private set; }

        public ActionModel(string loginAction, AccountActionTypeEnum accountActionType)
        {
            TimeData = new TimeData();
            Login = loginAction;
            Action = accountActionType;
        }
    }
}
