﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Yandex.Zen.Core.Enums;
using Yandex.Zen.Core.Enums.Extensions;
using Yandex.Zen.Core.Enums.Logger;
using Yandex.Zen.Core.Models;
using Yandex.Zen.Core.Tools;
using Yandex.Zen.Core.Tools.Extensions;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.Enums.Http;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace Yandex.Zen.Core.ServicesCommonComponents
{
    public class ServiceComponents
    {
        public static IZennoTable AccountsGeneralTable;
        public static Random rnd;

        public static Instance instance { get => Program.Instance; }
        public static IZennoPosterProjectModel zenno { get => Program.Zenno; }
        //[ThreadStatic] public static Instance instance;
        //[ThreadStatic] public static IZennoPosterProjectModel zenno;

        /// <summary>
        /// Таблица режима.
        /// </summary>
        [ThreadStatic] public static IZennoTable AccountsTable;
        [ThreadStatic] public static FileInfo TableGeneralAccountFile;
        [ThreadStatic] public static FileInfo TableModeAccountFile;
        [ThreadStatic] public static bool TableGeneralAndTableModeIsSame;

        /// <summary>
        /// Донор (ссылка на инстаграм профиль).
        /// </summary>
        [ThreadStatic] public static string InstUrl;

        /// <summary>
        /// Ссылка на zen канал.
        /// </summary>
        [ThreadStatic] public static string ZenChannel;

        /// <summary>
        /// Ссылка на zen канал.
        /// </summary>
        [ThreadStatic] public static string ZenChannelProfile;

        [ThreadStatic] public static string ChannelName;

        /// <summary>
        /// Логин от яндекс аккаунта.
        /// </summary>
        [ThreadStatic] public static string Login;

        /// <summary>
        /// Пароль от яндекс аккаунта.
        /// </summary>
        [ThreadStatic] public static string Password;

        /// <summary>
        /// Ответ на контрольный вопрос.
        /// </summary>
        [ThreadStatic] public static string Answer;

        /// <summary>
        /// Номер телефона.
        /// </summary>
        [ThreadStatic] public static string Phone;

        /// <summary>
        /// Прокси проекта.
        /// </summary>
        [ThreadStatic] public static string Proxy;

        /// <summary>
        /// Страна к которой принадлежит ip прокси (сокращенный вариант).
        /// </summary>
        [ThreadStatic] public static string CountryIp;

        /// <summary>
        /// Информация об текущем IP.
        /// </summary>
        [ThreadStatic] public static ProxyInfo IpInfo = new ProxyInfo();

        /// <summary>
        /// Домен который будет использоваться для работы yandex сервиса.
        /// </summary>
        [ThreadStatic] public static string Domain;

        /// <summary>
        /// Полная информация файла профиля (путь, размер и т.д.).
        /// </summary>
        [ThreadStatic] public static FileInfo ProfileInfo;

        /// <summary>
        /// Полная информация о папке аккаунта/донора (путь и прочее).
        /// </summary>
        [ThreadStatic] public static DirectoryInfo ResourceDirectory;

        /// <summary>
        /// Полная информация файла описания к аккаунту (путь, размер и т.д.).
        /// </summary>
        [ThreadStatic] public static FileInfo ChannelDescription;

        /// <summary>
        /// Полная информация файла аватара (путь, размер и т.д.).
        /// </summary>
        [ThreadStatic] public static FileInfo AvatarInfo;

        [ThreadStatic] public static bool ShorDonorNameForLog;
        [ThreadStatic] public static bool ProfileRetrievedFromSharedFolder;

        [ThreadStatic] public static int TimeToSecondsWaitPhone;
        [ThreadStatic] public static int MinutesWaitSmsCode;
        [ThreadStatic] public static int AttemptsReSendSmsCode;
        [ThreadStatic] public static bool BindingPhoneToAccountIfRequaid;

        [ThreadStatic] public static string DescriptionChannel;
        [ThreadStatic] public static ResourceType ResourceType;

        [ThreadStatic] public static int MinSizeProfileUseInModes;
        [ThreadStatic] public static bool CreateFolderResourceIfNotExist;



        /// <summary>
        /// Загрузка аватарки yandex (url: https://passport.yandex.{YandexDomain}/profile).
        /// </summary>
        public void UploadAvatarToYandexPassport()
        {
            var counterAttemptsUpload = 0;

            while (true)
            {
                try
                {
                    instance.ActiveTab.NavigateTimeout = 20;

                    if (++counterAttemptsUpload > 3)
                    {
                        Logger.Write($"Израсходован лимит попыток загрузки аватара", LoggerType.Warning);
                        return;
                    }

                    if (!Regex.IsMatch(instance.ActiveTab.URL, @"https://passport\.yandex\.[a-zA-Z]+/profile.*?$"))
                        instance.ActiveTab.Navigate($"https://passport.yandex.{Domain}/profile", instance.ActiveTab.URL, true);

                    var heAvatarCheck = instance.FuncGetFirstHe("//span[@class='avatar']", "Аватар", true, true, 7);

                    if (!Regex.IsMatch(heAvatarCheck.GetAttribute("style"), @"(?<=get-yapic/0/)0-0(?=/)"))
                    {
                        Logger.Write($"Аватар аккаунта yandex уже установлена", LoggerType.Info, true, false, true, LogColor.Blue);
                        return;
                    }

                    instance.FuncGetFirstHe("//div[contains(@class, 'personal')]/descendant::div[contains(@class, 'add-avatar')]").Click(instance.ActiveTab, rnd.Next(150, 500));
                    var heUpload = instance.FuncGetFirstHe("//span[contains(@id, 'load_avatar')]/descendant::input[contains(@name, 'attachment')]", "Загрузить");

                    instance.SetFilesForUpload(AvatarInfo, true);

                    heUpload.Click(instance.ActiveTab, rnd.Next(150, 500));

                    instance.FuncGetFirstHe("//span[contains(@class, 'Attach-Holder')]/descendant::label[text()!='']", "Загруженный аватар", true, true, 10);

                    instance.ActiveTab.NavigateTimeout = 90;

                    instance.FuncGetFirstHe("//div[contains(@class, 'avatar-buttons')]/descendant::span[contains(@id, 'save')]/button", "Сохранить изменения", true, true, 7).Click(instance.ActiveTab, 5000);

                    if (!Regex.IsMatch(instance.FuncGetFirstHe("//span[@class='avatar']", "Аватар").GetAttribute("style"), @"(?<=get-yapic/0/)0-0(?=/)"))
                    {
                        Logger.Write($"[Файл: {AvatarInfo.FullName}]\tАватар аккаунта yandex успешно установлен", LoggerType.Info, true, false, true, LogColor.Blue);
                        ProfileWorker.SaveProfile(true);

                        return;
                    }
                    else instance.ActiveTab.Refresh(TypeRefreshEnum.JavaScript);
                }
                catch
                {
                    instance.ActiveTab.Refresh(TypeRefreshEnum.JavaScript);
                    continue;
                }
            }
        }

        /// <summary>
        /// Определить страну по ip.
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="countryReturnedVariant"></param>
        /// <returns></returns>
        public ProxyInfo DetermineCountryByIp(string ip)
        {
            var ipInfo = new ProxyInfo();

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
                    ResponceType.BodyOnly, 20000, "", zenno.Profile.UserAgent, true, 5
                );

                ipInfo.CountryShortName = Regex.Replace(ZennoPoster.Parser.ParseByXpath(httpResponse, "//a[contains(@href, '/countries/')]", "href").First(), @"/countries/.*?", "");
                ipInfo.CountryFullName = ZennoPoster.Parser.ParseByXpath(httpResponse, "//a[contains(@href, '/countries/')]", "innerhtml").First();
            }
            catch (Exception ex)
            {
                Logger.Write($"[Exception message: {ex.Message}]\tУпало исключение во время определения IP страны", LoggerType.Warning, true, true, true, LogColor.Red);
                return null;
            }

            return ipInfo;
        }

        /// <summary>
        /// Проверка директории на существование (создать, если требуется).
        /// </summary>
        /// <returns>true - директория существует; false - директория отсутствует.</returns>
        public bool ResourceDirectoryExists()
        {
            if (!ResourceDirectory.Exists)
            {
                if (CreateFolderResourceIfNotExist)
                {
                    ResourceDirectory.Create();

                    Logger.Write($"[Папка: {ResourceDirectory.FullName}]\tПапка создана в автоматическом режиме. Заполните её всеми необходимыми данными", LoggerType.Info, true, false, true);

                    // Создание папки
                    var nameFolderWithPosts = zenno.Variables["cfgNameFolderArticles"].Value;

                    if (!string.IsNullOrWhiteSpace(nameFolderWithPosts))
                    {
                        var dirTemp = new DirectoryInfo(Path.Combine(ResourceDirectory.FullName, nameFolderWithPosts));
                        dirTemp.Create();
                        Logger.Write($"[Папке: {dirTemp.FullName}]\tАвтоматически создана папка для постов. Создайде в ней отдельную папку (с произвольным именем) с файлами поста (1 папка с файлами = 1 пост)", LoggerType.Info, true, false, true);
                    }

                    // Создание файла с описанием к аккаунту и канала
                    var channelDescriptionFileName = zenno.ExecuteMacro(zenno.Variables["cfgFileNameDescriptionChannel"].Value);

                    if (!string.IsNullOrWhiteSpace(channelDescriptionFileName))
                        ChannelDescription = new FileInfo(Path.Combine(ResourceDirectory.FullName, channelDescriptionFileName));

                    if (ChannelDescription != null)
                    {
                        File.CreateText(ChannelDescription.FullName);
                        Logger.Write($"[Папке: {ChannelDescription.FullName}]\tАвтоматически создан файл с описанием канала. Заполните его", LoggerType.Info, true, false, true);
                    }
                }
                else
                {
                    Logger.Write
                    (
                        $"[Папка: {ResourceDirectory.FullName}]\tОтсутствует папка. Создайте её и заполните всеми необходимыми данными (некоторые данные могут создаться автоматически, если выставлены соответствующие настройки)",
                        LoggerType.Info, false, true, true, LogColor.Yellow
                    );
                }

                return false;
            }
            else return true;
        }

        /// <summary>
        /// Проверка аккаунта или донора на пустоту и занятость другим потоком.
        /// </summary>
        /// <returns></returns>
        public bool ResourceIsAvailable(string accountOrDonor, int row)
        {
            if (string.IsNullOrWhiteSpace(accountOrDonor))
            {
                Logger.Write($"[Row: {row + 2}]\tОтсутствует ресурс", LoggerType.Warning, false, true, false, LogColor.Yellow);
                return false;
            }

            // Проверка на занятость другим потоком
            if (Program.ResourcesAllThreadsInWork.Any(x => accountOrDonor == x))
            {
                Logger.Write($"Ресурс используется другим потоком", LoggerType.Info, false, false, false);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Проверка наличия формы для разрешения использования куков и принятие условий.
        /// </summary>
        public void AcceptingPrivacyPolicyCookie()
        {
            instance.UseFullMouseEmulation = false;

            var xpathButtonCookieWhiteForm = new[] { "//a[contains(@href, 'privacy')]/../following-sibling::div[contains(@class, 'controls')]/descendant::button[contains(@class, 'type_ok')]", "Белая форма справа" };
            var xpathButtonCookieMulticoloredForm = new[] { "//td/descendant::table/descendant::button[contains(@data-text, 'Accept all')]", "Разноцветная форма внизу" };

            var heButtonCookieWhiteForm = instance.FuncGetFirstHe(xpathButtonCookieWhiteForm[0], "", false, false, 0);
            var heButtonCookieMulticoloredForm = instance.FuncGetFirstHe(xpathButtonCookieMulticoloredForm[0], "", false, false, 0);

            if (!heButtonCookieWhiteForm.IsNullOrVoid()) heButtonCookieWhiteForm.Click(instance.ActiveTab, rnd.Next(150, 500));
            if (!heButtonCookieMulticoloredForm.IsNullOrVoid()) heButtonCookieMulticoloredForm.Click(instance.ActiveTab, rnd.Next(150, 500));

            instance.UseFullMouseEmulation = true;
        }

        /// <summary>
        /// Проверяет прокси на правильность заполнения, а так же проверяет региональную принадлежность IP и установка соответствующих полей (яндекс домена и идентификатора страны для заказа номера).
        /// </summary>
        /// <param name="columnProxy"></param>
        /// <param name="rowProxy"></param>
        /// <param name="determineCountry"></param>
        /// <param name="logText"></param>
        /// <returns></returns>
        public bool SetProxy(int columnProxy, int rowProxy, bool determineCountry)
        {
            Proxy = AccountsTable.GetCell(columnProxy, rowProxy);

            if (!Proxy.Contains(":"))
            {
                Logger.Write($"[Row: {rowProxy + 2}]\tОтсутствует proxy", LoggerType.Warning, true, true, true, LogColor.Yellow);
                return false;
            }

            // Определение страны IP
            if (determineCountry)
            {
                IpInfo = DetermineCountryByIp(Proxy);

                if (IpInfo == null) return false;

                CountryIp = IpInfo.CountryShortName;

                switch (CountryIp)
                {
                    default:
                    case "ru":
                        Domain = "ru";
                        break;
                    case "us":
                        Domain = "com";
                        break;
                }
            }

            // Установка прокси в инстанс
            try
            {
                var additionalLog = IpInfo != null ? $" | Proxy country: {IpInfo.CountryShortName} — {IpInfo.CountryFullName}" : "";

                instance.SetProxy(Proxy, false, true, true, true);

                if (instance.ActiveTab.IsBusy)
                    instance.ActiveTab.WaitDownloading();

                Logger.Write($"[Proxy instance: {instance.GetProxy()}{additionalLog}]\tПрокси установлено в инстанс", LoggerType.Info, true, false, true);
            }
            catch (Exception ex)
            {
                Logger.Write($"[Row: {rowProxy}]\t[Exception message: {ex.Message}]\tУпало исключение во время установки прокси...", LoggerType.Warning, true, true, true, LogColor.Yellow);
            }

            return true;
        }

        /// <summary>
        /// Проверка и установка аватара в соответствующее поле.
        /// </summary>
        /// <returns></returns>
        public bool GetAvatar()
        {
            var images = ResourceDirectory.EnumerateFiles("*", SearchOption.TopDirectoryOnly).Where(x => Regex.IsMatch(Path.GetExtension(x.FullName), @"\.(jpg|jpeg|png)$"));

            if (images.Count() == 0)
            {
                Logger.Write($"[Папка: {ResourceDirectory.FullName}]\tНе найдено ни одной аватарки. Изображения должны находиться в папке донора и иметь формат: \"*.jpg\", \"*.jpeg\", \"*.png\"", LoggerType.Info, true, true, true, LogColor.Yellow);
                return false;
            }
            else AvatarInfo = images.First();

            return true;
        }
    }
}
