using Yandex.Zen.Core.ServicesComponents.ResourceObject.Interfaces;

namespace Yandex.Zen.Core.Interfaces
{
    public interface IService
    {
        IAccount Account { get; set; }

        void Start();
    }
}
