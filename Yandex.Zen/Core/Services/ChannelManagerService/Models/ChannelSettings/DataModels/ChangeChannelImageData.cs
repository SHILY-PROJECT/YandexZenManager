using Yandex.Zen.Core.Toolkit.LoggerTool.Models;

namespace Yandex.Zen.Core.Services.ChannelManagerService.Models.ChannelSettings.DataModels
{
    public class ChangeChannelImageData
    {
        public bool ActionsStatus { get; set; }
        public TimeData TimeAction { get; set; }
        public string ImageFile { get; set; }
    }
}