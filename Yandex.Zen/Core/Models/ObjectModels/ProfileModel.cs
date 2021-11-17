using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Tools.LoggerTool;
using Yandex.Zen.Core.Tools.LoggerTool.Enums;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace Yandex.Zen.Core.Models.ObjectModels
{
    public class ProfileModel
    {
        private FileInfo _profile;
        private IZennoPosterProjectModel Zenno { get => ServicesComponents.Zenno; }


        public FileInfo File
        { 
            get
            {
                _profile.Refresh();
                return _profile;
            }
            set { _profile = value; }
        }
        public string Path => File.FullName;
        public string Name => File.Name;


        /// <summary>
        /// Сохранение профиля аккаунта.
        /// </summary>
        public void SaveProfile()
        {
            Zenno.Profile.Save
            (
                path: File.FullName,
                saveProxy: true,
                savePlugins: true,
                saveLocalStorage: true,
                saveTimezone: true,
                saveGeoposition: true,
                saveSuperCookie: true,
                saveFonts: true,
                saveWebRtc: true,
                saveIndexedDb: true,
                saveVariables: null
            );
            Logger.Write("Профиль сохранен", LoggerType.Info, true, false, false);
        }

        public void Load(bool createVariables = true)
            => Zenno.Profile.Load(Path, createVariables);

    }
}
