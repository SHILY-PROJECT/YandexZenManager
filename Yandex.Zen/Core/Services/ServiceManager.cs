using Yandex.Zen.Core.Enums;
using Yandex.Zen.Core.Interfaces;
using Yandex.Zen.Core.Interfaces.Services;
using Yandex.Zen.Core.Services.AccounRegisterService;
using Yandex.Zen.Core.Services.ActivityManagerService;
using Yandex.Zen.Core.Services.BrowserAccountManagerService;
using Yandex.Zen.Core.Services.ChannelManagerService;
using Yandex.Zen.Core.Services.PublicationManagerService;
using Yandex.Zen.Core.Services.WalkerOnZenService;

namespace Yandex.Zen.Core.Services
{
    public class ServiceManager : IServiceManager
    {
        public void RunService(DataManager manager, ProgramModeEnum mode)
        {
            switch (mode)
            {
                case ProgramModeEnum.AccounRegisterService: RunService(new MainAccounRegister_new(manager)); break;
                case ProgramModeEnum.BrowserAccountManagerService: RunService(new MainBrowserAccountManager_new(manager)); break;
                case ProgramModeEnum.ChannelManagerService: RunService(new MainChannelManager_new(manager)); break;
                case ProgramModeEnum.PublicationManagerService: RunService(new MainPublicationManager_new(manager)); break;
                case ProgramModeEnum.ActivityManagerService: RunService(new MainActivityManager_new(manager)); break;
                case ProgramModeEnum.WalkerOnZenService: RunService(new MainWalkerOnZen_new(manager)); break;
            }
        }

        public void RunService(IChannelManagerService channelManager) => channelManager.Start();
        public void RunService(IPublicationManagerService publicationManager) => publicationManager.Start();
        public void RunService(IBrowserAccountManagerService browserAccountManager) => browserAccountManager.Start();
        public void RunService(IAccounRegisterService accounRegister) => accounRegister.Start();
        public void RunService(IWalkerOnZenService walkerOnZen) => walkerOnZen.Start();
        public void RunService(IActivityManagerService activityManager) => activityManager.Start();
    }
}
