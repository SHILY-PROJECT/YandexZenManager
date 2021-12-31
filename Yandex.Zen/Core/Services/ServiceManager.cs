using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Interfaces;

namespace Yandex.Zen.Core.Services
{
    public class ServiceManager : IServices
    {


        public void StartService(IAccounRegisterService accounRegister)
        {
            throw new NotImplementedException();
        }

        public void StartService(IActivityManagerService activityManager)
        {
            throw new NotImplementedException();
        }

        public void StartService(IBrowserAccountManagerService browserAccountManager)
        {
            throw new NotImplementedException();
        }

        public void StartService(IChannelManagerService channelManager)
        {
            throw new NotImplementedException();
        }

        public void StartService(IPublicationManagerService publicationManager)
        {
            throw new NotImplementedException();
        }

        public void StartService(IWalkerOnZenService walkerOnZen)
        {
            throw new NotImplementedException();
        }
    }
}
