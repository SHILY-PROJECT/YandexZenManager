using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace Yandex.Zen.Core.Services.Models
{
    public class Tables
    {
        public IZennoTable GeneralAccountsTable { get; set; }
        public IZennoTable ModeAccountTable { get; set; }
        public FileInfo GeneralAccountFile { get; set; }
        public FileInfo ModeAccountFile { get; set; }
        public static bool GeneralAndModeIsSame { get; set; }
    }
}
