﻿using Yandex.Zen.Core.Models.Logger;

namespace Yandex.Zen.Core.Models.ZenChannelCreationAndDesign.ChannelSettings.DataModels
{
    public class ChangeChannelDescriptionData
    {
        public bool ActionsStatus { get; set; }
        public TimeData TimeAction { get; set; }
        public string ChannelDescription { get; set; }
    }
}