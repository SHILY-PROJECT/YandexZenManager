using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Enums;
using Yandex.Zen.Core.Models;
using Yandex.Zen.Core.Models.ResourceModels;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace Yandex.Zen.Core
{
    public class DataManager_new
    {
        /// <summary>
        /// Объект зенно постера (project).
        /// </summary>
        public IZennoPosterProjectModel Zenno { get; set; }

        /// <summary>
        /// Объект зенно постера (браузер).
        /// </summary>
        public Instance Browser { get; set; }

        /// <summary>
        /// Объект типа аккаунта или донора с соответствующими данными.
        /// </summary>
        public ResourceBaseModel Resource { get; set; }

        /// <summary>
        /// Таблица текущего режима.
        /// </summary>
        public TableModel Table { get; set; }
    }
}
