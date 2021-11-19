using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Yandex.Zen.Core.Enums;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.Enums.Log;

namespace Yandex.Zen.Core.Toolkit
{
    public class InstanceSettings
    {
        public class OtherSettings
        {
            //Добавить в выводные настройки
            //Настройки инстанса:{Загрузка картинок|Загрузка ActiveX|Запуск ActiveX|Загрузка Frame|Загрузка CSS|Использование JavaApplets|Использование JavaScript|Использование плагинов|Использование плагинов Wmode (загрузка плагинов в том-же окне)|Использование Media|Реклама|Загрузка видео|Разрешить всплывающее окно|Воспроизведение фоновых звуков}

            /// <summary>
            /// Загрузка картинок.
            /// </summary>
            public bool LoadPictures { get; private set; }
            /// <summary>
            /// Загрузка ActiveX.
            /// </summary>
            public bool DownloadActiveX { get; private set; }
            /// <summary>
            /// Запуск ActiveX.
            /// </summary>
            public bool RunActiveX { get; private set; }
            /// <summary>
            /// Загрузка Frame.
            /// </summary>
            public bool DownloadFrame { get; private set; }
            /// <summary>
            /// Загрузка CSS.
            /// </summary>
            public bool UseCSS { get; private set; }
            /// <summary>
            /// Использование JavaApplets.
            /// </summary>
            public bool UseJavaApplets { get; private set; }
            /// <summary>
            /// Использование JavaScript.
            /// </summary>
            public bool UseJavaScripts { get; private set; }
            /// <summary>
            /// Использование плагинов.
            /// </summary>
            public bool UsePlugins { get; private set; }
            /// <summary>
            /// Использование плагинов Wmode (загрузка плагинов в том-же окне).
            /// </summary>
            public bool UsePluginsForceWmode { get; private set; }
            /// <summary>
            /// Использование Media.
            /// </summary>
            public bool UseMedia { get; private set; }
            /// <summary>
            /// Реклама.
            /// </summary>
            public bool UseAdds { get; private set; }
            /// <summary>
            /// Загрузка видео.
            /// </summary>
            public bool DownloadVideos { get; private set; }
            /// <summary>
            /// Разрешить всплывающее окно.
            /// </summary>
            public bool AllowPopUp { get; private set; }
            /// <summary>
            /// Воспроизведение фоновых звуков.
            /// </summary>
            public bool BackGroundSoundsPlay { get; private set; }

            public static OtherSettings ExtractOtherSettingsFromVariable(string variable)
            {
                var instanceSettings = new OtherSettings
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

                return instanceSettings;
            }

            public static OtherSettings GetCurrentOtherSettings()
            {
                var instance = ServicesComponents.Instance;

                var instanceSettings = new OtherSettings
                {
                    LoadPictures = instance.LoadPictures,                   // Загрузка картинок
                    DownloadActiveX = instance.DownloadActiveX,             // Загрузка ActiveX
                    RunActiveX = instance.RunActiveX,                       // Запуск ActiveX
                    DownloadFrame = instance.DownloadFrame,                 // Загрузка Frame
                    UseCSS = instance.UseCSS,                               // Загрузка CSS
                    UseJavaApplets = instance.UseJavaApplets,               // Использование JavaApplets
                    UseJavaScripts = instance.UseJavaScripts,               // Использование JavaScript
                    UsePlugins = instance.UsePlugins,                       // Использование плагинов
                    UsePluginsForceWmode = instance.UsePluginsForceWmode,   // Использование плагинов Wmode (загрузка плагинов в том-же окне)
                    UseMedia = instance.UseMedia,                           // Использование Media
                    UseAdds = instance.UseAdds,                             // Реклама
                    DownloadVideos = instance.DownloadVideos,               // Загрузка видео
                    AllowPopUp = instance.AllowPopUp,                       // Разрешить всплывающее окно
                    BackGroundSoundsPlay = instance.BackGroundSoundsPlay    // Воспроизведение фоновых звуков
                };

                return instanceSettings;
            }

