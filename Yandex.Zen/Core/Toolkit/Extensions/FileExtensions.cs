using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Yandex.Zen.Core.Toolkit.Extensions
{
    public static class FileExtensions
    {
        private static readonly object locker = new object();

        public static void DeleteFile(this FileInfo fileInfo, bool useThreadLocker = true)
        {
            if (!fileInfo.Exists) return;
            
            if (useThreadLocker) Monitor.Enter(locker);
            {
                try
                {
                    File.Delete(fileInfo.FullName);
                }
                catch { }
            }
            if (useThreadLocker) Monitor.Exit(locker);
        }
    }
}
