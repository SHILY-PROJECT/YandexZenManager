using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yandex.Zen.Core.Tools.Extensions
{
    public static class FileExtensions
    {
        private static object locker = new object();

        public static void DeleteFile(this FileInfo fileInfo)
        {
            if (!fileInfo.Exists) return;

            lock (locker)
            {
                try { File.Delete(fileInfo.FullName); } catch { }
            }

        }
    }
}
