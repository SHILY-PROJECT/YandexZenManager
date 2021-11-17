using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Yandex.Zen.Core.Enums;
using Yandex.Zen.Core.Tools.LoggerTool;
using Yandex.Zen.Core.Tools.LoggerTool.Enums;
using ZennoLab.InterfacesLibrary.Enums.Log;

namespace Yandex.Zen.Core.Models.ZenArticlePublication
{
    public class Article : ServicesComponents
    {
        public bool IsVoid { get; private set; }
        public List<FileInfo> SimpleImagesList { get; set; }
        public List<FileInfo> NamedImagesList { get; set; }
        public DirectoryInfo ArticleDirectory { get; set; }
        public string TitleArticle { get; set; }
        public List<string> TextArticle { get; set; }

        public Article(DirectoryInfo dirArticle, string logText = "")
        {
            ArticleDirectory = dirArticle;

            // Проверяем наличие текстовых файлов в папке со статьей
            var textFilesArticle = dirArticle.EnumerateFiles("*.txt", SearchOption.TopDirectoryOnly);

            if (textFilesArticle.Count() == 0)
            {
                Logger.Write($"{logText}[Папка: {dirArticle.FullName}]\tВ папке отсутствует файл с текстом статьи", LoggerType.Warning, true, true, true, LogColor.Yellow);
                IsVoid = true;
                return;
            }

            // Проверяем наличие строк в самом файле
            var file = textFilesArticle.First();
            var fullText = File.ReadAllLines(file.FullName, Encoding.UTF8).ToList();

            if (fullText.Where(x => string.IsNullOrEmpty(x)).ToList().Count < 2)
            {
                Logger.Write($"{logText}[Файл: {file.FullName}]\tФайл заполнен не корректно (1-я строка - заголовок статьи; со 2-й строки - статья)", LoggerType.Warning, true, true, true, LogColor.Yellow);
                IsVoid = true;
                return;
            }
            else
            {
                TitleArticle = fullText[0];
                TextArticle = fullText.Skip(1).ToList();
            }

            // Получение имен из именованных макросов
            var listNamesNamedImages = Regex.Matches(string.Join("\n", fullText), @"(?<=\[IMAGE=)[\.a-zA-Z0-9]+?(?=])")
                .Cast<Match>()
                .Select(x => x.Value)
                .ToList();

            // Получение именованных изображений
            var namedImages = dirArticle
                .EnumerateFiles("*", SearchOption.TopDirectoryOnly)
                .Where(x => Regex.IsMatch(Path.GetExtension(x.FullName), @"\.(jpg|jpeg|png)$") && listNamesNamedImages.Contains(x.Name))
                .ToList();

            // Проверка наличия заданного количества изображений
            if (namedImages.Count < listNamesNamedImages.Count)
            {
                var macros = "[IMAGE=Имя файла.jpg]";
                Logger.Write($"{logText}[Папка: {dirArticle.FullName}]\t[MacrosType: {macros}]\tМакросов больше, чем самих изображений", LoggerType.Warning, true, true, true, LogColor.Yellow);
                IsVoid = true;
                return;
            }
            else NamedImagesList = namedImages;

            // Получение количества простых изображений
            var numbSimpleImages = Regex.Matches(string.Join("\n", fullText), @"\[IMAGE].*?")
                .Cast<Match>()
                .Select(x => x.Value)
                .ToList()
                .Count;

            // Получение простых изображений
            var simpleImages = dirArticle
                .EnumerateFiles("*", SearchOption.TopDirectoryOnly)
                .Where(x => Regex.IsMatch(Path.GetExtension(x.FullName), @"\.(jpg|jpeg|png)$") && !listNamesNamedImages.Contains(x.Name))
                .ToList();

            // Проверка наличия заданного количества изображений
            if (simpleImages.Count < numbSimpleImages)
            {
                var macros = "[IMAGE]";
                Logger.Write($"{logText}[Папка: {dirArticle.FullName}]\t[MacrosType: {macros}]\tМакросов больше, чем самих изображений", LoggerType.Warning, true, true, true, LogColor.Yellow);
                IsVoid = true;
                return;
            }
            else SimpleImagesList = simpleImages;

            IsVoid = false;
        }
    }
}