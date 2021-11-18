using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yandex.Zen.Core.Tools.PhoneServiceTool.Models
{
    public class PhoneServiceParamsModel
    {
        private string _serviceDllAndCountry;

        /// <summary>
        /// Название сервиса dll.
        /// </summary>
        public string Dll { get; private set; }

        /// <summary>
        /// Параметр сервиса для которого нужен номер.
        /// Например: ya, vk, ok, tw и т.д.
        /// </summary>
        public string NetworkService { get; private set; }

        /// <summary>
        /// Параметр страны для сервиса.
        /// </summary>
        public string Country { get; private set; }

        /// <summary>
        /// Название страны (из выводных настроек).
        /// </summary>
        public string CountryName { get; private set; }

        /// <summary>
        /// Название dll сервиса и страны, через разделитель (" - ").
        /// Пример: SmsActivate.dll - Россия
        /// </summary>
        public string ServiceDllAndCountry
        {
            get => _serviceDllAndCountry;
            private set
            {
                _serviceDllAndCountry = value;
                SetOtherParams(_serviceDllAndCountry);
            }
        }

        /// <summary>
        /// Модель с параметрами для сервиса.
        /// </summary>
        /// <param name="serviceDllAndCountry">
        /// Название dll сервиса и страны, через разделитель (" - ").
        /// Пример: SmsActivate.dll - Россия
        /// </param>
        public PhoneServiceParamsModel(string serviceDllAndCountry)
            => ServiceDllAndCountry = serviceDllAndCountry;

        private void SetOtherParams(string serviceDllAndCountry)
        {
            var service = serviceDllAndCountry.Split(new[] { " - " }, StringSplitOptions.None)[0];
            var country = serviceDllAndCountry.Split(new[] { " - " }, StringSplitOptions.None)[1];

            switch (service)
            {
                case "FiveSimSms.dll":
                    Dll = service;
                    NetworkService = "yandex";
                    Country = new Dictionary<string, string>
                    {
                        ["Россия"] = "&country=russia",
                        ["USA"] = "&country=usa",
                        ["Украина"] = "&country=ukraine",
                        ["Канада"] = "&country=canada",
                        ["Великобритания"] = "&country=england"
                    }
                    [country];
                    break;

                case "SmsActivate.dll":
                    Dll = service;
                    NetworkService = "ya";
                    Country = new Dictionary<string, string>
                    {
                        ["Россия"] = "&country=0",
                        ["USA"] = "&country=187",
                        ["USA (виртуальные)"] = "&country=12"
                    }
                    [country];
                    break;
            }
        }
    }
}
