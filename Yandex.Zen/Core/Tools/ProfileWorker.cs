using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Yandex.Zen.Core.Tools.LoggerTool;
using Yandex.Zen.Core.Tools.LoggerTool.Enums;
using ZennoLab.InterfacesLibrary.Enums.Log;

namespace Yandex.Zen.Core.Tools
{
    public class ProfileWorker : ServicesComponents
    {
        /// <summary>
        /// Получение и загрузка профиля (если в рабочей папке нет профиля, то он будет получен из общей папки).
        /// </summary>
        /// <param name="useSharedProfileIfResourceProfileNotExist"></param>
        /// <returns></returns>
        public static bool LoadProfile(bool useSharedProfileIfResourceProfileNotExist)
        {
            switch (useSharedProfileIfResourceProfileNotExist)
            {
                case true: return CheckAndLoadProfile(MinSizeProfileUseInModes);
                case false: return CheckAndLoadProfile();
            }

            return false;
        }

        /// <summary>
        /// Сохраняет зенно профиль (требуется Profile.FullName).
        /// </summary>
        public static void SaveProfile(bool refreshProfileInfo)
        {
            Zenno.Profile.Save
            (
                path: ProfileInfo.FullName,
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

            // Обновить информацию о профиле
            if (refreshProfileInfo) ProfileInfo.Refresh();

            // Логировать информацию о успешном сохранение
            Logger.Write("Профиль сохранен", LoggerType.Info, true, false, false);
        }

        /// <summary>
        /// Получение и загрузка профиля (если в рабочей папке нет профиля, то он будет получен из общей папки).
        /// </summary>
        /// <param name="minSizeProfile"></param>
        /// <returns></returns>
        private static bool CheckAndLoadProfile(int minSizeProfile)
        {
            // Получение профиля из папки аккаунта
            var profiles = ObjectDirectory.EnumerateFiles("*.zpprofile", SearchOption.TopDirectoryOnly);

            if (profiles.Count() == 0)
            {
                ProfileInfo = GetProfileFromCommonFolder(minSizeProfile);

                if (ProfileInfo == null)
                {
                    return false;
                }
                else ProfileRetrievedFromSharedFolder = true;

                Logger.Write($"[Profile: {ProfileInfo.FullName} | {ProfileInfo.Length / 1024} КБ]\tПрофиль для работы успешно получен из общей папки", LoggerType.Info, true, false, true);

                var newPathProfile = Path.Combine(ObjectDirectory.FullName, ProfileInfo.Name);

                // Копирование профиля из общий папки в папку с аккаунтом
                File.Copy(ProfileInfo.FullName, newPathProfile);

                Thread.Sleep(3000);

                // Удаление профиля скопированного профиля
                try
                {
                    File.Delete(ProfileInfo.FullName);
                }
                catch (Exception ex)
                {
                    Logger.Write($"[Exception message: {ex.Message}]\t[Profile: {ProfileInfo.FullName}]\tНе удалось удалить профиль", LoggerType.Warning, true, true, true);
                }

                ProfileInfo = new FileInfo(newPathProfile);
            }
            else ProfileInfo = profiles.First();

            if (ProfileInfo == null)
            {
                Logger.Write($"Не удалось загрузить профиль", LoggerType.Warning, true, true, true, LogColor.Yellow);
                return false;
            }
            else
            {
                Zenno.Profile.Load(ProfileInfo.FullName, true);
                Logger.Write($"Профиль успешно загружен", LoggerType.Info, true, false, true);
                return true;
            }
        }

        /// <summary>
        /// Получение и загрузка профиля.
        /// </summary>
        /// <returns></returns>
        private static bool CheckAndLoadProfile()
        {
            var profiles = ObjectDirectory.EnumerateFiles("*.zpprofile", SearchOption.TopDirectoryOnly);

            if (profiles.Count() == 0)
            {
                Logger.Write($"[Папка: {ObjectDirectory.FullName}]\tПрофиль отсутствует", LoggerType.Warning, true, true, true, LogColor.Yellow);
                return false;
            }
            else ProfileInfo = profiles.First();

            Zenno.Profile.Load(ProfileInfo.FullName, true);

            Logger.Write($"Профиль успешно загружен", LoggerType.Info, true, false, true);
            return true;
        }

        /// <summary>
        /// Получение профиля для регистрации (автоматически заносит профиль в ListProfileInWork).
        /// </summary>
        /// <returns></returns>
        private static FileInfo GetProfileFromCommonFolder(int minSizeProfile)
        {
            if (Program.ProfilesDirectory == null) return null;

            var profiles = Program.ProfilesDirectory.EnumerateFiles("*.zpprofile", SearchOption.TopDirectoryOnly).ToList();

            if (profiles.Count == 0)
            {
                Logger.Write($"[Папка с профилями: {Program.ProfilesDirectory.FullName}]\tПрофиля в папке отсутствуют", LoggerType.Warning, true, true, true, LogColor.Yellow);
                return null;
            }

            profiles = profiles.Where(x => x.Length / 1024 > minSizeProfile).ToList();

            while (true)
            {
                if (profiles.Count == 0)
                {
                    Logger.Write($"[Папка с профилями: {Program.ProfilesDirectory.FullName}]\t[Минимальный размер профиля: {minSizeProfile} КБ]\tНе найдено подходящих профилей", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    return null;
                }

                var firstProfile = profiles.First();

                if (!Program.CurrentObjectsOfAllThreadsInWork.Any(x => x == firstProfile.FullName))
                {
                    Program.CurrentObjectCache.Add(firstProfile.FullName);
                    Program.CurrentObjectsOfAllThreadsInWork.Add(firstProfile.FullName);
                    return firstProfile;
                }
                else profiles.RemoveAt(0);
            }
        }
    }
}
