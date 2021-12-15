﻿using System;
using Yandex.Zen.Core.Models.ResourceModels;
using Yandex.Zen.Core.Services.PublicationManagerSecondWindService.Enums;
using Yandex.Zen.Core.Services.CommonComponents;
using ZennoLab.CommandCenter;
using Yandex.Zen.Core.Toolkit.BrowserCustomizer;
using Yandex.Zen.Core.Toolkit.BrowserCustomizer.Models;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using System.Collections.Generic;
using ZennoLab.InterfacesLibrary.Enums.Log;

namespace Yandex.Zen.Core.Services.PublicationManagerSecondWindService
{
    public class MainPublicationManagerSecondWind
    {
        #region [ВНЕШНИЕ РЕСУРСЫ]===================================================
        private DataManager Data { get => DataManager.Data; }
        private ResourceBaseModel Account { get => Data.Resource; }
        private Instance Browser { get => Data.Browser; }

        #endregion =================================================================

        private static readonly object _locker = new object();

        [ThreadStatic] private static PublicationManagerSecondWindModeEnum _currentMode;
        [ThreadStatic] private static bool _currentModeSetted;
        [ThreadStatic] private bool _statusBindPhoneNumberToZenChannel;

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

        /// <summary>
        /// Старт скрипта.
        /// </summary>
        public void Start()
        {
            if (_currentModeSetted is false) throw new Exception($"The current operating mode is not set");

            switch (CurrentMode)
            {
                case PublicationManagerSecondWindModeEnum.AuthAndBindingPhone:
                    AuthAndBindingPhone();
                    break;

                case PublicationManagerSecondWindModeEnum.Posting:
                    break;
            }
        }

        /// <summary>
        /// Авторизация и привязка номера.
        /// </summary>
        private void AuthAndBindingPhone()
        {
            AuthorizationNew.AuthNew(out var isSuccessful);
            if (!isSuccessful) return;
            BindPhoneNumberToZenChannel(out _);
        }

        private void BindPhoneNumberToZenChannel(out bool isSuccessful)
        {
            isSuccessful = false;

            HE xButtonSettings = new HE("//div[contains(@class, 'navbar')]/descendant::button[contains(@class, 'nav-item-content')]", "Настройки");
            //HE x = new HE("", "");
            //HE x = new HE("", "");
            //HE x = new HE("", "");
            //HE x = new HE("", "");
            //HE x = new HE("", "");
            var counterAttempts = 0;

            while (true)
            {
                if (++counterAttempts > 3)
                {
                    Logger.Write("Слишком много ошибок в время привязки номера к каналу", LoggerType.Warning, true, true, true, LogColor.Yellow);
                    Logger.ErrorAnalysis(true, true, true, new List<string> { Browser.ActiveTab.URL });
                    isSuccessful = _statusBindPhoneNumberToZenChannel = false;
                    return;
                }

                Browser.ActiveTab.Navigate("https://zen.yandex.ru/media/zen/new", "https://zen.yandex.ru/", true);

            }

        }
    }
}
