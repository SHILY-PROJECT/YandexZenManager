using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yandex.Zen.Core.Models.AccountOrDonorModels
{
    public class ChannelDataModel
    {
        public Uri Url { get; set; }
        public string ProfileEditor { get; set; }
        public string Description { get; set; }
        public FileInfo Avatar { get; set; }
    }
}
