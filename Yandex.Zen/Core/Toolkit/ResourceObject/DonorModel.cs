using Yandex.Zen.Core.Toolkit.ResourceObject.Interfaces;

namespace Yandex.Zen.Core.Toolkit.ResourceObject
{
    public sealed class DonorModel : AccountModel, IDonor
    {
        public DonorModel(DataManager manager) : base(manager)
        {

        }
    }
}
