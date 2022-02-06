using System.IO;

namespace Yandex.Zen.Core.ServicesComponents.ResourceObject.Models
{
    public class ChannelModel
    {
        public string Url { get; set; }
        public string ProfileEditorUrl { get; set; }
        public string IndexationAndBanStatus { get; set; }
        public string NumberPhone { get; set; }
        public string Description { get; set; }
        public FileInfo Avatar { get; set; }
    }
}
