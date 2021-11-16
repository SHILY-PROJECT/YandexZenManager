using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Enums;
using Yandex.Zen.Core.ServicesCommonComponents;

namespace Yandex.Zen.Core.Models.Logger
{
    /// <summary>
    /// Класс для формирования данных о ресурсе в работе (автоматический).
    /// </summary>
    public class ResourceData
    {
        public ResourceType Type { get; set; }
        public string Name { get; set; }
        public string Dir { get; set; }

        public ResourceData()
        {
            Name = ServiceComponents.Login;
            Type = ServiceComponents.ResourceType;
            Dir = ServiceComponents.ResourceDirectory.FullName;
        }
    }
}
