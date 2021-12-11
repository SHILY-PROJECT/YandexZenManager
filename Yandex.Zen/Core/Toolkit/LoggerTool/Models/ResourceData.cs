using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Enums;

namespace Yandex.Zen.Core.Toolkit.LoggerTool.Models
{
    /// <summary>
    /// Класс для формирования данных о ресурсе в работе (автоматический).
    /// </summary>
    public class ResourceData
    {
        public ResourceTypeEnum Type { get; set; }
        public string Name { get; set; }
        public string Dir { get; set; }

        public ResourceData()
        {
            Name = Obsolete_ServicesDataAndComponents.Login;
            Type = Obsolete_ServicesDataAndComponents.ResourceType;
            Dir = Obsolete_ServicesDataAndComponents.ObjectDirectory.FullName;
        }
    }
}
