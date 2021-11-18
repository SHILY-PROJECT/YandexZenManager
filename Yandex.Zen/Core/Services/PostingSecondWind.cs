using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Models.AccountOrDonorModels;

namespace Yandex.Zen.Core.Services
{
    public class PostingSecondWind
    {
        private static readonly object _locker = new object();

        [ThreadStatic] private static AccountOrDonorBaseModel _account;

        public PostingSecondWind()
        {
            
        }

        public void Start()
        {
            if (_account is null)
                throw new Exception($"'{nameof(_account)}' - равен null");
        }
    }
}
