using Yandex.Zen.Core.Interfaces;
using Yandex.Zen.Core.Toolkit.ResourceObject.Interfaces;

namespace Yandex.Zen.Core.Toolkit.ResourceObject.Objects
{
    public sealed class DonorModel : AccountModel, IDonor
    {
        public DonorModel(IDataManager manager) : base(manager)
        {

        }
    }
}
