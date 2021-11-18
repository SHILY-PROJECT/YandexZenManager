using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Tools.Extensions;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace Yandex.Zen.Core.Models.AccountOrDonorModels
{
    public class SettingsUseProfileSharedModel
    {
        public bool UseWalkedProfileFromSharedFolder { get; set; }
        public int MinProfileSizeToUse { get; set; }

        public SettingsUseProfileSharedModel(bool useWalkedProfileFromSharedFolder, int minProfileSizeToUse)
        {
            UseWalkedProfileFromSharedFolder = useWalkedProfileFromSharedFolder;
            MinProfileSizeToUse = minProfileSizeToUse;
        }

        public SettingsUseProfileSharedModel(ILocalVariable useWalkedProfileFromSharedFolder, ILocalVariable minProfileSizeToUse) :
            this(Convert.ToBoolean(useWalkedProfileFromSharedFolder.Value), Convert.ToInt32(minProfileSizeToUse.Value)) { }
    }
}
