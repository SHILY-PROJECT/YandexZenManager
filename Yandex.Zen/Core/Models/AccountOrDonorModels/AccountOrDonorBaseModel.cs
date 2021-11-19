using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Enums;
using Yandex.Zen.Core.Models.AccountOrDonorModels.ProfileModels;
using Yandex.Zen.Core.Toolkit.PhoneServiceTool.Models;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace Yandex.Zen.Core.Models.AccountOrDonorModels
{
    /// <summary>
    /// Класс для хранения данных аккаунта.
    /// </summary>
    public class AccountOrDonorBaseModel
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public string AnswerQuestion { get; set; }
        public string PhoneNumber { get; set; }
        public Uri Instagram { get; set; }
        public PhoneDataModel PhoneData { get; set; }

        public ObjectTypeEnum Type { get; set; }
        public DirectoryInfo Directory { get; set; }
        public ProfileModel Profile { get; set; }
        public ProxyDataModel ProxyData { get; set; }
        public SettingsAccountOrDonorFromZennoVariablesModel SettingsFromZennoVariables { get; set; }

        public AccountOrDonorBaseModel(SettingsAccountOrDonorFromZennoVariablesModel settingsFromZennoVariables, SettingsUseSharedProfileFromZennoVariablesModel settingsUseProfileShared)
        {
            Profile = new ProfileModel(settingsUseProfileShared);
            SettingsFromZennoVariables = settingsFromZennoVariables;
            PhoneData = ProjectComponents.PhoneServiceNew.Data;
        }
    }
}
