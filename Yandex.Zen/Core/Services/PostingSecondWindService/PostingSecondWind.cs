using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Models.ResourceModels;
using Yandex.Zen.Core.Services.PostingSecondWindService.Enums;
using Yandex.Zen.Core.Services.PostingSecondWindService.Models;
using Yandex.Zen.Core.Services.Components;

namespace Yandex.Zen.Core.Services.PostingSecondWindService
{
    public class PostingSecondWind
    {
        private static readonly object _locker = new object();
        [ThreadStatic] private static PostingSecondWindSettings _settings;
        [ThreadStatic] private static PostingSecondWindModeEnum _currentMode;
        [ThreadStatic] private static bool _currentModeSetted;

        private ResourceBaseModel Account { get => ProjectComponents.Project.ResourceObject ?? null; }
        //public static PostingSecondWindSettings Settings { get => _settings; }
        //public static void SetSettings(PostingSecondWindSettings Settings) => _settings = Settings;
        /// <summary>
        /// Текущий режим работы сервиса.
        /// </summary>
        public static PostingSecondWindModeEnum CurrentMode
        {
            get => _currentMode;
            set
            {
                _currentMode = value;
                _currentModeSetted = true;
            }
        }

        public void Start()
        {
            if (Account is null) throw new Exception($"Account object is null");
            if (_currentModeSetted is false) throw new Exception($"The current operating mode is not set");

            switch (CurrentMode)
            {
                case PostingSecondWindModeEnum.AuthorizationAndLinkPhone:
                    AuthorizationAndLinkPhone();
                    break;

                case PostingSecondWindModeEnum.Posting:

                    break;
            }
        }

        private void AuthorizationAndLinkPhone()
        {
            AuthorizationNew.AuthNew(out var status);


        }
    }
}
