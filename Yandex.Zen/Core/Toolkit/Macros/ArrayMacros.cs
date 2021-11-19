using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yandex.Zen.Core.Toolkit.Macros
{
    public class ArrayMacros
    {
        /// <summary>
        /// Проверка массива на пустые элементы, элементы состоящие из пробельных символов или null.
        /// </summary>
        public static bool ArrayAnyMatchIsNullOrWhiteSpace(params string[] array)
        {
            return array.Any(x => string.IsNullOrWhiteSpace(x));
        }
    }
}
