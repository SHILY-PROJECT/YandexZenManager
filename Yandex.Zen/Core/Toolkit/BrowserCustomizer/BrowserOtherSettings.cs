using ZennoLab.CommandCenter;
using Yandex.Zen.Core.Toolkit.BrowserCustomizer.Models;

namespace Yandex.Zen.Core.Toolkit.BrowserCustomizer
{
    public static class BrowserOtherSettings
    {
        /// <summary>
        /// Извлечение настроек браузера из переменной.
        /// </summary>
        /// <param name="variable"></param>
        /// <returns></returns>
        public static BrowserOtherSettingsModel ExtractOtherSettingsFromVariable(string variable)
            => new BrowserOtherSettingsModel
            {
                LoadPictures = variable.Contains("Загрузка картинок"),
                DownloadActiveX = variable.Contains("Загрузка ActiveX"),
                RunActiveX = variable.Contains("Запуск ActiveX"),
                DownloadFrame = variable.Contains("Загрузка Frame"),
                UseCSS = variable.Contains("Загрузка CSS"),
                UseJavaApplets = variable.Contains("Использование JavaApplets"),
                UseJavaScripts = variable.Contains("Использование JavaScript"),
                UsePlugins = variable.Contains("Использование плагинов"),
                UsePluginsForceWmode = variable.Contains("Использование плагинов Wmode (загрузка плагинов в том-же окне)"),
                UseMedia = variable.Contains("Использование Media"),
                UseAdds = variable.Contains("Реклама"),
                DownloadVideos = variable.Contains("Загрузка видео"),
                AllowPopUp = variable.Contains("Разрешить всплывающее окно"),
                BackGroundSoundsPlay = variable.Contains("Воспроизведение фоновых звуков")
            };

        /// <summary>
        /// Получение текущих настроек браузера.
        /// </summary>
        /// <param name="browser"></param>
        /// <returns></returns>
        public static BrowserOtherSettingsModel BrowserGetCurrentOtherSettings(this Instance browser)
            => new BrowserOtherSettingsModel
            {
                LoadPictures = browser.LoadPictures,                   // Загрузка картинок
                DownloadActiveX = browser.DownloadActiveX,             // Загрузка ActiveX
                RunActiveX = browser.RunActiveX,                       // Запуск ActiveX
                DownloadFrame = browser.DownloadFrame,                 // Загрузка Frame
                UseCSS = browser.UseCSS,                               // Загрузка CSS
                UseJavaApplets = browser.UseJavaApplets,               // Использование JavaApplets
                UseJavaScripts = browser.UseJavaScripts,               // Использование JavaScript
                UsePlugins = browser.UsePlugins,                       // Использование плагинов
                UsePluginsForceWmode = browser.UsePluginsForceWmode,   // Использование плагинов Wmode (загрузка плагинов в том-же окне)
                UseMedia = browser.UseMedia,                           // Использование Media
                UseAdds = browser.UseAdds,                             // Реклама
                DownloadVideos = browser.DownloadVideos,               // Загрузка видео
                AllowPopUp = browser.AllowPopUp,                       // Разрешить всплывающее окно
                BackGroundSoundsPlay = browser.BackGroundSoundsPlay    // Воспроизведение фоновых звуков
            };

        /// <summary>
        /// Установка настроек браузера.
        /// </summary>
        /// <param name="browser"></param>
        /// <param name="otherSettings"></param>
        public static void BrowserSetOtherSettings(this Instance browser, BrowserOtherSettingsModel otherSettings)
        {
            browser.LoadPictures = otherSettings.LoadPictures;                  // Загрузка картинок
            browser.DownloadActiveX = otherSettings.DownloadActiveX;            // Загрузка ActiveX
            browser.RunActiveX = otherSettings.RunActiveX;                      // Запуск ActiveX
            browser.DownloadFrame = otherSettings.DownloadFrame;                // Загрузка Frame
            browser.UseCSS = otherSettings.UseCSS;                              // Загрузка CSS
            browser.UseJavaApplets = otherSettings.UseJavaApplets;              // Использование JavaApplets
            browser.UseJavaScripts = otherSettings.UseJavaScripts;              // Использование JavaScript
            browser.UsePlugins = otherSettings.UsePlugins;                      // Использование плагинов
            browser.UsePluginsForceWmode = otherSettings.UsePluginsForceWmode;  // Использование плагинов Wmode (загрузка плагинов в том-же окне)
            browser.UseMedia = otherSettings.UseMedia;                          // Использование Media
            browser.UseAdds = otherSettings.UseAdds;                            // Реклама
            browser.DownloadVideos = otherSettings.DownloadVideos;              // Загрузка видео
            browser.AllowPopUp = otherSettings.AllowPopUp;                      // Разрешить всплывающее окно
            browser.BackGroundSoundsPlay = otherSettings.BackGroundSoundsPlay;  // Воспроизведение фоновых звуков
        }

        /// <summary>
        /// Установить настройки браузера по умолчанию.
        /// </summary>
        /// <param name="browser"></param>
        public static void BrowserSetDefaultOtherSettings(this Instance browser)
        {
            browser.LoadPictures = true;         // Загрузка картинок
            browser.DownloadActiveX = true;      // Загрузка ActiveX
            browser.RunActiveX = true;           // Запуск ActiveX
            browser.DownloadFrame = true;        // Загрузка Frame
            browser.UseCSS = true;               // Загрузка CSS
            browser.UseJavaApplets = true;       // Использование JavaApplets
            browser.UseJavaScripts = true;       // Использование JavaScript
            browser.UsePlugins = true;           // Использование плагинов
            browser.UsePluginsForceWmode = true; // Использование плагинов Wmode (загрузка плагинов в том-же окне)
            browser.UseMedia = true;             // Использование Media
            browser.UseAdds = true;              // Реклама
            browser.DownloadVideos = true;       // Загрузка видео
            browser.AllowPopUp = true;           // Разрешить всплывающее окно
            browser.BackGroundSoundsPlay = true; // Воспроизведение фоновых звуков
        }
    }
}
