using System;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;
using Yandex.Zen.Core.Models.ResourceModels;
using Yandex.Zen.Core.Models;

namespace Yandex.Zen.Core
{
    /// <summary>
    /// Класс-посредник для передачи данных проекта.
    /// </summary>
    public class DataManager
    {
        [ThreadStatic] private static DataManager _data = new DataManager();
        public static DataManager Data { get => _data; }

        public ResourceBaseModel Resource { get => StateKeeper.Resource; }
        public IZennoPosterProjectModel Zenno { get => StateKeeper.Zenno; }
        public Instance Browser { get => StateKeeper.Browser; }
        public TableModel MainTable { get => StateKeeper.MainTable; }
        public TableModel ModeTable { get => StateKeeper.ModeTable; }
    }
}
