using Yandex.Zen.Core.Toolkit.LoggerTool.Models;

namespace Yandex.Zen.Core.Models.ZenChannelCreationAndDesign.ChannelSettings.DataModels
{
    public class ChangeChannelDescriptionData
    {
        public bool ActionsStatus { get; set; }
        public TimeData TimeAction { get; set; }
        public string ChannelDescription { get; set; }
    }
}