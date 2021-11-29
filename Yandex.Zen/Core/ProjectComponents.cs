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
        public static ProgramModeEnum ProgramMode { get => ProjectDataStore.ProgramMode; }
        public static AccountOrDonorBaseModel ResourceObject { get => ProjectDataStore.ResourceObject; }
        public static PhoneServiceNew PhoneServiceNew { get => ProjectDataStore.PhoneServiceNew; }
        public static CaptchaServiceNew CaptchaService { get => ProjectDataStore.CaptchaService; }


        public static IZennoPosterProjectModel Zenno { get => ProjectDataStore.Zenno; }
        public static Instance Browser { get => ProjectDataStore.Browser; }


        public static TableModel MainTable { get => ProjectDataStore.MainTable; }
        public static TableModel ModeTable { get => ProjectDataStore.ModeTable; }
    }
}
