using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Interfaces.Services;

namespace Yandex.Zen.Core.Services.BrowserAccountManagerService
{
    public class MainBrowserAccountManager_new : IBrowserAccountManagerService
    {
        public DataManager_new DataManager { get; set; }

        public MainBrowserAccountManager_new(DataManager_new manager)
        {
            DataManager = manager;
        }

        public void Start()
        {
            throw new NotImplementedException();
        }
    }
}
