using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Services.PostingSecondWindService.Enums;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace Yandex.Zen.Core.Services.PostingSecondWindService.Models
{
    public class PostingSecondWindSettings
    {
        public PostingSecondWindModeEnum Mode { get; private set; }

        public PostingSecondWindSettings(PostingSecondWindModeEnum postingSecondWindMode)
        {
            Mode = postingSecondWindMode;
        }
    }
}
