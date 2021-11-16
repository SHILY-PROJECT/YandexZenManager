using Global.ZennoExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace Yandex.Zen.Core.Tools.Extensions
{
    public static class ZennoTableExtensions
    {
        /// <summary>
        /// Записать значение в таблицу (в заданную ячейку).
        /// </summary>
        /// <param name="table">Таблица в которую записывать данные.</param>
        /// <param name="searchByValue">Значение по которому искать (перебор по row).</param>
        /// <param name="searchByColumn">Столбец по которому искать (column который перебирать).</param>
        /// <param name="setValue">Значение которое записать в ячейку.</param>
        /// <param name="setToColumn">Столбец в который записать.</param>
        public static void WriteToCell(this IZennoTable table, int searchByColumn, string searchByValue, int setToColumn, string setValue, bool useLockSyncObjectsTableSyncer = true)
        {
            if (useLockSyncObjectsTableSyncer)
            {
                lock (SyncObjects.TableSyncer)
                {
                    var numberAccounts = table.RowCount;

                    for (int i = 0; i < numberAccounts; i++)
                    {
                        if (table.GetCell(searchByColumn, i).Equals(searchByValue, StringComparison.Ordinal)) table.SetCell(setToColumn, i, setValue);
                    }
                }
            }
            else
            {
                var numberAccounts = table.RowCount;

                for (int i = 0; i < numberAccounts; i++)
                {
                    if (table.GetCell(searchByColumn, i).Equals(searchByValue, StringComparison.Ordinal)) table.SetCell(setToColumn, i, setValue);
                }
            }
        }

        /// <summary>
        /// Получить значение из таблицу.
        /// </summary>
        /// <param name="table">Таблица в которую записывать данные.</param>
        /// <param name="searchByValue">Значение по которому искать (перебор по row).</param>
        /// <param name="searchByColumn">Столбец по которому искать (column который перебирать).</param>
        /// <param name="getColumn">Столбец по которому получать ячейку.</param>
        public static string GetCell(this IZennoTable table, int searchByColumn, string searchByValue, int getColumn, bool useLockSyncObjectsTableSyncer = true)
        {
            if (useLockSyncObjectsTableSyncer)
            {
                lock (SyncObjects.TableSyncer)
                {
                    var numberAccounts = table.RowCount;

                    for (int i = 0; i < numberAccounts; i++)
                    {
                        if (table.GetCell(searchByColumn, i).Equals(searchByValue, StringComparison.Ordinal)) return table.GetCell(getColumn, i);
                    }
                }
            }
            else
            {
                var numberAccounts = table.RowCount;

                for (int i = 0; i < numberAccounts; i++)
                {
                    if (table.GetCell(searchByColumn, i).Equals(searchByValue, StringComparison.Ordinal)) return table.GetCell(getColumn, i);
                }
            }

            return null;
        }

        /// <summary>
        /// Получить значение из таблицу.
        /// </summary>
        /// <param name="table">Таблица в которую записывать данные.</param>
        /// <param name="searchByValue">Значение по которому искать (перебор по row).</param>
        /// <param name="searchByColumn">Столбец по которому искать (column который перебирать).</param>
        /// <param name="getColumn">Столбец по которому получать ячейку.</param>
        public static string GetCell(this IZennoTable table, int searchByColumn, string searchByValue, int getColumn, out int numbRow, bool useLockSyncObjectsTableSyncer = true)
        {
            if (useLockSyncObjectsTableSyncer)
            {
                lock (SyncObjects.TableSyncer)
                {
                    var numberAccounts = table.RowCount;

                    for (int i = 0; i < numberAccounts; i++)
                    {

                        if (table.GetCell(searchByColumn, i).Equals(searchByValue, StringComparison.Ordinal))
                        {
                            numbRow = i;
                            return table.GetCell(getColumn, i);
                        }
                    }
                }
            }
            else
            {
                var numberAccounts = table.RowCount;

                for (int i = 0; i < numberAccounts; i++)
                {

                    if (table.GetCell(searchByColumn, i).Equals(searchByValue, StringComparison.Ordinal))
                    {
                        numbRow = i;
                        return table.GetCell(getColumn, i);
                    }
                }
            }

            numbRow = -1;
            return null;
        }
    }
}
