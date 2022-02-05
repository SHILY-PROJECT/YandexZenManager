using System;
using System.IO;
using Yandex.Zen.Core.Toolkit.ResourceObject.Models;
using Yandex.Zen.Core.Toolkit.ResourceObject.Interfaces;
using Yandex.Zen.Core.Toolkit.ResourceObject.Models;

namespace Yandex.Zen.Core.Toolkit.ResourceObject
{
    public class AccountModel : ObjectBase, IAccount
    {
        public AccountModel(DataManager manager) : base(manager)
        {

        }

        public IProfile Profile { get; set; }
        public DirectoryInfo Directory { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string AnswerQuestion { get; set; }
        public string PhoneNumber { get; set; }
        public string CurrentMessageInTable { get; set; }
        public Uri WebSite { get; set; }
        public ChannelModel Channel { get; set; }
        public TemplateSettingsModel Settings { get; set; }

        ///// <summary>
        ///// Генерация нового пароля (автоматически вставляется в свойство 'Password')
        ///// </summary>
        //public void GenerateNewPassword()
        //{
        //    Password = TextMacros.GenerateString(12, 16, "abcd");
        //    Logger.Write($"[{nameof(Password)}:{Password}]\tСгенерирован новый пароль (на момент записи в лог, этот пароль не установлен)", LoggerType.Info, true, false, false);
        //}

        ///// <summary>
        ///// Установка ресурса.
        ///// </summary>
        //public void SetObject(Type serviceType)
        //{
        //    var tb = DataManager.Table;

        //    if (tb.Instance.RowCount == 0)
        //        throw new Exception($"Таблица пуста: {tb.FileName}");

        //    lock (_locker)
        //    {
        //        for (int row = 0; row < tb.Instance.RowCount; row++)
        //        {
        //            try
        //            {
        //                switch (serviceType)
        //                {

        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                Logger.Write(ex.Message, LoggerType.Warning, Directory.Exists, true, true, LogColor.Yellow);
        //            }
        //        }
        //    }

        //    return;

        //    throw new Exception($"Отсутствуют свободные/подходящие аккаунты: {tb.FileName}");
        //}

        ///// <summary>
        ///// Сохранение значений в таблицу.
        ///// </summary>
        //public void SaveToTable(params TableColumnsEnum[] tableColumns)
        //{
        //    var columnMapper = ColumnValueMapper;
        //    var values = new List<Tuple<TableColumnsEnum, string>>();
        //    //var values = Enumerable.Range(0, tableColumns.Length).Select((value, index) => Tuple.Create(tableColumns[index], mapper[tableColumns[index]])).ToList();

        //    Array.ForEach(tableColumns, x => values.Add(Tuple.Create(x, columnMapper[x])));
        //    SaveToTable(values);
        //}

        ///// <summary>
        ///// Сохранение значений в таблицу.
        ///// </summary>
        //public void SaveToTable(List<Tuple<TableColumnsEnum, string>> values)
        //{
        //    var obj = DataManager.CurrentObject;
        //    var tableModel = DataManager.Table;
        //    var table = DataManager.Table.Instance;

        //    lock (_locker)
        //    {
        //        for (int row = 0; row < table.RowCount; row++)
        //        {
        //            if (table.GetRow(row).Any(x => x.Equals(obj.Login, StringComparison.OrdinalIgnoreCase)))
        //            {
        //                foreach (var value in values)
        //                {
        //                    var column = (int)value.Item1;

        //                    table.SetCell(column, row, value.Item2);
        //                    Thread.Sleep(300);

        //                    if (table.GetCell(column, row).Equals(value.Item2) is false)
        //                        Logger.Write($"[{row}|{column}|{value.Item1}]\t[Строка:{row + 2}]\tНе удалось сохранить значение", LoggerType.Warning, true, false, true, LogColor.Yellow);
        //                }
        //                return;
        //            }
        //        }
        //    }
        //}

        ///// <summary>
        ///// Сохранение значения в таблицу.
        ///// </summary>
        //public void SaveToTable(TableColumnsEnum tableColumn, string value)
        //    => SaveToTable(new List<Tuple<TableColumnsEnum, string>> { Tuple.Create(tableColumn, value) });

        //private Dictionary<TableColumnsEnum, string> ColumnValueMapper => new Dictionary<TableColumnsEnum, string>
        //{
        //    { TableColumnsEnum.Login,                   Login },
        //    { TableColumnsEnum.Password,                Password },
        //    { TableColumnsEnum.AnswerQuestion,          AnswerQuestion },
        //    { TableColumnsEnum.AccountNumberPhone,      PhoneNumber },
        //    { TableColumnsEnum.ChannelNumberPhone,      Channel.NumberPhone },
        //    { TableColumnsEnum.Proxy,                   ProxyData.Proxy },
        //    { TableColumnsEnum.ChannelUrl,              Channel.Url.AbsoluteUri },
        //    { TableColumnsEnum.IndexationAndBanStatus,  Channel.IndexationAndBanStatus },
        //    { TableColumnsEnum.ProfileAndIPCountry,     $"{DataManager.Zenno.Profile.Country}/{ProxyData.CountryFullName}" },
        //    { TableColumnsEnum.MessageFromSettings,     CurrentMessageInTable }
        //};

    }
}
