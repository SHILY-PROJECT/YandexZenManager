using Yandex.Zen.Core.Interfaces.Services;

namespace Yandex.Zen.Core.Interfaces
{
    public interface IServiceManager
    {
        void RunService(DataManager_new manager, IAccounRegisterService accounRegister);
        void RunService(DataManager_new manager, IActivityManagerService activityManager);
        void RunService(DataManager_new manager, IBrowserAccountManagerService browserAccountManager);
        void RunService(DataManager_new manager, IChannelManagerService channelManager);
        void RunService(DataManager_new manager, IPublicationManagerService publicationManager);
        void RunService(DataManager_new manager, IWalkerOnZenService walkerOnZen);
    }
}
