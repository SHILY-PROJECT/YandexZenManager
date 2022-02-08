using Yandex.Zen.Core.Interfaces;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using Yandex.Zen.Core.Toolkit.ResourceObject.Interfaces;
using ZennoLab.InterfacesLibrary.Enums.Log;

namespace Yandex.Zen.Core.Services.WalkerOnZenService
{
    public class WalkerOnZen : IService
    {
        public WalkerOnZen(IDataManager manager, IServiceConfiguration configuration)
        {
            DataManager = manager;
            Configuration = configuration;
        }

        public IDataManager DataManager { get; set; }
        public IResourceObject ResourceObject { get; set; }
        public IServiceConfiguration Configuration { get; }

        public void Start()
        {
            Logger.Write($"Тест сервиса: {nameof(WalkerOnZen)}", LoggerType.Info, false, false, true, LogColor.Green);
        }
    }
}
