using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.Enums.Http;
using ZennoLab.InterfacesLibrary.Enums.Log;

namespace Yandex.Zen.Core.Toolkit.ResourceObject.Models
{
    /// <summary>
    /// Класс для хранения данных IP.
    /// </summary>
    public partial class ProxyModel
    {
        public string CountryFullName { get; set; }
        public string CountryShortName { get; set; }
        public string Proxy { get; set; }

        private string UserAgent { get; set; }

        public ProxyModel(DataManager manager, string proxy, bool defineIpCountryInfo)
        {
            UserAgent = manager.Zenno.Profile.UserAgent;
            Configure(proxy, defineIpCountryInfo);
        }

        /// <summary>
        /// Конфигурирование данных IP (установка и валидация IP; определение страны, если нужно).
        /// </summary>
        public void Configure(string proxy, bool defineIpCountryInfo)
        {
            if (proxy.Equals("none", StringComparison.OrdinalIgnoreCase) || proxy.Equals("-", StringComparison.OrdinalIgnoreCase))
            {
                Proxy = string.Empty;
                return;
            }

            if (string.IsNullOrWhiteSpace(proxy) || proxy.Contains(":") is false)
                throw new ArgumentException($"'{nameof(proxy)}:{proxy}' - Некорректный аргумент");

            if (defineIpCountryInfo)
            {
                DefineIpCountry(proxy, out var countryFullName, out var countryShortName);
                CountryFullName = countryFullName;
                CountryShortName = countryShortName;
            }

            Proxy = proxy;
        }

        /// <summary>
        /// Определение страны IP.
        /// </summary>
        public void DefineIpCountry(string ip, out string countryFullName, out string countryShortName)
        {
            countryFullName = null;
            countryShortName = null;

            ip = Regex.Match(ip, @"([0-9]{1,3}[\.]){3}[0-9]{1,3}").Value;

            if (string.IsNullOrWhiteSpace(ip))
            {
                Logger.Write($"Не найден IP", LoggerType.Warning);
                return;
            }

            try
            {
                var httpResponse = ZennoPoster.HTTP.Request
                (
                    HttpMethod.GET, $"https://ipinfo.io/{ip}", "", "", "", "UTF-8",
                    ResponceType.BodyOnly, 20000, "", UserAgent, true, 5
                );

                countryFullName = ZennoPoster.Parser.ParseByXpath(httpResponse, "//a[contains(@href, '/countries/')]", "innerhtml").First();
                countryShortName = Regex.Replace(ZennoPoster.Parser.ParseByXpath(httpResponse, "//a[contains(@href, '/countries/')]", "href").First(), @"/countries/.*?", "");
            }
            catch (Exception ex)
            {
                Logger.Write(ex.FormatException("Упало исключение во время определения IP страны"), LoggerType.Warning, true, true, true, LogColor.Red);
                return;
            }

            return;
        }
    }
}
