using ZennoLab.InterfacesLibrary.Enums.Log;
using Yandex.Zen.Core.Interfaces;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;

namespace Yandex.Zen.Core.Services.AccounRegisterService
{
    public class AccounRegister : IService
    {
        public DataManager DataManager { get; set; }

        public AccounRegister(DataManager manager)
        {
            DataManager = manager;
        }

        public void Start()
        {
            Logger.Write($"Тест сервиса: {nameof(AccounRegister)}", LoggerType.Info, false, false, true, LogColor.Green);
        }
    }
}
