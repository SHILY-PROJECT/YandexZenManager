using Global.ZennoExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace Yandex.Zen.Core.Toolkit.Extensions
{
    public static class ZennoListExtensions
    {
        /// <summary>
        /// Получить первую строку с переносом в конец списка.
        /// </summary>
        public static string GetFirstLineWithMoveToEnd(this IZennoList zennoList, bool throwNewException = false)
        {
            if (zennoList.Count == 0)
            {
                if (throwNewException) throw new Exception("Список пуст");

                return string.Empty;
            }

            lock (SyncObjects.ListSyncer)
            {
                var str = zennoList[0];
                zennoList.RemoveAt(0);
                zennoList.Add(str);

                return str;
            }
        }

        /// <summary>
        /// Получить рандомную строку из списка.
        /// </summary>
        public static string GetRandLine(this IZennoList zennoList, bool throwNewException = false)
        {
            if (zennoList.Count == 0)
            {
                if (throwNewException) throw new Exception("Список пуст");

                return string.Empty;
            }

            return zennoList[new Random().Next(zennoList.Count)];
        }

        /// <summary>
        /// Получить первую строку с удалением из списка.
        /// </summary>
        public static string GetFirstLineWithToRemoved(this IZennoList zennoList, bool throwNewException = false)
        {
            if (zennoList.Count == 0)
            {
                if (throwNewException) throw new Exception("Список пуст");

                return string.Empty;
            }

            lock (SyncObjects.ListSyncer)
            {
                var str = zennoList[0];
                zennoList.RemoveAt(0);

                return str;
            }
        }

        /// <summary>
        /// Получить случайную строку с удалением из списка.
        /// </summary>
        /// <param name="zennoList">Зенно список (IZennoList).</param>
        /// <param name="throwNewException">Бросить исключение, если список пуст (true - да; false - нет; по умолчанию - false).</param>
        /// <returns>Возвращает случайную строку из списка (взятая строка удаляется из списка).</returns>
        public static string GetRandLineWithToRemoved(this IZennoList zennoList, bool throwNewException = false)
        {
            if (zennoList.Count == 0)
            {
                if (throwNewException) throw new Exception("Список пуст");

                return string.Empty;
            }
            lock (SyncObjects.ListSyncer)
            {
                var index = new Random().Next(zennoList.Count);
                var str = zennoList[index];
                zennoList.RemoveAt(index);

                return str;
            }
        }

        /// <summary>
        /// Добавить элемент в список используя SyncObjects.ListSyncer.
        /// </summary>
        public static void AddUseLock(this IZennoList zennoList, string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return;

            lock (SyncObjects.ListSyncer)
            {
                zennoList.Add(str);
            }
        }

        /// <summary>
        /// Удалить все строки из списка по заданному шаблону (не точное совпадение/удаляет все строки которые содержат заданный текст).
        /// </summary>
        public static void RemoveAllLinesContainsTrue(this IZennoList zennoList, string stringPattern)
        {
            lock (SyncObjects.ListSyncer)
            {
                for (int i = 0; i < zennoList.Count;)
                {
                    if (zennoList[i].Contains(stringPattern))
                    {
                        zennoList.RemoveAt(i);
                    }
                    else i++;
                }
            }
        }

        /// <summary>
        /// Удалить все строки из списка по заданному шаблону (точное совпадение/регистр учитывается).
        /// </summary>
        /// <param name="zennoList">Зенно список из которого нужно удалять (IZennoList).</param>
        /// <param name="stringPattern">Шаблон по которому удалять.</param>
        public static void RemoveAllLinesEqualsOrdinalTrue(this IZennoList zennoList, string stringPattern, bool useSyncObjectsListSyncer = true)
        {
            if (useSyncObjectsListSyncer)
            {
                lock (SyncObjects.ListSyncer)
                {
                    for (int i = 0; i < zennoList.Count;)
                    {
                        if (zennoList[i].Equals(stringPattern, StringComparison.Ordinal))
                        {
                            zennoList.RemoveAt(i);
                        }
                        else i++;
                    }
                }
            }
            else
            {
                for (int i = 0; i < zennoList.Count;)
                {
                    if (zennoList[i].Equals(stringPattern, StringComparison.Ordinal))
                    {
                        zennoList.RemoveAt(i);
                    }
                    else i++;
                }
            }
        }
    }
}
