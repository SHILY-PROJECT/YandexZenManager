using Yandex.Zen.Core.Toolkit.LoggerTool.Models;

namespace Yandex.Zen.Core.Services.ChannelHandlerService.Models.ChannelSettings.DataModels
{
    public class BindingPhoneToChannelData
    {
        public bool ActionsStatus { get; set; }
        public TimeData TimeAction { get; set; }
        public string Phone { get; set; }
        public string ServiceDll { get; set; }
        public string JobId { get; set; }
    }
}