            public static void SetOtherSettings(OtherSettings otherSettings)
            {
                var instance = ServicesComponents.Instance;

                instance.LoadPictures = otherSettings.LoadPictures;                  // Загрузка картинок
                instance.DownloadActiveX = otherSettings.DownloadActiveX;            // Загрузка ActiveX
                instance.RunActiveX = otherSettings.RunActiveX;                      // Запуск ActiveX
                instance.DownloadFrame = otherSettings.DownloadFrame;                // Загрузка Frame
                instance.UseCSS = otherSettings.UseCSS;                              // Загрузка CSS
                instance.UseJavaApplets = otherSettings.UseJavaApplets;              // Использование JavaApplets
                instance.UseJavaScripts = otherSettings.UseJavaScripts;              // Использование JavaScript
                instance.UsePlugins = otherSettings.UsePlugins;                      // Использование плагинов
                instance.UsePluginsForceWmode = otherSettings.UsePluginsForceWmode;  // Использование плагинов Wmode (загрузка плагинов в том-же окне)
                instance.UseMedia = otherSettings.UseMedia;                          // Использование Media
                instance.UseAdds = otherSettings.UseAdds;                            // Реклама
                instance.DownloadVideos = otherSettings.DownloadVideos;              // Загрузка видео
                instance.AllowPopUp = otherSettings.AllowPopUp;                      // Разрешить всплывающее окно
                instance.BackGroundSoundsPlay = otherSettings.BackGroundSoundsPlay;  // Воспроизведение фоновых звуков
            }

            public static void SetDefaultOtherSettings()
            {
                var instance = ServicesComponents.Instance;

                instance.LoadPictures = true;         // Загрузка картинок
                instance.DownloadActiveX = true;      // Загрузка ActiveX
                instance.RunActiveX = true;           // Запуск ActiveX
                instance.DownloadFrame = true;        // Загрузка Frame
                instance.UseCSS = true;               // Загрузка CSS
                instance.UseJavaApplets = true;       // Использование JavaApplets
                instance.UseJavaScripts = true;       // Использование JavaScript
                instance.UsePlugins = true;           // Использование плагинов
                instance.UsePluginsForceWmode = true; // Использование плагинов Wmode (загрузка плагинов в том-же окне)
                instance.UseMedia = true;             // Использование Media
                instance.UseAdds = true;              // Реклама
                instance.DownloadVideos = true;       // Загрузка видео
                instance.AllowPopUp = true;           // Разрешить всплывающее окно
                instance.BackGroundSoundsPlay = true; // Воспроизведение фоновых звуков
            }
        }

        public class BusySettings
        {
            public bool IgnoreAdditionalRequests { get; set; }
            public bool IgnoreAjaxRequests { get; set; }
            public bool IgnoreFrameRequests { get; set; }
            public bool IgnoreFlashRequests { get; set; }

            public BusySettings()
            {

            }

            public static BusySettings ExtractBusySettingsFromVariable(string variable)
            {
                var busySettings = new BusySettings
                {
                    IgnoreAdditionalRequests = variable.Contains("Игнорировать Post/Get-запросы"),
                    IgnoreAjaxRequests = variable.Contains("Игнорировать Ajax-запросы"),
                    IgnoreFrameRequests = variable.Contains("Игнорировать Frame-запросы"),
                    IgnoreFlashRequests = variable.Contains("Игнорировать Flash-запросы")
                };

                return busySettings;
            }

            public static BusySettings GetCurrentBusySettings()
            {
                var instance = ServicesComponents.Instance;

                var instanceSettings = new BusySettings
                {
                    IgnoreAdditionalRequests = instance.IgnoreAdditionalRequests,
                    IgnoreAjaxRequests = instance.IgnoreAjaxRequests,
                    IgnoreFrameRequests = instance.IgnoreFrameRequests,
                    IgnoreFlashRequests = instance.IgnoreFlashRequests
                };

                return instanceSettings;
            }

            public static void SetBusySettings(BusySettings busySettings)
            {
                var instance = ServicesComponents.Instance;

                instance.IgnoreAdditionalRequests = busySettings.IgnoreAdditionalRequests;
                instance.IgnoreAjaxRequests = busySettings.IgnoreAjaxRequests;
                instance.IgnoreFrameRequests = busySettings.IgnoreFrameRequests;
                instance.IgnoreFlashRequests = busySettings.IgnoreFlashRequests;
            }

            public static void SetDefaultBusySettings()
            {
                var instance = ServicesComponents.Instance;

                instance.IgnoreAdditionalRequests = false;  // Игнорировать Post/Get-запросы
                instance.IgnoreAjaxRequests = false;        // Игнорировать Ajax-запросы
                instance.IgnoreFrameRequests = false;       // Игнорировать Frame-запросы
                instance.IgnoreFlashRequests = true;        // Игнорировать Flash-запросы
            }
        }
    }
}
