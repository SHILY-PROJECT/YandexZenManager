using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Enums;

namespace Yandex.Zen.Core.Models.ResourceModels
{
    /// <summary>
    /// Класс для хранения данных аккаунта.
    /// </summary>
    public class Resource
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public string Proxy { get; set; }
        public Uri Instagram { get; set; }
        public string AnswerQuestion { get; set; }
        public string PhoneNumber { get; set; }

        public DirectoryInfo Directory { get; set; }
        public FileInfo Profile { get; set; }
        public ProxyData ProxyData { get; set; }
        public ResourceType ResourceType { get; set; }
        public PropertiesState PropertiesState { get; set; }
    }
}
