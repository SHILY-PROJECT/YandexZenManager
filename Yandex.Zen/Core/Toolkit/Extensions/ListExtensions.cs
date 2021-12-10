using Global.ZennoExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZennoLab.InterfacesLibrary.ProjectModel;
using Yandex.Zen.Core.Toolkit.Extensions.Enums;

namespace Yandex.Zen.Core.Toolkit.Extensions
{
    public static class ListExtensions
    {
        /// <summary>
        /// Получение элемента из списка.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="lineGetType"></param>
        /// <param name="throwExceptionIfListIsVoid"></param>
        /// <returns></returns>
        public static T GetLine<T>(this List<T> list, LineOptions lineGetType, bool throwExceptionIfListIsVoid = false)
        {
            if (list.Count == 0)
            {
                if (throwExceptionIfListIsVoid) throw new Exception("Список пуст");

                return default;
            }

            switch (lineGetType)
            {
                case LineOptions.Random: return list.GetRandomLine(new Random().Next(list.Count));
                case LineOptions.RandomWithRemoved: return list.GetRandomLineWithRemoved(new Random().Next(list.Count));
                case LineOptions.First: return list.GetFirstLine();
                case LineOptions.FirstWithRemoved: return list.GetFirstLineWithRemoved();
                case LineOptions.FirstWithMoveToEnd: return list.GetFirstLineWithMoveToEnd();
            }

            return default;
        }

        /// <summary>
        /// Получение первой строки (без удаления).
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private static T GetFirstLine<T>(this List<T> list) => list[0];

        /// <summary>
        /// Получение первой строки с удалением её из списка.
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private static T GetFirstLineWithRemoved<T>(this List<T> list)
        {
            var line = list[0];
            list.RemoveAt(0);
            return line;
        }

        /// <summary>
        /// Получение рандомной строки из списка (без удаления).
        /// </summary>
        private static T GetRandomLine<T>(this List<T> list, int index) => list[index];

        /// <summary>
        /// Получение рандомной строки с удалением её из списка.
        /// </summary>
        private static T GetRandomLineWithRemoved<T>(this List<T> list, int index)
        {
            var line = list[index];
            list.RemoveAt(index);
            return line;
        }

        /// <summary>
        /// Получить первую строку с переносом в конец списка.
        /// </summary>
        private static T GetFirstLineWithMoveToEnd<T>(this List<T> list)
        {
            var line = list[0];
            list.RemoveAt(0);
            list.Add(line);
            return line;
        }
    }
}
