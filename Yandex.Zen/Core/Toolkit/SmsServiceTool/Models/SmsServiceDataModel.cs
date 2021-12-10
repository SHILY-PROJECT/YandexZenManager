namespace Yandex.Zen.Core.Toolkit.SmsServiceTool.Models
{
    /// <summary>
    /// Хранилище данных полученных от SMS сервиса (автоматически получает и сохраняет все данные).
    /// </summary>
    public class SmsServiceDataModel
    {
        public string JobID { get; set; }
        public string SmsCodeOrStatus { get; set; }
        public string NumberPhone { get; set; }
        public string NumberPhoneForServiceView { get; set; }
    }
}
