using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Interfaces.Services;

namespace Yandex.Zen.Core.Services.ActivityManagerService
{
    public class MainActivityManager_new : IActivityManagerService
    {
        public DataManager DataManager { get; set; }

        public MainActivityManager_new(DataManager manager)
        {
            DataManager = manager;
        }

        public void Start()
        {
            throw new NotImplementedException();
        }
    }
}
