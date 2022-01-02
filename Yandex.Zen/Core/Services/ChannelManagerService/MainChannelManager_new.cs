using System;
using Yandex.Zen.Core.Interfaces;
using Yandex.Zen.Core.Interfaces.Services;

namespace Yandex.Zen.Core.Services.ChannelManagerService
{
    public class MainChannelManager_new : IChannelManagerService
    {
        public DataManager_new DataManager { get; set; }
        public IAuthorizationModule Authorization { get; set; }

        public MainChannelManager_new(DataManager_new manager)
        {
            DataManager = manager;
        }

        public void Start()
        {
            throw new NotImplementedException();
        }
    }
}
