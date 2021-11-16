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
        public ProxyInfo ProxyInfo { get; set; }
        public ResourceType ResourceType { get; set; }
        public DataStatuses DataStatuses { get; set; }
    }

    /// <summary>
    /// Класс для хранения статуса данных аккаунта (true - данные есть, иначе - false).
    /// </summary>
    public class DataStatuses
    {
        public bool Login { get; set; }
        public bool Password { get; set; }
        public bool Proxy { get; set; }
        public bool InstUrl { get; set; }
        public bool AnswerQuestion { get; set; }
        public bool Phone { get; set; }
        public bool Channel { get; set; }
        public bool ChannelProfileEditor { get; set; }
        public bool ChannelDescription { get; set; }
        public bool ChannelImage { get; set; }
        public bool Dir { get; set; }
        public bool Profile { get; set; }
        public bool ProxyInfo { get; set; }
    }
}
