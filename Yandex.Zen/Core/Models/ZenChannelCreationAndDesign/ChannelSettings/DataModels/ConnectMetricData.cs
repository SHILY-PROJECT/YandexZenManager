﻿using Yandex.Zen.Core.Models.Logger;

namespace Yandex.Zen.Core.Models.ZenChannelCreationAndDesign.ChannelSettings.DataModels
{
    public class ConnectMetricData
    {
        public bool ActionsStatus { get; set; }
        public TimeData TimeAction { get; set; }
        public string CounterId { get; set; }
    }
}