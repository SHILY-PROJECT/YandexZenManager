using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace Yandex.Zen.Core.Models.AccountOrDonorModels.ProfileModels
{
    /// <summary>
    /// Модель с данными профиля.
    /// </summary>
    public class ProfileModel
    {
        #region====================================================================
        private IZennoPosterProjectModel Zenno { get => ServicesComponents.Zenno; }
        private FileInfo _profile;
        #endregion=================================================================


        /// <summary>
        /// Модель с данными профиля.
        /// </summary>
        public ProfileModel() { }

        /// <summary>
        /// Модель с данными профиля.
        /// </summary>
        /// <param name="settingsFromZennoVariables">Настройки использования общих профилей.</param>
        public ProfileModel(SettingsUseSharedProfileFromZennoVariablesModel settingsFromZennoVariables)
        {
            SettingsFromZennoVariables = settingsFromZennoVariables;
        }


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
        /// Настройки использования общих профилей.
        /// </summary>
        public SettingsUseSharedProfileFromZennoVariablesModel SettingsFromZennoVariables { get; set; }


        /// <summary>
        /// Загрузка профиля.
        /// </summary>
        /// <param name="createVariables"></param>
        public void Load(bool createVariables = true) => Zenno.Profile.Load(Path, createVariables);

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
