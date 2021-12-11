using System.IO;
using System.Text;
using Global.ZennoLab.Json;
using Yandex.Zen.Core.Services.ChannelManagerService.Models.ChannelSettings.DataModels;
using Yandex.Zen.Core.Services.ChannelManagerService;

namespace Yandex.Zen.Core.Services.ChannelManagerService.Models.ChannelSettings
{
    public class SettingsBaseModel
    {
        public BindingPhoneToChannelData BindingPhoneToChannel { get; set; }
        public ChangeChannelImageData ChangeChannelImage { get; set; }
        public ChangeChannelNameData ChangeChannelName { get; set; }
        public ChangeChannelDescriptionData ChangeChannelDescription { get; set; }
        public AddUrlToSocialNetworkData AddUrlToSocialNetwork { get; set; }
        public EnablePrivateMessagesData EnablePrivateMessages { get; set; }
        public SetMailData SetMail { get; set; }
        public AgreeToReceiveZenNewsletterData AgreeToReceiveZenNewsletter { get; set; }
        public SetSiteData SetSite { get; set; }
        public ConnectMetricData ConnectMetric { get; set; }
        public AcceptTermsOfUserAgreementData AcceptTermsOfUserAgreement { get; set; }

        public static SettingsBaseModel GetCurrentSettings()
        {
            var settingsFile = MainChannelManager.SettingsFile;

            if (settingsFile.Exists)
            {
                // Получение настроек из файла
                return JsonConvert.DeserializeObject<SettingsBaseModel>(File.ReadAllText(settingsFile.FullName, Encoding.UTF8));
            }
            else
            {
                // Создание нового экземпляра настроек
                var settings = new SettingsBaseModel();

                // Сохранение настроек в файл
                File.WriteAllText(settingsFile.FullName, JsonConvert.SerializeObject(settings, Formatting.Indented), Encoding.UTF8);

                return settings;
            }
        }

        public static SettingsBaseModel ExtractSettingsFromTemplateVariable(string templateVariableWithSettings)
        {
            var settings = new SettingsBaseModel
            {

            };

            return settings;
        }
    }
}
