namespace Yandex.Zen.Core.Toolkit.BrowserCustomizer.Models
{
    public class BrowserOtherSettingsModel
    {
        //Добавить в выводные настройки
        //Настройки инстанса:{Загрузка картинок|Загрузка ActiveX|Запуск ActiveX|Загрузка Frame|Загрузка CSS|Использование JavaApplets|Использование JavaScript|Использование плагинов|Использование плагинов Wmode (загрузка плагинов в том-же окне)|Использование Media|Реклама|Загрузка видео|Разрешить всплывающее окно|Воспроизведение фоновых звуков}

        /// <summary>
        /// Загрузка картинок.
        /// </summary>
        public bool LoadPictures { get; set; }
        /// <summary>
        /// Загрузка ActiveX.
        /// </summary>
        public bool DownloadActiveX { get; set; }
        /// <summary>
        /// Запуск ActiveX.
        /// </summary>
        public bool RunActiveX { get; set; }
        /// <summary>
        /// Загрузка Frame.
        /// </summary>
        public bool DownloadFrame { get; set; }
        /// <summary>
        /// Загрузка CSS.
        /// </summary>
        public bool UseCSS { get; set; }
        /// <summary>
        /// Использование JavaApplets.
        /// </summary>
        public bool UseJavaApplets { get; set; }
        /// <summary>
        /// Использование JavaScript.
        /// </summary>
        public bool UseJavaScripts { get; set; }
        /// <summary>
        /// Использование плагинов.
        /// </summary>
        public bool UsePlugins { get; set; }
        /// <summary>
        /// Использование плагинов Wmode (загрузка плагинов в том-же окне).
        /// </summary>
        public bool UsePluginsForceWmode { get; set; }
        /// <summary>
        /// Использование Media.
        /// </summary>
        public bool UseMedia { get; set; }
        /// <summary>
        /// Реклама.
        /// </summary>
        public bool UseAdds { get; set; }
        /// <summary>
        /// Загрузка видео.
        /// </summary>
        public bool DownloadVideos { get; set; }
        /// <summary>
        /// Разрешить всплывающее окно.
        /// </summary>
        public bool AllowPopUp { get; set; }
        /// <summary>
        /// Воспроизведение фоновых звуков.
        /// </summary>
        public bool BackGroundSoundsPlay { get; set; }
    }
}
