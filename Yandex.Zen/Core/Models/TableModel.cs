using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace Yandex.Zen.Core.Services.Models
{
    public class TableModel
    {
        public IZennoTable Table { get; private set; }
        public FileInfo File { get; private set; }

        public TableModel(IZennoTable table, string path)
        {
            Table = table;
            File = new FileInfo(path);
        }

        public TableModel(IZennoTable table, ILocalVariable variable) : this(table, variable.Value) { }
        
    }
}
