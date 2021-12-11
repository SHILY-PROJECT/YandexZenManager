using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Services.PublicationManagerSecondWindService.Enums;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace Yandex.Zen.Core.Services.PublicationManagerSecondWindService.Models
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
