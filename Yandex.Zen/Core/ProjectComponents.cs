using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Enums;
using Yandex.Zen.Core.Models.AccountOrDonorModels;
using Yandex.Zen.Core.Toolkit;
using Yandex.Zen.Core.Toolkit.PhoneServiceTool;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;
using ZennoLab.InterfacesLibrary.ProjectModel.Collections;

namespace Yandex.Zen.Core
{
    public class ProjectComponents
    {
        public static ProgramModeEnum ProgramMode { get => DataStore.ProgramMode; }
        public static PhoneServiceNew PhoneServiceNew { get => DataStore.PhoneServiceNew; }
        public static CaptchaServiceNew CaptchaService { get => DataStore.CaptchaService; }
        public static AccountOrDonorBaseModel ResourceObject { get => DataStore.ResourceObject; } 
        public static IZennoPosterProjectModel Zenno { get => DataStore.Zenno; }
        public static Instance Browser { get => DataStore.Browser; }
        public static ILocalVariables ZVars { get => DataStore.Zenno.Variables; }
        public static ITables ZTables { get => DataStore.Zenno.Tables; }
        public static ILists ZLists { get => DataStore.Zenno.Lists; }
        public static Random Rnd { get; set; } = new Random();
    }
}
