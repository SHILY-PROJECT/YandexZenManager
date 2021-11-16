using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace Yandex.Zen.Core.Tools.Extensions
{
    public static class VariableExtensions
    {

        /// <summary>
        /// Извлечение числа из переменной (поддерживает рандом через разделители '-', ':', ';', ' ').
        /// </summary>
        public static int ExtractNumber(this ILocalVariable zennoVariable)
            => ExtractNumber(zennoVariable.Value);

        /// <summary>
        /// Извлечение числа из переменной (поддерживает рандом через разделители '-', ':', ';', ' ').
        /// </summary>
        public static int ExtractNumber(this string line)
        {
            var rnd = new Random();
            var separators = new[] { '-', ':', ';', ' ' };
            var result = 0;

            if (separators.Any(x => line.Contains(x)) && int.TryParse(line.Split(separators)[0], out int numberFrom) && int.TryParse(line.Split(separators)[1], out int numberTo))
            {
                result = numberFrom > numberTo ? rnd.Next(numberTo, numberFrom + 1) : rnd.Next(numberFrom, numberTo + 1);
            }
            else if (int.TryParse(line, out int number))
            {
                result = number;
            }

            return result;
        }

        /// <summary>
        /// Проверить совпадение.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="arrayPattern"></param>
        /// <returns></returns>
        public static bool AnyMatch(this string text, params string[] arrayPattern)
        {
            return arrayPattern.Any(x => text.Contains(x));
        }
    }
}
