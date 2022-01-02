using System;
using System.IO;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace Yandex.Zen.Core.ServiceModules.ObjectModule.Models
{
    /// <summary>
    /// Модель с данными профиля.
    /// </summary>
    public class ProfileDataModel
    {
        private FileInfo _profile;

        /// <summary>
        /// Файл профиля.
        /// </summary>
        public FileInfo File
        {
            get
            {
                _profile.Refresh();
                return _profile;
            }
            set { _profile = value; }
        }

        /// <summary>
        /// Полный путь к файлу профиля.
        /// </summary>
        public string Path => File.FullName;

        /// <summary>
        /// Имя файла профиля.
        /// </summary>
        public string Name => File.Name;

        /// <summary>
        /// Использовать нагуленные профиля.
        /// </summary>
        public bool UseWalkedProfileFromSharedFolder { get; set; }

        /// <summary>
        /// Минимальный размер нагуленного профиля.
        /// </summary>
        public int MinProfileSizeToUse { get; set; }

        private DataManager_new DataManager { get; set; }

        /// <summary>
        /// Модель с данными профиля.
        /// </summary>
        public ProfileDataModel(DataManager_new manager)
        {
            DataManager = manager;
        }

        /// <summary>
        /// Модель с данными профиля.
        /// </summary>
        public ProfileDataModel(DataManager_new manager, bool useWalkedProfileFromSharedFolder, int minProfileSizeToUse) : this(manager)
        {
            UseWalkedProfileFromSharedFolder = useWalkedProfileFromSharedFolder;
            MinProfileSizeToUse = minProfileSizeToUse;
        }

        /// <summary>
        /// Установка профиля.
        /// </summary>
        public void SetProfile(FileInfo file)
        {
            File = file;
            if (File.Exists) Load();
        }

        /// <summary>
        /// Загрузка профиля.
        /// </summary>
        /// <param name="createVariables"></param>
        public void Load(bool createVariables = true)
            => DataManager.Zenno.Profile.Load(Path, createVariables);

        /// <summary>
        /// Сохранение профиля аккаунта.
        /// </summary>
        public void SaveProfile()
        {
            DataManager.Zenno.Profile.Save
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

        /// <summary>
        /// Удаление профиля.
        /// </summary>
        public void Delete()
        {
            try
            {
                File.Delete();
                Logger.Write($"[{Path}]\tПрофиль успешно удален", LoggerType.Info, true, false, false);
            }
            catch (Exception ex)
            {
                Logger.Write($"[{Path}]\tПрофиль не удалось удалить: {ex.Message}", LoggerType.Warning, true, false, true);
            }

        }
    }
}
