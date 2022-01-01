using System;
using Yandex.Zen.Core.Models;
using Yandex.Zen.Core.ServiceModules.ObjectModule;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace Yandex.Zen.Core
{
    public class DataManager_new
    {
        public IZennoPosterProjectModel Zenno { get; set; }
        public Instance Browser { get; set; }
        public ObjectBase Object { get; set; }
        public TableModel Table { get; set; }

        public DataManager_new(Instance instance, IZennoPosterProjectModel zenno)
        {
            Zenno = zenno;
            Browser = instance;
        }

        public bool TryConfigureProjectSettings()
        {
            ConfigureProjectSettings(out var configurationStatus);
            return configurationStatus;
        }

        public void ConfigureProjectSettings(out bool configurationStatus)
        {
            try
            {
                Program.CurrentMode = DictionariesAndLists.ProgramModes[Zenno.Variables["cfgTemplateMode"].Value];
                new Logger().Configure(this);
                this.Configure();
                configurationStatus = true;
            }
            catch (Exception ex)
            {
                Logger.Write(ex.FormatException(), LoggerType.Error, false, false, true, LogColor.Red);
                configurationStatus = false;
            }
        }

        private void Configure()
        {

        }

        /// <summary>
        /// Установка настроек браузера.
        /// </summary>
        private void SetBrowserSettings(string instanceWindowSize)
        {
            Browser.SetWindowSize
            (
                int.Parse(instanceWindowSize.Split('x')[0]),
                int.Parse(instanceWindowSize.Split('x')[1])
            );

            Browser.ClearCache();
            Browser.ClearCookie();

            Browser.IgnoreAdditionalRequests = false;
            Browser.IgnoreAjaxRequests = false;
            Browser.IgnoreFrameRequests = false;
            Browser.IgnoreFlashRequests = true;
        }
    }
}
