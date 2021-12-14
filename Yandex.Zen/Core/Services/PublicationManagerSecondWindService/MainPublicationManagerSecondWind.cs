using System;
using Yandex.Zen.Core.Models.ResourceModels;
using Yandex.Zen.Core.Services.PublicationManagerSecondWindService.Enums;
using Yandex.Zen.Core.Services.CommonComponents;

namespace Yandex.Zen.Core.Services.PublicationManagerSecondWindService
{
    public class MainPublicationManagerSecondWind
    {
        private static readonly object _locker = new object();

        [ThreadStatic] private static PublicationManagerSecondWindModeEnum _currentMode;
        [ThreadStatic] private static bool _currentModeSetted;

        private ResourceBaseModel Account { get => DataManager.Data.Resource; }

        /// <summary>
        /// Текущий режим работы сервиса.
        /// </summary>
        public static PublicationManagerSecondWindModeEnum CurrentMode
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
            if (_currentModeSetted is false) throw new Exception($"The current operating mode is not set");

            switch (CurrentMode)
            {
                case PublicationManagerSecondWindModeEnum.AuthAndBindingPhone: AuthAndBindingPhone(); break;
                case PublicationManagerSecondWindModeEnum.Posting: break;
            }
        }

        private void AuthAndBindingPhone()
        {
            AuthorizationNew.AuthNew(out var status);


        }
    }
}
