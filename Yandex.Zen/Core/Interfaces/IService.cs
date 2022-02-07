using Yandex.Zen.Core.ServicesComponents.ResourceObject.Interfaces;

namespace Yandex.Zen.Core.Interfaces
{
    public interface IService
    {
        IResourceObject ResourceObject { get; set; }
        IServiceConfiguration Configuration { get; }

        void Start();
    }
}
