using System;
using Yandex.Zen.Core.Interfaces;
using Yandex.Zen.Core.Interfaces.Services;

namespace Yandex.Zen.Core.Services
{
    public class ServiceManager : IServiceManager
    {
        public void StartService(DataManager_new manager, IAccounRegisterService accounRegister)
        {
            throw new NotImplementedException();
        }

        public void StartService(IActivityManagerService activityManager)
        {
            throw new NotImplementedException();
        }

        public void StartService(IBrowserAccountManagerService browserAccountManager)
        {
            throw new NotImplementedException();
        }

        public void StartService(IChannelManagerService channelManager)
        {
            throw new NotImplementedException();
        }

        public void StartService(IPublicationManagerService publicationManager)
        {
            throw new NotImplementedException();
        }

        public void StartService(IWalkerOnZenService walkerOnZen)
        {
            throw new NotImplementedException();
        }
    }
}
