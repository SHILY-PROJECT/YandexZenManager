using Yandex.Zen.Core.Interfaces.Services;

namespace Yandex.Zen.Core.Interfaces
{
    public interface IServiceManager
    {
        void StartService(DataManager_new manager, IAccounRegisterService accounRegister);
        void StartService(DataManager_new manager, IActivityManagerService activityManager);
        void StartService(DataManager_new manager, IBrowserAccountManagerService browserAccountManager);
        void StartService(DataManager_new manager, IChannelManagerService channelManager);
        void StartService(DataManager_new manager, IPublicationManagerService publicationManager);
        void StartService(DataManager_new manager, IWalkerOnZenService walkerOnZen);
    }
}
