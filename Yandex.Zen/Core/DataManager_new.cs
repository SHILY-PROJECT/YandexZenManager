using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Enums;
using Yandex.Zen.Core.Models;
using Yandex.Zen.Core.Models.ResourceModels;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace Yandex.Zen.Core
{
    public class DataManager_new
    {
        /// <summary>
        /// Объект зенно постера (project).
        /// </summary>
        public IZennoPosterProjectModel Zenno { get; set; }

        /// <summary>
        /// Объект зенно постера (браузер).
        /// </summary>
        public Instance Browser { get; set; }

        /// <summary>
        /// Объект типа аккаунта или донора с соответствующими данными.
        /// </summary>
        public ResourceBaseModel Resource { get; set; }

        /// <summary>
        /// Таблица текущего режима.
        /// </summary>
        public TableModel Table { get; set; }

        public DataManager_new(Instance instance, IZennoPosterProjectModel zenno)
        {
            Zenno = zenno;
            Browser = instance;
            Logger.ConfigureMain(this);
        }

        public void ConfigureProjectSettings(out bool status)
        {
            try
            {

                Program.CurrentMode = DictionariesAndLists.ProgramModes[Zenno.Variables["cfgScriptServices"].Value];
                status = true;
            }
            catch (Exception ex)
            {
                status = false;
            }
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
