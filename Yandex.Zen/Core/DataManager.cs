using System;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;
using Yandex.Zen.Core.Enums;
using Yandex.Zen.Core.Models.ResourceModels;
using Yandex.Zen.Core.Services.Models;

namespace Yandex.Zen.Core
{
    public class DataManager
    {
        [ThreadStatic] private static DataManager _data;
        public static DataManager Data { get => _data ?? (_data = _data = new DataManager()); }

        public ProgramModeEnum CurrentProgramMode { get => ProjectKeeper.CurrentProgramMode; }
        public ResourceBaseModel Resource { get => ProjectKeeper.Resource; }

        public IZennoPosterProjectModel Zenno { get => ProjectKeeper.Zenno; }
        public Instance Browser { get => ProjectKeeper.Browser; }

        public TableModel MainTable { get => ProjectKeeper.MainTable; }
        public TableModel ModeTable { get => ProjectKeeper.ModeTable; }
    }
}
