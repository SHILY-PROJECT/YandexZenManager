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
        public ObjectTypeEnum Type { get; set; }
        public string Name { get; set; }
        public string Dir { get; set; }

        public ResourceData()
        {
            //Name = ServicesDataAndComponents_obsolete.Login;
            //Type = ServicesDataAndComponents_obsolete.ResourceType;
            //Dir = ServicesDataAndComponents_obsolete.ObjectDirectory.FullName;
        }
    }
}
