using Yandex.Zen.Core.Models.Logger;

namespace Yandex.Zen.Core.Models.ZenChannelCreationAndDesign.ChannelSettings.DataModels
{
    public class SetMailData
    {
        public bool ActionsStatus { get; set; }
        public TimeData TimeAction { get; set; }
        public string Mail { get; set; }
    }
}