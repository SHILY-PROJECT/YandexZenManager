using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Enums;
using Yandex.Zen.Core.Services.PostingSecondWindService;
using Yandex.Zen.Core.Services.PostingSecondWindService.Enums;
using Yandex.Zen.Core.Toolkit.BrowserCustomizer;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using Yandex.Zen.Core.Toolkit.PhoneServiceTool.Models;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace Yandex.Zen.Core.Models.AccountOrDonorModels
{
    /// <summary>
    /// Класс для хранения данных аккаунта.
    /// </summary>
    public class AccountOrDonorBaseModel
    {
        private static readonly object _locker = new object();
        private ProjectComponents Project { get => ProjectComponents.Project; }

        public string Login { get; set; }
        public string Password { get; set; }
        public string AnswerQuestion { get; set; }
        public string PhoneNumber { get; set; }
        public Uri Instagram { get; set; }

        public ObjectTypeEnum Type { get; set; }
        public ChannelDataModel ChannelData { get; set; }
        public DirectoryInfo Directory { get; set; }
        public ProfileDataModel Profile { get; set; }
        public ProxyDataModel ProxyData { get; set; }
        public SettingsAccountOrDonorFromZennoVariablesModel SettingsFromZennoVariables { get; set; }

        public AccountOrDonorBaseModel(SettingsAccountOrDonorFromZennoVariablesModel settingsFromZennoVariables, ProfileDataModel profileData)
        {
            ChannelData = new ChannelDataModel();
            Profile = profileData;
            SettingsFromZennoVariables = settingsFromZennoVariables;

            SetAccount(Project.ProgramMode);
        }

        private void SetAccount(ProgramModeEnum programMode)
        {
            var tb = Project.ModeTable;

            if (tb.Table.RowCount == 0)
                throw new Exception($"Таблица пуста: {tb.FileName}");

            lock (_locker)
            {
                for (int row = 0; row < tb.Table.RowCount; row++)
                {
                    try
                    {
                        switch (programMode)
                        {
                            case ProgramModeEnum.PostingSecondWind:
                                if (ConfigurePostingSecondWind(tb.Table, row)) return;
                                break;

                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Write(ex.Message, LoggerType.Warning, this.Directory.Exists, true, true, LogColor.Yellow);
                    }
                }
            }

            throw new Exception($"Отсутствуют свободные/подходящие аккаунты: {tb.FileName}");
        }

        private bool ConfigurePostingSecondWind(IZennoTable table, int row)
        {
            var colLogin = (int)TableColumnEnum.PostingSecondWind.Login;
            var colPassword = (int)TableColumnEnum.PostingSecondWind.Password;
            var colProxy = (int)TableColumnEnum.PostingSecondWind.Proxy;
            var colAnswerQuestion = (int)TableColumnEnum.PostingSecondWind.AnswerQuestion;
            var colAccountPhone = (int)TableColumnEnum.PostingSecondWind.AccountNumberPhone;
            var colChannelPhone = (int)TableColumnEnum.PostingSecondWind.ChannelNumberPhone;


            if (table.ParseValueFromCell(colLogin, row, out var result))
            {
                this.Login = result;
                this.Directory = new DirectoryInfo(Path.Combine(ProjectDataStore.AccountsDirectory.FullName, Login));

                if (PostingSecondWind.Settings.Mode == PostingSecondWindModeEnum.AuthorizationAndLinkPhone && !this.Directory.Exists) this.Directory.Create();
            }
            else throw new Exception($"[{nameof(colLogin)}:{result}]\tValue is void or null");


            if (table.ParseValueFromCell(colPassword, row, out result))
            {
                this.Password = result;
            }
            else throw new Exception($"[{nameof(colPassword)}:{result}]\tValue is void or null");


            if (table.ParseValueFromCell(colAnswerQuestion, row, out result))
            {
                this.AnswerQuestion = result;
            }
            else throw new Exception($"[{nameof(colAnswerQuestion)}:{result}]\tValue is void or null");


            if (table.ParseValueFromCell(colProxy, row, out result))
            {
                this.ProxyData = new ProxyDataModel(result, true);
            }
            else throw new Exception($"[{nameof(colProxy)}:{result}]\tValue is void or null");


            if (table.ParseValueFromCell(colAccountPhone, row, out result))
            {
                this.PhoneNumber = result;
            }
            else if (PostingSecondWind.Settings.Mode == PostingSecondWindModeEnum.Posting)
                throw new Exception($"[{nameof(colAccountPhone)}:{result}]\tValue is void or null");


            if (table.ParseValueFromCell(colChannelPhone, row, out result))
            {
                this.ChannelData.NumberPhone = result;
            }
            else if (PostingSecondWind.Settings.Mode == PostingSecondWindModeEnum.Posting)
                throw new Exception($"[{nameof(colChannelPhone)}:{result}]\tValue is void or null");


            return true;
        }


    }
}
