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

namespace Yandex.Zen.Core.Models.ResourceModels
{
    public class ProxyDataModel
    {
        private string _countryFullName;
        private string _countryShortName;
        private string _proxy;

        public string CountryFullName { get => _countryFullName; }
        public string CountryShortName { get => _countryShortName; }
        public string Proxy { get => _proxy; }

        public ProxyDataModel(string proxy, bool defineIpCountryInfo)
        {
            if (proxy.Equals("none", StringComparison.OrdinalIgnoreCase) || proxy.Equals("-", StringComparison.OrdinalIgnoreCase))
            {
                _proxy = string.Empty;
                return;
            }

            if (string.IsNullOrWhiteSpace(proxy) || proxy.Contains(":") is false)
                throw new ArgumentException($"'{nameof(proxy)}' - Некорректный аргумент");

            if (defineIpCountryInfo)
            {
                GetIpCountryInfo(proxy, out var countryFullName, out var countryShortName);
                _countryFullName = countryFullName;
                _countryShortName = countryShortName;
            }

            _proxy = proxy;
        }

        public void Configure(string proxy, bool defineIpCountryInfo)
        {
            if (proxy.Equals("none", StringComparison.OrdinalIgnoreCase) || proxy.Equals("-", StringComparison.OrdinalIgnoreCase))
            {
                _proxy = string.Empty;
                return;
            }

            if (string.IsNullOrWhiteSpace(proxy) || proxy.Contains(":") is false)
                throw new ArgumentException($"'{nameof(proxy)}' - Некорректный аргумент");

            if (defineIpCountryInfo)
            {
                GetIpCountryInfo(proxy, out var countryFullName, out var countryShortName);
                _countryFullName = countryFullName;
                _countryShortName = countryShortName;
            }

            _proxy = proxy;
        }

        public void GetIpCountryInfo(string ip, out string countryFullName, out string countryShortName)
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
                    ResponceType.BodyOnly, 20000, "", ServicesDataAndComponents.Zenno.Profile.UserAgent, true, 5
                );

                countryFullName = ZennoPoster.Parser.ParseByXpath(httpResponse, "//a[contains(@href, '/countries/')]", "innerhtml").First();
                countryShortName = Regex.Replace(ZennoPoster.Parser.ParseByXpath(httpResponse, "//a[contains(@href, '/countries/')]", "href").First(), @"/countries/.*?", "");
            }
            catch (Exception ex)
            {
                Logger.Write($"[Exception message: {ex.Message}]\tУпало исключение во время определения IP страны", LoggerType.Warning, true, true, true, LogColor.Red);
                return;
            }

            return;
        }
    }
}
