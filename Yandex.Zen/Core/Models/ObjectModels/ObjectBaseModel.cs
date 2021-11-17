using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Enums;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace Yandex.Zen.Core.Models.ObjectModels
{
    /// <summary>
    /// Класс для хранения данных аккаунта.
    /// </summary>
    public class ObjectBaseModel
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public string AnswerQuestion { get; set; }
        public string PhoneNumber { get; set; }
        public Uri Instagram { get; set; }
        public DirectoryInfo Directory { get; set; }
        public ProfileModel Profile { get; set; }
        public ProxyDataModel ProxyData { get; set; }
        public ObjectTypeEnum Type { get; set; }
        public PropertiesStateModel PropertiesState { get; set; }

        private static readonly ServicesComponents Components;

        public ObjectBaseModel()
        {
            
        }
    }
}
