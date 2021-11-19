using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace Yandex.Zen.Core.Models.AccountOrDonorModels
{
    public class SettingsAccountOrDonorFromZennoVariablesModel
    {
        public bool CreateFolderResourceIfNoExist { get; set; }

        public SettingsAccountOrDonorFromZennoVariablesModel(ILocalVariable createFolderResourceIfNoExist)
        {
            CreateFolderResourceIfNoExist = bool.Parse(createFolderResourceIfNoExist.Value);
        }
    }
}
