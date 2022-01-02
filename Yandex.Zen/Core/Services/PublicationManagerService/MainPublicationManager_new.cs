using System;
using Yandex.Zen.Core.Interfaces;
using Yandex.Zen.Core.Interfaces.Services;

namespace Yandex.Zen.Core.Services.PublicationManagerService
{
    public class MainPublicationManager_new : IPublicationManagerService
    {
        public DataManager DataManager { get; set; }
        public IAuthorizationModule Authorization { get; set; }

        public MainPublicationManager_new(DataManager manager)
        {
            DataManager = manager;
        }

        public void Start()
        {
            throw new NotImplementedException();
        }
    }
}
