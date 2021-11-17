using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Enums;

namespace Yandex.Zen.Core.Tools.LoggerTool.Models
{
    /// <summary>
    /// Класс для формирования данных о ресурсе в работе (автоматический).
    /// </summary>
    public class ResourceData
    {
        public ObjectTypeEnum Type { get; set; }
        public string Name { get; set; }
        public string Dir { get; set; }

        public ResourceData()
        {
            Name = ServicesComponents.Login;
            Type = ServicesComponents.ResourceType;
            Dir = ServicesComponents.ObjectDirectory.FullName;
        }
    }
}
