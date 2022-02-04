namespace Yandex.Zen.Core.Toolkit.ObjectModule.Models
{
    public class TemplateSettingsModel
    {
        public bool CreateFolderResourceIfNoExist { get; set; }

        /// <summary>
        /// Использовать нагуленные профиля.
        /// </summary>
        public bool UseWalkedProfileFromSharedFolder { get; set; }

        /// <summary>
        /// Минимальный размер нагуленного профиля.
        /// </summary>
        public int MinProfileSizeToUse { get; set; }

    }
}
