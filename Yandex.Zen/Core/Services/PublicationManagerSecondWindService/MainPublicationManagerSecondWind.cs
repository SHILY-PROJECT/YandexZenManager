using System;
using Yandex.Zen.Core.Models.ResourceModels;
using Yandex.Zen.Core.Services.PublicationManagerSecondWindService.Enums;
using Yandex.Zen.Core.Services.CommonComponents;

namespace Yandex.Zen.Core.Services.PublicationManagerSecondWindService
{
    public class MainPublicationManagerSecondWind
    {
        private static readonly object _locker = new object();

        [ThreadStatic] private static PostingSecondWindModeEnum _currentMode;
        [ThreadStatic] private static bool _currentModeSetted;

        private ResourceBaseModel Account { get => DataManager.Data.Resource ?? null; }

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
