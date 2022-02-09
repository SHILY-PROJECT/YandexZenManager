using System;
using System.Linq;
using System.Threading;
using Global.ZennoExtensions;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace Yandex.Zen.Core.Toolkit.Extensions
{
    public static class ZennoListExtensions
    {
        private static readonly Random _rnd = new Random();

        /// <summary>
        /// Получить первую строку с переносом в конец списка.
        /// </summary>
        public static string GetFirstLineWithMoveToEnd(this IZennoList zennoList, bool useThreadsLock = false, bool throwNewException = false)
        {
            string value;

            if (zennoList.Count == 0)
            {
                if (throwNewException)
                    throw new Exception("List is empty");
                return null;
            }

            if (useThreadsLock) Monitor.Enter(FileSyncObjects.ListSyncer);
            {
                value = zennoList[0];
                zennoList.RemoveAt(0);
                zennoList.Add(value);
            }
            if (useThreadsLock) Monitor.Exit(FileSyncObjects.ListSyncer);

            return value;
        }

        /// <summary>
        /// Получить рандомную строку из списка.
        /// </summary>
        public static string GetRandLine(this IZennoList zennoList, bool throwNewException = false)
        {
            if (zennoList.Any() is false)
            {
                if (throwNewException)
                    throw new Exception("List is empty");
                return null;
            }

            return zennoList[_rnd.Next(zennoList.Count)];
        }

        /// <summary>
        /// Получить первую строку с удалением из списка.
        /// </summary>
        public static string GetFirstLineWithToRemoved(this IZennoList zennoList, bool useThreadsLock = false, bool throwNewException = false)
        {
            string value;

            if (zennoList.Any() is false)
            {
                if (throwNewException)
                    throw new Exception("List is empty");
                return null;
            }

            if (useThreadsLock) Monitor.Enter(FileSyncObjects.ListSyncer);
            {
                value = zennoList[0];
                zennoList.RemoveAt(0);
            }
            if (useThreadsLock) Monitor.Exit(FileSyncObjects.ListSyncer);

            return value;
        }

        /// <summary>
        /// Получить случайную строку с удалением из списка.
        /// </summary>
        /// <param name="zennoList">Зенно список (IZennoList).</param>
        /// <param name="throwException">Бросить исключение, если список пуст (true - да; false - нет; по умолчанию - false).</param>
        /// <returns>Возвращает случайную строку из списка (взятая строка удаляется из списка).</returns>
        public static string GetRandLineWithToRemoved(this IZennoList zennoList, bool useThreadsLock = false, bool throwException = false)
        {
            int index;
            string value;

            if (zennoList.Any() is false)
            {
                if (throwException)
                    throw new Exception("List is empty");
                return null;
            }

            if (useThreadsLock) Monitor.Enter(FileSyncObjects.ListSyncer);
            {
                index = _rnd.Next(zennoList.Count);
                value = zennoList[index];
                zennoList.RemoveAt(index);
            }
            if (useThreadsLock) Monitor.Exit(FileSyncObjects.ListSyncer);

            return value;
        }

        /// <summary>
        /// Добавить элемент в список используя SyncObjects.ListSyncer.
        /// </summary>
        public static void Add(this IZennoList zennoList, string value, bool useThreadsLock = false)
        {
            if (string.IsNullOrWhiteSpace(value)) return;

            if (useThreadsLock) Monitor.Enter(FileSyncObjects.ListSyncer);
            {
                zennoList.Add(value);
            }
            if (useThreadsLock) Monitor.Exit(FileSyncObjects.ListSyncer);
        }

        /// <summary>
        /// Удалить все строки из списка по заданному шаблону (не точное совпадение/удаляет все строки которые содержат заданный текст).
        /// </summary>
        public static void RemoveAllLinesContainsTrue(this IZennoList zennoList, string deletedValue, bool useThreadsLock = false)
        {
            if (useThreadsLock) Monitor.Enter(FileSyncObjects.ListSyncer);
            {
                for (var row = 0; row < zennoList.Count;)
                {
                    if (zennoList[row].Contains(deletedValue))
                    {
                        zennoList.RemoveAt(row);
                    }
                    else row++;
                }
            }
            if (useThreadsLock) Monitor.Exit(FileSyncObjects.ListSyncer);
        }

        /// <summary>
        /// Удалить все строки из списка по заданному шаблону (точное совпадение/регистр учитывается).
        /// </summary>
        /// <param name="zennoList">Зенно список из которого нужно удалять (IZennoList).</param>
        /// <param name="deletedValue">Шаблон по которому удалять.</param>
        public static void RemoveAllLinesEqualsOrdinalTrue(this IZennoList zennoList, string deletedValue, bool useThreadsLock = false)
        {
            if (useThreadsLock) Monitor.Enter(FileSyncObjects.ListSyncer);
            {
                for (var row = 0; row < zennoList.Count;)
                {
                    if (zennoList[row].Equals(deletedValue, StringComparison.Ordinal))
                    {
                        zennoList.RemoveAt(row);
                    }
                    else row++;
                }
            }
            if (useThreadsLock) Monitor.Exit(FileSyncObjects.ListSyncer);
        }
    }
}
