using System;
using System.IO;

namespace Yandex.Zen.Core.ServiceModules.ObjectModule.Models
{
    public class ChannelDataModel
    {
        public Uri Url { get; set; }
        public Uri ProfileEditor { get; set; }
        public string NumberPhone { get; set; }
        public string Description { get; set; }
        public FileInfo Avatar { get; set; }
    }
}
