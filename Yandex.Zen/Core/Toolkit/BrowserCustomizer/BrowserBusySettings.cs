using ZennoLab.CommandCenter;
using Yandex.Zen.Core.Toolkit.BrowserCustomizer.Models;

namespace Yandex.Zen.Core.Toolkit.BrowserCustomizer
{
    public static class BrowserBusySettings
    {
        public static BrowserBusySettingsModel ExtractBusySettingsFromVariable(string variable)
            => new BrowserBusySettingsModel
            {
                IgnoreAdditionalRequests = variable.Contains("Игнорировать Post/Get-запросы"),
                IgnoreAjaxRequests = variable.Contains("Игнорировать Ajax-запросы"),
                IgnoreFrameRequests = variable.Contains("Игнорировать Frame-запросы"),
                IgnoreFlashRequests = variable.Contains("Игнорировать Flash-запросы")
            };

        public static BrowserBusySettingsModel BrowserGetCurrentBusySettings(this Instance browser)
            => new BrowserBusySettingsModel
            {
                IgnoreAdditionalRequests = browser.IgnoreAdditionalRequests,
                IgnoreAjaxRequests = browser.IgnoreAjaxRequests,
                IgnoreFrameRequests = browser.IgnoreFrameRequests,
                IgnoreFlashRequests = browser.IgnoreFlashRequests
            };

        public static void BrowserSetBusySettings(this Instance browser, BrowserBusySettingsModel busySettings)
        {
            browser.IgnoreAdditionalRequests = busySettings.IgnoreAdditionalRequests;
            browser.IgnoreAjaxRequests = busySettings.IgnoreAjaxRequests;
            browser.IgnoreFrameRequests = busySettings.IgnoreFrameRequests;
            browser.IgnoreFlashRequests = busySettings.IgnoreFlashRequests;
        }

        public static void BrowserSetDefaultBusySettings(this Instance browser)
        {
            browser.IgnoreAdditionalRequests = false;  // Игнорировать Post/Get-запросы
            browser.IgnoreAjaxRequests = false;        // Игнорировать Ajax-запросы
            browser.IgnoreFrameRequests = false;       // Игнорировать Frame-запросы
            browser.IgnoreFlashRequests = true;        // Игнорировать Flash-запросы
        }
    }
}
