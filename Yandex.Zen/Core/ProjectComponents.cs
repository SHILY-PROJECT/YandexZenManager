using System;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;
using ZennoLab.InterfacesLibrary.ProjectModel.Collections;
using Yandex.Zen.Core.Enums;
using Yandex.Zen.Core.Models.AccountOrDonorModels;
using Yandex.Zen.Core.Services.Models;
using Yandex.Zen.Core.Toolkit;
using Yandex.Zen.Core.Toolkit.PhoneServiceTool;

namespace Yandex.Zen.Core
{
    public class ProjectComponents
    {
        [ThreadStatic] private static ProjectComponents _instance;
        public static ProjectComponents Project { get => _instance is null ? _instance = _instance = new ProjectComponents() : _instance; }

        public ProgramModeEnum ProgramMode { get => ProjectDataStore.ProgramMode; }
        public AccountOrDonorBaseModel ResourceObject { get => ProjectDataStore.ResourceObject; }
        public PhoneServiceNew PhoneServiceNew { get => ProjectDataStore.PhoneServiceNew; }
        public CaptchaServiceNew CaptchaService { get => ProjectDataStore.CaptchaService; }

        public IZennoPosterProjectModel Zenno { get => ProjectDataStore.Zenno; }
        public Instance Browser { get => ProjectDataStore.Browser; }

        public TableModel MainTable { get => ProjectDataStore.MainTable; }
        public TableModel ModeTable { get => ProjectDataStore.ModeTable; }
    }
}
