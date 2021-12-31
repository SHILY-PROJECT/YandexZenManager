namespace Yandex.Zen.Core.Interfaces
{
    public interface IServices
    {
        void StartService(IAccounRegisterService accounRegister);
        void StartService(IActivityManagerService activityManager);
        void StartService(IBrowserAccountManagerService browserAccountManager);
        void StartService(IChannelManagerService channelManager);
        void StartService(IPublicationManagerService publicationManager);
        void StartService(IWalkerOnZenService walkerOnZen);
    }
}
