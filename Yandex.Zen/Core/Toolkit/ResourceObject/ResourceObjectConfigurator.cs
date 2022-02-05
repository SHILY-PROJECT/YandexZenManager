using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Toolkit.ResourceObject.Interfaces;

namespace Yandex.Zen.Core.Toolkit.ResourceObject
{
    public class ResourceObjectConfigurator : IResourceObjectConfiguration
    {
        private bool _isSuccess;
        private string _message;

        public bool IsSuccess { get => _isSuccess; }
        public string Message { get => _message; }

        public void Configure(IResourceObject res)
        {
            throw new NotImplementedException();
        }

        public bool TryConfigure(IResourceObject res)
        {
            throw new NotImplementedException();
        }
    }
}
