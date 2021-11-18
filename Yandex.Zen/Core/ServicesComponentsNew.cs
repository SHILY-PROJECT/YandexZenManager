using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Enums;
using Yandex.Zen.Core.Models.AccountOrDonorModels;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;
using ZennoLab.InterfacesLibrary.ProjectModel.Collections;

namespace Yandex.Zen.Core
{
    public class ServicesComponentsNew
    {
        public static AccountOrDonorBaseModel Object { get => DataStore.ResourceObject; } 
        public static IZennoPosterProjectModel Zenno { get => DataStore.Zenno; }
        public static Instance Instance { get => DataStore.Browser; }
        public static ProgramModeEnum ProgramMode { get => DataStore.ProgramMode; }
        public static ILocalVariables ZVars { get => DataStore.Zenno.Variables; }
        public static ILists ZLists { get => DataStore.Zenno.Lists; }
        public static Random Rnd { get; set; } = new Random();
    }
}
