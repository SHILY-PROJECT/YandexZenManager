using System;
using System.IO;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using Yandex.Zen.Core.Toolkit.ResourceObject.Interfaces;

namespace Yandex.Zen.Core.Toolkit.ResourceObject
{
    public sealed class ProfileModel : ResourceObjectBase, IProfile
    {
        public ProfileModel(DataManager manager) : base(manager)
        {

        }

        public FileInfo File { get; set; }

        public void Delete()
        {
            try
            {
                File.Delete();
                Logger.Write($"Profile successfully deleted: '{File.FullName}'.", LoggerType.Info, true, false, false);
            }
            catch (Exception ex)
            {
                Logger.Write($"Profile failed to delete: '{ex.Message}'.", LoggerType.Warning, true, false, true);
            }
        }

        public void Load(bool createVariables = true)
        {
            if (File.Exists) Manager.Zenno.Profile.Load(File.FullName, createVariables);
            else Logger.Write($"Profile load: 'File not found'.", LoggerType.Info, true, false, false);
        }

        public void Save()
        {
            Manager.Zenno.Profile.Save
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
            Logger.Write($"Profile saved: '{File.FullName}'.", LoggerType.Info, true, false, false);
        }
    }
}
