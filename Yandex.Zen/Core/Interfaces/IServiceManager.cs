using Yandex.Zen.Core.Enums;
using Yandex.Zen.Core.Interfaces.Services;

namespace Yandex.Zen.Core.Interfaces
{
    public interface IServiceManager
    {
        void RunService(DataManager manager, ProgramModeEnum mode);
        void RunService(IAccounRegisterService accounRegister);
        void RunService(IActivityManagerService activityManager);
        void RunService(IBrowserAccountManagerService browserAccountManager);
        void RunService(IChannelManagerService channelManager);
        void RunService(IPublicationManagerService publicationManager);
        void RunService(IWalkerOnZenService walkerOnZen);
    }
}
