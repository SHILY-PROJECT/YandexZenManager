using Yandex.Zen.Core.Interfaces;
using Yandex.Zen.Core.Interfaces.Services;

namespace Yandex.Zen.Core.Services
{
    public class ServiceManager : IServiceManager
    {
        public void RunService(DataManager_new manager, IChannelManagerService channelManager) => channelManager.Start(manager);
        public void RunService(DataManager_new manager, IPublicationManagerService publicationManager) => publicationManager.Start(manager);
        public void RunService(DataManager_new manager, IBrowserAccountManagerService browserAccountManager) => browserAccountManager.Start(manager);
        public void RunService(DataManager_new manager, IAccounRegisterService accounRegister) => accounRegister.Start(manager);
        public void RunService(DataManager_new manager, IWalkerOnZenService walkerOnZen) => walkerOnZen.Start(manager);
        public void RunService(DataManager_new manager, IActivityManagerService activityManager) => activityManager.Start(manager);
    }
}
