using System.Linq;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace Yandex.Zen.Core.Models.AccountOrDonorModels.ProfileModels
{
    /// <summary>
    /// Настройки использования общих профилей.
    /// </summary>
    public class SettingsUseSharedProfileFromZennoVariablesModel
    {
        public bool UseWalkedProfileFromSharedFolder { get; set; }
        public int MinProfileSizeToUse { get; set; }

        public SettingsUseSharedProfileFromZennoVariablesModel(bool useWalkedProfileFromSharedFolder, int minProfileSizeToUse)
        {
            UseWalkedProfileFromSharedFolder = useWalkedProfileFromSharedFolder;
            MinProfileSizeToUse = minProfileSizeToUse;
        }

        public SettingsUseSharedProfileFromZennoVariablesModel(ILocalVariable useWalkedProfileFromSharedFolder, ILocalVariable minProfileSizeToUse) :
            this(bool.Parse(useWalkedProfileFromSharedFolder.Value), int.Parse(minProfileSizeToUse.Value)) { }
    }
}
