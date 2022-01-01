using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZennoLab.InterfacesLibrary.ProjectModel;
using ZennoLab.InterfacesLibrary.ProjectModel.Collections;

namespace Yandex.Zen.Core.Models
{
    public class TableModel
    {
        /// <summary>
        /// Экземпляр таблицы.
        /// </summary>
        public IZennoTable Instance { get; private set; }

        /// <summary>
        /// Файл таблицы.
        /// </summary>
        public FileInfo File { get; private set; }

        /// <summary>
        /// Название таблицы.
        /// </summary>
        public string TableName { get; private set; }

        /// <summary>
        /// Название файла таблицы (должен быть указан File/path).
        /// </summary>
        public string FileName => File.Name;

        /// <summary>
        /// Название файла.
        /// </summary>
        public string Path => File.FullName;

        public TableModel(IZennoPosterProjectModel zenno, string nameTable, string path)
        {
            Instance = zenno.Tables[nameTable];
            TableName = nameTable;
            File = new FileInfo(zenno.ExecuteMacro(path));
        }

        public TableModel(IZennoPosterProjectModel zenno, string nameTable, ILocalVariable path)
            : this(zenno, nameTable, path.Value) { }

    }
}
