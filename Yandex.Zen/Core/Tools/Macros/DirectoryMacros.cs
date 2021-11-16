using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yandex.Zen.Core.Tools.Macros
{
    public class DirectoryMacros
    {
        public static void DirectoryCopy(string sourceDirectory, string targetDirectory)
        {
            var source = new DirectoryInfo(sourceDirectory);
            var target = new DirectoryInfo(targetDirectory);

            DirectoryCopy(source, target);
        }

        public static void DirectoryCopy(DirectoryInfo source, DirectoryInfo target)
        {
            // Если директория для копирования файлов не существует, то создаем ее
            if (Directory.Exists(target.FullName) == false) Directory.CreateDirectory(target.FullName);

            // Копируем все файлы в новую директорию
            foreach (var fi in source.GetFiles()) fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);

            // Копируем рекурсивно все поддиректории
            foreach (var diSourceSubDir in source.GetDirectories())
            {
                // Создаем новую поддиректорию в директории
                var nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                // Опять вызываем функцию копирования (рекурсия)
                DirectoryCopy(diSourceSubDir, nextTargetSubDir);
            }
        }
    }
}
