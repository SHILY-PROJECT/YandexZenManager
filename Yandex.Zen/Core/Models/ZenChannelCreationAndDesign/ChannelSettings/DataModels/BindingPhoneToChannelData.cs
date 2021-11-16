using Yandex.Zen.Core.Models.Logger;

namespace Yandex.Zen.Core.Models.ZenChannelCreationAndDesign.ChannelSettings.DataModels
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