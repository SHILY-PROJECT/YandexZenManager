using System;
using System.Linq;
using System.Text.RegularExpressions;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.Enums.Http;
using Yandex.Zen.Core.Toolkit.LoggerTool;

namespace Yandex.Zen.Core.ServicesComponents.ResourceObject.Models
{
    public class ProxyModel
    {
        private readonly DataManager _manager;

        public ProxyModel(DataManager manager)
        {
            _manager = manager;
        }

        public ProxyModel(DataManager manager, string proxy, bool defineCountry = false)
        {
            _manager = manager;
            TrySetProxy(proxy, defineCountry);
        }

        public bool TrySetProxy(string proxy, bool defineCountry = false)
        {
            if (string.IsNullOrWhiteSpace(proxy) || proxy.Contains(":") is false)
            {
                ErrorMessage = $"'{nameof(proxy)}:{proxy}' - Invalid proxy.";
                return (IsValid = false);
            }
            else Proxy = proxy;

            if (string.IsNullOrWhiteSpace(IP = Regex.Match(Proxy, @"[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}").Value))
            {
                ErrorMessage = $"'{nameof(proxy)}:{proxy}' - Invalid IP.";
                return (IsValid = false);
            }

            if (defineCountry is false) return (IsValid = true);

            try
            {
                DefineCountry(_manager.Zenno.Profile.UserAgent);
                return (IsValid = true);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"{LogFormattingHandler.FormatException(ex)}";
                return (IsValid = false);
            }
        }

        public string CountryShortName { get; set; }
        public string CountryFullName { get; set; }
        public string Proxy { get; set; }
        public string IP { get; set; }
        public bool IsValid { get; set; } = true;
        public string ErrorMessage { get; set; }

        public void DefineCountry(string userAgent)
        {
            var httpResponse = ZennoPoster.HTTP.Request
            (
                HttpMethod.GET, $"https://ipinfo.io/{IP}", "", "", "", "UTF-8",
                ResponceType.BodyOnly, 20000, "", userAgent, true, 5
            );

            CountryShortName = Regex.Replace(ZennoPoster.Parser.ParseByXpath(httpResponse, "//a[contains(@href, '/countries/')]", "href").FirstOrDefault(), @"/countries/.*?", "");
            CountryFullName = ZennoPoster.Parser.ParseByXpath(httpResponse, "//a[contains(@href, '/countries/')]", "innerhtml").FirstOrDefault();
        }
    }
}
