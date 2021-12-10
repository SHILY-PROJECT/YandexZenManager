using System;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;
using Yandex.Zen.Core.Enums;
using Yandex.Zen.Core.Models.ResourceModels;
using Yandex.Zen.Core.Services.Models;

namespace Yandex.Zen.Core
{
    public class ProjectComponents
    {
        [ThreadStatic] private static ProjectComponents _instance;
        public static ProjectComponents Project { get => _instance ?? (_instance = _instance = new ProjectComponents()); }

        public ProgramModeEnum ProgramMode { get => ProjectSettingsDataStore.ProgramMode; }
        public ResourceBaseModel ResourceObject { get => ProjectSettingsDataStore.ResourceObject; }

        public IZennoPosterProjectModel Zenno { get => ProjectSettingsDataStore.Zenno; }
        public Instance Browser { get => ProjectSettingsDataStore.Browser; }

        public TableModel MainTable { get => ProjectSettingsDataStore.MainTable; }
        public TableModel ModeTable { get => ProjectSettingsDataStore.ModeTable; }
    }
}
