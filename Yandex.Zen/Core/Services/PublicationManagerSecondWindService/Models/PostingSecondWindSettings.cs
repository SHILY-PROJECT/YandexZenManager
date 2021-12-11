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
        public PublicationManagerSecondWindModeEnum Mode { get; private set; }

        public PostingSecondWindSettings(PublicationManagerSecondWindModeEnum postingSecondWindMode)
        {
            Mode = postingSecondWindMode;
        }
    }
}
