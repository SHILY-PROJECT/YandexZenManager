using ZennoLab.InterfacesLibrary.ProjectModel;

namespace Yandex.Zen.Core.ServiceModules.ObjectModule.Interfaces
{
    public interface IConfigurationForService
    {
        bool TryConfigure(IZennoTable table, int row);
    }
}
