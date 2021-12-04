using System.Collections.Generic;
using Global.ZennoExtensions;
using System.Threading;
using ZennoLab.InterfacesLibrary.Enums.Log;
using Yandex.Zen.Core.Models.TableHandler;
using Yandex.Zen.Core.Enums;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;

namespace Yandex.Zen.Core.Toolkit
{
    /// <summary>
    /// Класс для работы с таблицей.
    /// </summary>
    public class TableHandler : ServicesDataAndComponents
    {
        /// <summary>
        /// Записать данные в ячейку таблицы режима и общую таблицу.
        /// </summary>
        /// <param name="searchByColumn"></param>
        /// <param name="searchByValue"></param>
        /// <param name="setToColumn"></param>
        /// <param name="setValue"></param>
        public static void WriteToCellInSharedAndMode(TableColumnEnum.Inst searchByColumn, string searchByValue, InstDataItem data)
        {
            var logModeTable = TableGeneralAndTableModeIsSame ? $"[Table mode and table shared is same: {TableGeneralAndTableModeIsSame}]\tРезультат сохранен в таблицу" : $"[Table mode and table shared is same: {TableGeneralAndTableModeIsSame}]\tРезультат сохранен в таблицу режима";

            // Сохранение результата в таблицу режима
            if (AccountsTable != null)
            {
                lock (SyncObjects.TableSyncer)
                {
                    var numbRow = AccountsTable.RowCount;

                    for (int i = 0; i < numbRow; i++)
                    {
                        if (AccountsTable.GetCell((int)searchByColumn, i) == searchByValue)
                        {
                            AccountsTable.SetCell((int)data.SetToColumn, i, data.SetValue);

                            if (!string.IsNullOrWhiteSpace(AccountsTable.GetCell((int)data.SetToColumn, i)))
                            {
                                Logger.Write($"[Row: {i + 2}]\t{logModeTable}", LoggerType.Info, true, false, true, LogColor.Blue);
                            }
                            else Logger.Write($"[Row: {i + 2}]\t[{data.SetToColumn}]\tНе удалось вписать данные в таблицу", LoggerType.Info, true, false, true, LogColor.Yellow);

                            break;
                        }
                    }
                }
            }

            // Сохранение результата в общую таблицу
            if (!TableGeneralAndTableModeIsSame)
            {
                Logger.Write($"[Общая таблица: {TableGeneralAccountFile.FullName.ToLower()}]\t[Таблица режима: {TableModeAccountFile.FullName.ToLower()}]", LoggerType.Info, true, false, false);

                lock (SyncObjects.TableSyncer)
                {
                    var numbRow = AccountsGeneralTable.RowCount;

                    for (int i = 0; i < numbRow; i++)
                    {
                        if (AccountsGeneralTable.GetCell((int)searchByColumn, i) == searchByValue)
                        {
                            AccountsGeneralTable.SetCell((int)data.SetToColumn, i, data.SetValue);

                            if (!string.IsNullOrWhiteSpace(AccountsGeneralTable.GetCell((int)data.SetToColumn, i)))
                            {
                                Logger.Write($"[Row: {i + 2}]\t[Table mode and table shared is same: {TableGeneralAndTableModeIsSame}]\tРезультат сохранен в общую таблицу", LoggerType.Info, true, false, true, LogColor.Blue);
                            }
                            else Logger.Write($"[Row: {i + 2}]\t[{data.SetToColumn}]\tНе удалось вписать данные в таблицу", LoggerType.Info, true, false, true, LogColor.Yellow);

                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Записать данные в ячейку таблицы режима и общую таблицу (сразу несколько ячеек).
        /// </summary>
        /// <param name="searchByColumn"></param>
        /// <param name="searchByValue"></param>
        /// <param name="setToColumn"></param>
        /// <param name="setValue"></param>
        public static void WriteToCellInSharedAndMode(TableColumnEnum.Inst searchByColumn, string searchByValue, List<InstDataItem> data)
        {
            var logModeTable = TableGeneralAndTableModeIsSame ? $"[Table mode and table shared is same: {TableGeneralAndTableModeIsSame}]\tРезультат сохранен в таблицу" : $"[Table mode and table shared is same: {TableGeneralAndTableModeIsSame}]\tРезультат сохранен в таблицу режима";

            // Сохранение результата в таблицу режима
            if (AccountsTable != null)
            {
                var goodLog = new List<string>();
                var badLog = new List<string>();

                lock (SyncObjects.TableSyncer)
                {
                    var numbRow = AccountsTable.RowCount;

                    for (int i = 0; i < numbRow; i++)
                    {
                        if (AccountsTable.GetCell((int)searchByColumn, i) == searchByValue)
                        {
                            // Запись данных
                            foreach (var d in data) AccountsTable.SetCell((int)d.SetToColumn, i, d.SetValue);

                            Thread.Sleep(5000);

                            // Проверка данных
                            foreach (var d in data)
                            {
                                if (!string.IsNullOrWhiteSpace(AccountsTable.GetCell((int)d.SetToColumn, i)))
                                {
                                    goodLog.Add($"[Row: {i + 2}]\t{logModeTable}");
                                }
                                else badLog.Add($"[Row: {i + 2}]\t[{d.SetToColumn}]\tНе удалось вписать данные в таблицу");
                            }

                            break;
                        }
                    }
                }

                // Логирование ошибок
                if (badLog.Count != 0)
                    foreach (var log in badLog)
                        Logger.Write(log, LoggerType.Info, true, false, true, LogColor.Yellow);

                // Логирование успешного результата
                if (goodLog.Count != 0)
                {
                    var logEnd = badLog.Count != 0 ? " (частичное сохранение данных)" : " (успешное сохранение)";

                    Logger.Write($"{goodLog[0]}{logEnd}", LoggerType.Info, true, false, true, LogColor.Blue);
                }
            }

            // Сохранение результата в общую таблицу
            if (!TableGeneralAndTableModeIsSame)
            {
                var goodLog = new List<string>();
                var badLog = new List<string>();

                Logger.Write($"[Общая таблица: {TableGeneralAccountFile.FullName.ToLower()}]\t[Таблица режима: {TableModeAccountFile.FullName.ToLower()}]", LoggerType.Info, true, false, false);

                lock (SyncObjects.TableSyncer)
                {
                    var numbRow = AccountsGeneralTable.RowCount;

                    for (int i = 0; i < numbRow; i++)
                    {
                        if (AccountsGeneralTable.GetCell((int)searchByColumn, i) == searchByValue)
                        {
                            // Запись данных
                            foreach (var d in data) AccountsGeneralTable.SetCell((int)d.SetToColumn, i, d.SetValue);

                            Thread.Sleep(5000);

                            // Проверка записанных данных данных
                            foreach (var d in data)
                            {
                                if (!string.IsNullOrWhiteSpace(AccountsGeneralTable.GetCell((int)d.SetToColumn, i)))
                                {
                                    goodLog.Add($"[Row: {i + 2}]\t[Table mode and table shared is same: {TableGeneralAndTableModeIsSame}]\tРезультат сохранен в общую таблицу");
                                }
                                else badLog.Add($"[Row: {i + 2}]\t[{d.SetToColumn}]\tНе удалось вписать данные в таблицу");
                            }

                            break;
                        }
                    }
                }

                // Логирование ошибок
                if (badLog.Count != 0)
                    foreach (var log in badLog)
                        Logger.Write(log, LoggerType.Info, true, false, true, LogColor.Yellow);

                // Логирование успешного результата
                if (goodLog.Count != 0)
                {
                    var logEnd = badLog.Count != 0 ? " (частичное сохранение данных)" : " (успешное сохранение)";

                    Logger.Write($"{goodLog[0]}{logEnd}", LoggerType.Info, true, false, true, LogColor.Blue);
                }
            }
        }

    }
}
