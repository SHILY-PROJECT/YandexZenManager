using System;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;
using Yandex.Zen.Core.Models;
using Yandex.Zen.Core.Toolkit.ResourceObject.Interfaces;

namespace Yandex.Zen.Core.Interfaces
{
    public interface IDataManager
    {
        Type ServiceType { get; }
        IService Service { get; set; }
        IResourceObject CurrentResourceObject { get; set; }
        IZennoPosterProjectModel Zenno { get; }
        Instance Browser { get; }
        TableData TableData { get; }

        void ConfigureProjectSettings(out bool configurationStatus);
        bool TryConfigureProjectSettings();
    }
}
