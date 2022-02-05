using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Toolkit.ResourceObject.Interfaces;

namespace Yandex.Zen.Core.Toolkit.ResourceObject.Models
{
    public class ResourceObjectConfiguration
    {
        private DataManager _manager;
        private IResourceObject _res;
       // private Type _resType;

        //private Dictionary<Type, Type> _mapper => new Dictionary<Type, Type>
        //{
        //    { t }
        //};

        public ResourceObjectConfiguration(DataManager manager, IResourceObject res)
        {
            
            _manager = manager;
            _res = res;
        }

        public string Message { get; set; } = "The configuration has not yet been started.";
        public bool IsReady { get; set; }

        public void Configure()
        {
            if (_res is ProfileModel prof)
            {

            }
            else if (_res is AccountModel acc)
            {

            }
            else if (_res is DonorModel don)
            {

            }
            else throw new InvalidOperationException($"There is no configuration for this type: '{_res.GetType()}'.");
        }
    }
}
