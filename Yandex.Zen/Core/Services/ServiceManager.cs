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
        public void RunService(DataManager_new manager)
        {
            switch (Program.CurrentMode)
            {
                case ProgramModeEnum.AccounRegisterService: RunService(manager, new MainAccounRegister_new()); break;
                case ProgramModeEnum.BrowserAccountManagerService: RunService(manager, new MainBrowserAccountManager_new()); break;
                case ProgramModeEnum.ChannelManagerService: RunService(manager, new MainChannelManager_new()); break;
                case ProgramModeEnum.PublicationManagerService: RunService(manager, new MainPublicationManager_new()); break;
                case ProgramModeEnum.ActivityManagerService: RunService(manager, new MainActivityManager_new()); break;
                case ProgramModeEnum.WalkerOnZenService: RunService(manager, new MainWalkerOnZen_new()); break;
            }
        }

        public void RunService(DataManager_new manager, IChannelManagerService channelManager) => channelManager.Start(manager);
        public void RunService(DataManager_new manager, IPublicationManagerService publicationManager) => publicationManager.Start(manager);
        public void RunService(DataManager_new manager, IBrowserAccountManagerService browserAccountManager) => browserAccountManager.Start(manager);
        public void RunService(DataManager_new manager, IAccounRegisterService accounRegister) => accounRegister.Start(manager);
        public void RunService(DataManager_new manager, IWalkerOnZenService walkerOnZen) => walkerOnZen.Start(manager);
        public void RunService(DataManager_new manager, IActivityManagerService activityManager) => activityManager.Start(manager);
    }
}
