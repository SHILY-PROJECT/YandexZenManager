using System;
using System.Linq;
using System.Threading;
using Yandex.Zen.Core.Enums;
using Yandex.Zen.Core.Toolkit.ObjectModule.Models;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using Yandex.Zen.Core.Toolkit.Macros;
using ZennoLab.InterfacesLibrary.Enums.Log;
using System.Collections.Generic;

namespace Yandex.Zen.Core.Toolkit.ObjectModule
{
    public class ObjectBase : ObjectBaseModel
    {
        private static readonly object _locker = new object();
        private DataManager DataManager { get; set; }

        public ObjectBase(DataManager manager)
        {
            DataManager = manager;
        }

        /// <summary>
        /// Сохранение профиля.
        /// </summary>
        public void SaveProfile()
            => ProfileData.SaveProfile();

        /// <summary>
        /// Загрузка профиля.
        /// </summary>
        /// <param name="createVariables"></param>
        public void Load(bool createVariables = true)
            => ProfileData.Load(createVariables);

        /// <summary>
        /// Генерация нового пароля (автоматически вставляется в свойство 'Password')
        /// </summary>
        public void GenerateNewPassword()
        {
            Password = TextMacros.GenerateString(12, 16, "abcd");
            Logger.Write($"[{nameof(Password)}:{Password}]\tСгенерирован новый пароль (на момент записи в лог, этот пароль не установлен)", LoggerType.Info, true, false, false);
        }

        /// <summary>
        /// Установка ресурса.
        /// </summary>
        public void SetObject(ProgramModeEnum mode)
        {
            var tb = DataManager.Table;

            if (tb.Instance.RowCount == 0)
                throw new Exception($"Таблица пуста: {tb.FileName}");

            lock (_locker)
            {
                for (int row = 0; row < tb.Instance.RowCount; row++)
                {
                    try
                    {
                        switch (mode)
                        {

                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Write(ex.Message, LoggerType.Warning, Directory.Exists, true, true, LogColor.Yellow);
                    }
                }
            }

            return;

            throw new Exception($"Отсутствуют свободные/подходящие аккаунты: {tb.FileName}");
        }

        /// <summary>
        /// Сохранение значений в таблицу.
        /// </summary>
        public void SaveToTable(params TableColumnsEnum[] tableColumns)
        {
            var mapper = ColumnValueMapper;
            var values = new List<Tuple<TableColumnsEnum, string>>();
            //var values = Enumerable.Range(0, tableColumns.Length).Select((value, index) => Tuple.Create(tableColumns[index], mapper[tableColumns[index]])).ToList();

            Array.ForEach(tableColumns, x => values.Add(Tuple.Create(x, mapper[x])));
            SaveToTable(values);
        }

        /// <summary>
        /// Сохранение значений в таблицу.
        /// </summary>
        public void SaveToTable(List<Tuple<TableColumnsEnum, string>> values)
        {
            var obj = DataManager.Object;
            var tableModel = DataManager.Table;
            var table = DataManager.Table.Instance;

            lock (_locker)
            {
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
                                Logger.Write($"[{row}|{column}|{value.Item1}]\t[Строка:{row + 2}]\tНе удалось сохранить значение", LoggerType.Warning, true, false, true, LogColor.Yellow);
                        }
                        return;
                    }
                }
            }
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
            { TableColumnsEnum.Proxy,                   ProxyData.Proxy },
            { TableColumnsEnum.ChannelUrl,              Channel.Url.AbsoluteUri },
            { TableColumnsEnum.IndexationAndBanStatus,  Channel.IndexationAndBanStatus },
            { TableColumnsEnum.ProfileAndIPCountry,     $"{DataManager.Zenno.Profile.Country}/{ProxyData.CountryFullName}" },
            { TableColumnsEnum.MessageFromSettings,     CurrentMessageInTable }
        };
    }
}
