using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using ZennoLab.InterfacesLibrary.Enums.Log;
using Yandex.Zen.Core.Enums;
using Yandex.Zen.Core.Interfaces;
using Yandex.Zen.Core.Toolkit.Macros;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using Yandex.Zen.Core.ServicesComponents.ResourceObject.Models;
using Yandex.Zen.Core.ServicesComponents.ResourceObject.Interfaces;

namespace Yandex.Zen.Core.ServicesComponents.ResourceObject
{
    public class AccountModel : ResourceObjectBase, IAccount
    {
        private readonly object _locker = new object();

        public AccountModel(IDataManager manager) : base(manager)
        {

        }

        public IProfile Profile { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string AnswerQuestion { get; set; }
        public string PhoneNumber { get; set; }
        public string CurrentMessageInTable { get; set; }
        public string WebSite { get; set; }
        public ChannelModel Channel { get; set; } 

        public void GenerateNewPassword()
        {
            Password = TextMacros.GenerateString(12, 16, "abcd");
            Logger.Write($"[{nameof(Password)}:{Password}]\tСгенерирован новый пароль (на момент записи в лог, этот пароль не установлен).", LoggerType.Info, true, false, false);
        }

        /// <summary>
        /// Сохранение значений в таблицу.
        /// </summary>
        public void SaveToTable(params TableColumnsEnum[] tableColumns)
        {
            var map = ColumnValueMapper;

            //SaveToTable(Enumerable.Range(0, tableColumns.Length)
            //    .Select((value, index) => Tuple.Create(tableColumns[index], map[tableColumns[index]])).ToList());

            var values = new List<Tuple<TableColumnsEnum, string>>();
            Array.ForEach(tableColumns, col => values.Add(Tuple.Create(col, map[col])));
            SaveToTable(values);
        }

        /// <summary>
        /// Сохранение значений в таблицу.
        /// </summary>
        public void SaveToTable(List<Tuple<TableColumnsEnum, string>> values)
        {
            var obj = (IAccount)Manager.CurrentResourceObject;
            var tableModel = Manager.Table;
            var table = Manager.Table.Obj;

            Monitor.Enter(_locker);
            for (int row = 0; row < table.RowCount; row++)
            {
                if (table.GetRow(row).Any(x => x.Equals(obj.Login, StringComparison.OrdinalIgnoreCase)))
                {
                    foreach (var value in values)
                    {
                        var column = (int)value.Item1;

                        table.SetCell(column, row, value.Item2);
                        Thread.Sleep(300);

                        if (table.GetCell(column, row).Equals(value.Item2) is false)
                            Logger.Write($"[{row}|{column}|{value.Item1}]\t[Строка:{row + 2}]\tНе удалось сохранить значение",
                                LoggerType.Warning, true, false, true, LogColor.Yellow);
                    }
                    return;
                }
            }
            Monitor.Exit(_locker);
        }

        /// <summary>
        /// Сохранение значения в таблицу.
        /// </summary>
        public void SaveToTable(TableColumnsEnum tableColumn, string value)
            => SaveToTable(new List<Tuple<TableColumnsEnum, string>> { Tuple.Create(tableColumn, value) });

        private Dictionary<TableColumnsEnum, string> ColumnValueMapper => new Dictionary<TableColumnsEnum, string>
        {
            { TableColumnsEnum.Login,                   Login },
            { TableColumnsEnum.Password,                Password },
            { TableColumnsEnum.AnswerQuestion,          AnswerQuestion },
            { TableColumnsEnum.AccountNumberPhone,      PhoneNumber },
            { TableColumnsEnum.ChannelNumberPhone,      Channel.NumberPhone },
            { TableColumnsEnum.Proxy,                   Proxy.Proxy },
            { TableColumnsEnum.ChannelUrl,              Channel.Url },
            { TableColumnsEnum.IndexationAndBanStatus,  Channel.IndexationAndBanStatus },
            { TableColumnsEnum.ProfileAndIPCountry,     $"{Manager.Zenno.Profile.Country}/{Proxy.CountryFullName}" },
            { TableColumnsEnum.MessageFromSettings,     CurrentMessageInTable }
        };

    }
}
