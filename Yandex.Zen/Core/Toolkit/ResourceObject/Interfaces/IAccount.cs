using Yandex.Zen.Core.Toolkit.ResourceObject.Models;

namespace Yandex.Zen.Core.Toolkit.ResourceObject.Interfaces
{
    public interface IAccount : IResourceObject
    {
        IProfile Profile { get; set; }
        TemplateSettingsModel Settings { get; set; }
        ChannelModel Channel { get; set; }
        string Login { get; set; }
        string Password { get; set; }
        string AnswerQuestion { get; set; }
        string PhoneNumber { get; set; }
        string CurrentMessageInTable { get; set; }
        string WebSite { get; set; }
    }
}
