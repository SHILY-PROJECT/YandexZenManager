using Global.ZennoExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Enums.FileManager;
using Yandex.Zen.Core.Tools.Extensions;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace Yandex.Zen.Core.Tools
{
    public class FileManager
    {
        /// <summary>
        /// Проверить файлы и данные проекта.
        /// </summary>
        public static void CheckingData(IZennoPosterProjectModel zenno)
        {
            zenno.SendToLog($"Проверка файлов", LogType.Info, false, LogColor.Default);

            var lstAccounts = zenno.Lists["Accounts"];
            var lstProxy = zenno.Lists["ProxyList"];

            var modeAccount = zenno.Variables["cfgModeAccount"].Value;
            var modeProxy = zenno.Variables["cfgModeProxy"].Value;

            if (modeAccount.Contains("FromFile") && lstAccounts.Count == 0) throw new Exception("Файл с аккаунтами пуст");
            if (modeProxy.Contains("FromFile") && lstProxy.Count == 0) throw new Exception("Файл с Proxy пуст");
        }

        /// <summary>
        /// Получить строку с аккаунтом.
        /// </summary>
        public static string GetAccountLine(IZennoPosterProjectModel zenno)
        {
            zenno.SendToLog($"Получение аккаунта", LogType.Info, false, LogColor.Default);

            var lstAccounts = zenno.Lists["Accounts"];
            var lstAccountsError = zenno.Lists["Accounts - Unknown Error"];

            var account = string.Empty;

            lock (SyncObjects.ListSyncer)
            {
                while (true)
                {
                    if (lstAccounts.Count == 0) throw new Exception("Файл с аккаунтами пуст");

                    account = lstAccounts[0];
                    lstAccounts.RemoveAt(0);

                    if (account.AnyMatch(":", ";"))
                    {
                        lstAccounts.Add(account);
                        zenno.SendToLog($"Аккаунт успешно получен: {account}", LogType.Info, true, LogColor.Default);
                        break;
                    }
                    else if (!string.IsNullOrWhiteSpace(account))
                    {
                        lstAccountsError.Add(account);
                        zenno.SendToLog($"Аккаунт [{account}] перенесен в «acc - unknown error.txt»", LogType.Warning, true, LogColor.Yellow);
                    }
                }
            }
            return account;
        }

        /// <summary>
        /// Получить строку с прокси.
        /// </summary>
        public static string GetProxy(IZennoPosterProjectModel zenno)
        {
            zenno.SendToLog($"Получение прокси", LogType.Info, false, LogColor.Default);

            var lstProxy = zenno.Lists["ProxyList"];
            var proxy = zenno.Variables["PROXY"].Value;
            var useProfileProxy = bool.Parse(zenno.Variables["cfgUseProfileProxy"].Value);

            if (useProfileProxy && !string.IsNullOrWhiteSpace(proxy)) return proxy;

            if (Enum.TryParse(zenno.Variables["cfgModeProxy"].Value, out ProxyModeEnum modeProxy))
            {
                switch (modeProxy)
                {
                    default:
                    case ProxyModeEnum.NoProxy: return string.Empty;
                    case ProxyModeEnum.ZennoProxyWithDelete: return ZennoPoster.GetProxy("");
                    case ProxyModeEnum.ZennoProxyWithOutDelete: return ZennoPoster.GetProxyWithOutDelete("");
                    case ProxyModeEnum.FromFileRandLine: return lstProxy.GetRandLine();
                    case ProxyModeEnum.FromFileFirstLineWithMovingToEnd: return lstProxy.GetFirstLineWithMoveToEnd();
                }
            }
            else throw new Exception("Не удалось определить режим получения прокси");
        }
    }
}
