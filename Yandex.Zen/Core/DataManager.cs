using System;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;
using Yandex.Zen.Core.Models.ResourceModels;
using Yandex.Zen.Core.Models;

namespace Yandex.Zen.Core
{
    [Obsolete]
    /// <summary>
    /// Класс-посредник для передачи данных проекта.
    /// </summary>
    public class DataManager
    {
        [ThreadStatic] private static DataManager _data = new DataManager();
        public static DataManager Data { get => _data; }

        public ResourceBaseModel Resource { get => DataKeeper.Resource; }
        public IZennoPosterProjectModel Zenno { get => DataKeeper.Zenno; }
        public Instance Browser { get => DataKeeper.Browser; }
        public TableModel MainTable { get => DataKeeper.MainTable; }
        public TableModel ModeTable { get => DataKeeper.ModeTable; }
    }
}
