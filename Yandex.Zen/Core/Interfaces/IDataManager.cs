using System;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;
using Yandex.Zen.Core.Models;
using Yandex.Zen.Core.ServicesComponents.ResourceObject.Interfaces;

namespace Yandex.Zen.Core.Interfaces
{
    public interface IDataManager
    {
        Type CurrentServiceType { get; }
        IService Service { get; set; }
        IResourceObject CurrentResourceObject { get; set; }
        IZennoPosterProjectModel Zenno { get; }
        Instance Browser { get; }
        TableModel Table { get; }

        void ConfigureProjectSettings(out bool configurationStatus);
        bool TryConfigureProjectSettings();
    }
}
