using ZennoLab.InterfacesLibrary.ProjectModel;

namespace Yandex.Zen.Core.Toolkit.ResourceObject.Interfaces
{
    public interface IResourceObjectConfiguration
    {
        bool IsSuccess { get; }
        string Message { get; }

        bool TryConfigure(IResourceObject res);
        void Configure(IResourceObject res);
    }
}
