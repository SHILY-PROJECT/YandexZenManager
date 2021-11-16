using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Enums.Extensions;

namespace Yandex.Zen.Core.Tools.Extensions
{
    public static class ArrayExtensions
    {
        /// <summary>
        /// Проверка массива на пустые элементы, элементы состоящие из пробельных символов или null.
        /// </summary>
        public static bool AnyMatchIsNullOrWhiteSpace(this string[] array)
        {
            return array.Any(x => string.IsNullOrWhiteSpace(x));
        }

        public static bool AnyMatch(this string text, string[] arrayPattern, SearchTypeForAnyMatch searchTypeForAnyMatch = SearchTypeForAnyMatch.Contains)
        {
            switch (searchTypeForAnyMatch)
            {
                default:
                case SearchTypeForAnyMatch.Contains: return arrayPattern.Any(x => text.Contains(x));
                case SearchTypeForAnyMatch.EqualsOrdinal: return arrayPattern.Any(x => text.Equals(x, StringComparison.Ordinal));
            }
        }

    }
}
