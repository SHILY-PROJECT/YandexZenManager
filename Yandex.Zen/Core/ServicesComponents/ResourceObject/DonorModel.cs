using Yandex.Zen.Core.ServicesComponents.ResourceObject.Interfaces;

namespace Yandex.Zen.Core.ServicesComponents.ResourceObject
{
    public sealed class DonorModel : AccountModel, IDonor
    {
        public DonorModel(DataManager manager) : base(manager)
        {

        }
    }
}
