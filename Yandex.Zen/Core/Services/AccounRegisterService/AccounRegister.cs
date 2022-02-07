using ZennoLab.InterfacesLibrary.Enums.Log;
using Yandex.Zen.Core.Interfaces;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using Yandex.Zen.Core.ServicesComponents.ResourceObject.Interfaces;

namespace Yandex.Zen.Core.Services.AccounRegisterService
{
    public class AccounRegister : IService
    {
        public AccounRegister(IDataManager manager, IServiceConfiguration configuration)
        {
            DataManager = manager;
            Configuration = configuration;
        }

        public IDataManager DataManager { get; set; }
        public IResourceObject ResourceObject { get; set; }
        public IServiceConfiguration Configuration { get; private set; }

        public void Start()
        {
            Logger.Write($"Тест сервиса: {nameof(AccounRegister)}", LoggerType.Info, false, false, true, LogColor.Green);
        }
    }
}
