using Yandex.Zen.Core.Interfaces;
using Yandex.Zen.Core.ServicesComponents.ResourceObject.Interfaces;

namespace Yandex.Zen.Core.ServicesComponents.ResourceObject
{
    public sealed class DonorModel : AccountModel, IDonor
    {
        public DonorModel(IDataManager manager) : base(manager)
        {

        }
    }
}
