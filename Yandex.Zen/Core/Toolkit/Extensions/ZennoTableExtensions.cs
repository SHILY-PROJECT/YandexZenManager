using System;
using System.Threading;
using ZennoLab.InterfacesLibrary.ProjectModel;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;

namespace Yandex.Zen.Core.Toolkit.Extensions
{
    public static class ZennoTableExtensions
    {
        private static readonly object _locker = new object();

        /// <summary>
        /// Записать значение в таблицу (в заданную ячейку).
        /// </summary>
        /// <param name="table">Таблица в которую записывать данные.</param>
        /// <param name="searchedValue">Значение по которому искать (перебор по row).</param>
        /// <param name="searchByColumn">Столбец по которому искать (column который перебирать).</param>
        /// <param name="setValue">Значение которое записать в ячейку.</param>
        /// <param name="setToColumn">Столбец в который записать.</param>
        public static void WriteToCell(this IZennoTable table, int searchByColumn, string searchedValue, int setToColumn, string setValue, bool UseThreadLocker = false)
        {
            if (UseThreadLocker) Monitor.Enter(_locker);

            for (var row = 0; row < table.RowCount; row++)
            {
                if (table.GetCell(searchByColumn, row).Equals(searchedValue, StringComparison.OrdinalIgnoreCase))
                {
                    table.SetCell(setToColumn, row, setValue);
                }
            }

            if (UseThreadLocker) Monitor.Exit(_locker);
        }

        /// <summary>
        /// Получить значение из таблицу.
        /// </summary>
        public static string GetCell(this IZennoTable table, int searchByColumn, string searchedValue, int getColumn, bool UseThreadLocker = false)
        {
            if (UseThreadLocker) Monitor.Enter(_locker);

            for (var row = 0; row < table.RowCount; row++)
            {
                if (table.GetCell(searchByColumn, row) is string r && r.Equals(searchedValue, StringComparison.OrdinalIgnoreCase))
                {
                    return r;
                }
            }

            if (UseThreadLocker) Monitor.Exit(_locker);

            return null;
        }

        /// <summary>
        /// Получить значение из таблицу.
        /// </summary>
        public static string GetCell(this IZennoTable table, int searchByColumn, string searchedValue, int getColumn, out int numbRow, bool UseThreadLocker = false)
        {
            numbRow = -1;

            if (UseThreadLocker) Monitor.Enter(_locker);

            for (var row = 0; row < table.RowCount; row++)
            {
                if (table.GetCell(searchByColumn, row).Equals(searchedValue, StringComparison.Ordinal))
                {
                    numbRow = row;
                    return table.GetCell(getColumn, row);
                }
            }

            if (UseThreadLocker) Monitor.Exit(_locker);

            return null;
        }

        /// <summary>
        /// Получение значения ячейки.
        /// </summary>
        public static bool TryParseValueFromCell(this IZennoTable table, int column, int row, out string result)
        {
            result = null;

            try
            {
                if (table.GetCell(column, row) is string r && !string.IsNullOrWhiteSpace(r))
                {
                    result = r;
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Write(ex.FormatException(), LoggerType.Warning, true, false, true);
            }

            return false;
        }
    }
}
