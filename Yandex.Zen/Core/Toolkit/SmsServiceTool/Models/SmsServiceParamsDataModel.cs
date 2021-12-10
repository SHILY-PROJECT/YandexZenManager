using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Yandex.Zen.Core.Toolkit.SmsServiceTool.Models
{
    public class SmsServiceParamsDataModel
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
                if (Regex.IsMatch(value, @"[0-9a-zA-Z]+\.dll\ -\ [0-9A-Za-zА-Яа-я]+") is false)
                    throw new Exception("Sms service is not formatted correctly");
                SetOtherParams(value);
            }
        }

        /// <summary>
        /// Модель с параметрами для сервиса.
        /// </summary>
        /// <param name="serviceDllAndCountry">
        /// Название dll сервиса и страны, через разделитель (" - ").
        /// Пример: SmsActivate.dll - Россия
        /// </param>
        public SmsServiceParamsDataModel(string serviceDllAndCountry)
            => ServiceDllAndCountry = serviceDllAndCountry;

        /// <summary>
        /// Получение и установка параметров для получения номера.
        /// </summary>
        /// <param name="serviceDllAndCountry"></param>
        private void SetOtherParams(string serviceDllAndCountry)
        {
            var service = serviceDllAndCountry.Split(new[] { " - " }, StringSplitOptions.None)[0];
            var country = CountryName = serviceDllAndCountry.Split(new[] { " - " }, StringSplitOptions.None)[1];

            switch (service)
            {
                case "FiveSimSms.dll":
                    Dll = service;
                    NetworkService = "yandex";
                    Country = new Dictionary<string, string>
                    {
                        ["Россия"] =            "&country=russia",
                        ["USA"] =               "&country=usa",
                        ["Украина"] =           "&country=ukraine",
                        ["Канада"] =            "&country=canada",
                        ["Великобритания"] =    "&country=england"
                    }
                    [country];
                    break;

                case "SmsActivate.dll":
                    Dll = service;
                    NetworkService = "ya";
                    Country = new Dictionary<string, string>
                    {
                        ["Россия"] =            "&country=0",
                        ["USA"] =               "&country=187",
                        ["USA (виртуальные)"] = "&country=12"
                    }
                    [country];
                    break;
            }
        }
    }
}
