using System;
using System.IO;

namespace Yandex.Zen.Core.Toolkit.ObjectModule.Models
{
    public class ChannelModel
    {
        public Uri Url { get; set; }
        public Uri ProfileEditor { get; set; }
        public string IndexationAndBanStatus { get; set; }
        public string NumberPhone { get; set; }
        public string Description { get; set; }
        public FileInfo Avatar { get; set; }
    }
}
