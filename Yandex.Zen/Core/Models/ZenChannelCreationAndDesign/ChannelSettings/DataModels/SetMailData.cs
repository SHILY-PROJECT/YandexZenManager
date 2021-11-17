using Yandex.Zen.Core.Tools.LoggerTool.Models;

namespace Yandex.Zen.Core.Models.ZenChannelCreationAndDesign.ChannelSettings.DataModels
{
    public class SetMailData
    {
        public bool ActionsStatus { get; set; }
        public TimeData TimeAction { get; set; }
        public string Mail { get; set; }
    }
}