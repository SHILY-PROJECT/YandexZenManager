using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Yandex.Zen.Core.Enums;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.Enums.Http;
using ZennoLab.InterfacesLibrary.Enums.Log;

namespace Yandex.Zen.Core.Models
{
    public class ProxyInfo
    {
        public string CountryShortName { get; set; }
        public string CountryFullName { get; set; }
        public string Proxy { get; set; }

        public ProxyInfo()
        {
            Array.ForEach(new[] { Proxy, CountryShortName, CountryFullName }, x => x = "none");
        }

        public ProxyInfo Create(string proxy)
        {
            if (string.IsNullOrWhiteSpace(proxy) || !proxy.Contains(":"))
            {

                return null;
            }

            var proxyInfo = GetProxyInfo(proxy);

            CountryShortName = proxyInfo.CountryShortName;
            CountryFullName = proxyInfo.CountryFullName;

            return proxyInfo;
        }

        public static ProxyInfo GetProxyInfo(string ip)
        {
            var proxyInfo = new ProxyInfo();

            ip = Regex.Match(ip, @"([0-9]{1,3}[\.]){3}[0-9]{1,3}").Value;

            if (string.IsNullOrWhiteSpace(ip))
            {
                Logger.Write($"Не найден IP", LoggerType.Warning);
                return null;
            }

            try
            {
                var httpResponse = ZennoPoster.HTTP.Request
                (
                    HttpMethod.GET, $"https://ipinfo.io/{ip}", "", "", "", "UTF-8",
                    ResponceType.BodyOnly, 20000, "", ServicesDataAndComponents_obsolete.Zenno.Profile.UserAgent, true, 5
                );

                proxyInfo.CountryShortName = Regex.Replace(ZennoPoster.Parser.ParseByXpath(httpResponse, "//a[contains(@href, '/countries/')]", "href").First(), @"/countries/.*?", "");
                proxyInfo.CountryFullName = ZennoPoster.Parser.ParseByXpath(httpResponse, "//a[contains(@href, '/countries/')]", "innerhtml").First();
            }
            catch (Exception ex)
            {
                Logger.Write($"[Exception message: {ex.Message}]\tУпало исключение во время определения IP страны", LoggerType.Warning, true, true, true, LogColor.Red);
                return null;
            }

            return proxyInfo;
        }

    }
}
