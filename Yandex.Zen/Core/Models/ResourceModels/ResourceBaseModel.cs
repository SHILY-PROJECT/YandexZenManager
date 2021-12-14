using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Enums;
using Yandex.Zen.Core.Models.ResourceModels;
using Yandex.Zen.Core.Services.PublicationManagerSecondWindService.Enums;
using Yandex.Zen.Core.Toolkit;
using Yandex.Zen.Core.Toolkit.Extensions;
using Yandex.Zen.Core.Toolkit.LoggerTool;
using Yandex.Zen.Core.Toolkit.LoggerTool.Enums;
using Yandex.Zen.Core.Toolkit.SmsServiceTool;
using Yandex.Zen.Core.Toolkit.SmsServiceTool.Models;
using ZennoLab.InterfacesLibrary.Enums.Log;
using ZennoLab.InterfacesLibrary.ProjectModel;
using Yandex.Zen.Core.Services.PublicationManagerSecondWindService;
using Yandex.Zen.Core.Toolkit.Macros;

namespace Yandex.Zen.Core.Models.ResourceModels
{
    /// <summary>
    /// Класс для хранения данных аккаунта.
    /// </summary>
    public class ResourceBaseModel
    {
        private static readonly object _locker = new object();
        private DataManager Project { get => DataManager.Data; }

        public string Login { get; set; }
        public string Password { get; set; }
        public string AnswerQuestion { get; set; }
        public string PhoneNumber { get; set; }
        public Uri Instagram { get; set; }
        public ChannelDataModel ChannelData { get; set; } = new ChannelDataModel();

        public ResourceTypeEnum Type { get; set; }
        public DirectoryInfo Directory { get; set; }
        public ProfileDataModel ProfileData { get; set; }
        public ProxyDataModel ProxyData { get; set; }
        public CaptchaService CaptchaService { get; set; }
        public SmsService SmsService { get; set; }
        public ResourceSettingsModel ActionSettings  { get; set; }

        /// <summary>
        /// Сохранение профиля.
        /// </summary>
        public void SaveProfile()
            => ProfileData.SaveProfile();

        /// <summary>
        /// Загрузка профиля.
        /// </summary>
        /// <param name="createVariables"></param>
        public void Load(bool createVariables = true)
            => ProfileData.Load(createVariables);

        /// <summary>
        /// Генерация нового пароля (автоматически вставляется в свойство 'Password')
        /// </summary>
        public void GenerateNewPassword()
        {
            Password = TextMacros.GenerateString(12, 16, "abcd");
            Logger.Write($"[{nameof(Password)}:{Password}]\tСгенерирован новый пароль (на момент записи в лог, этот пароль не установлен)", LoggerType.Info, true, false, false);
        }

        /// <summary>
        /// Установка ресурса.
        /// </summary>
        public void SetResource()
        {
            var tb = Project.ModeTable;

            if (tb.Table.RowCount == 0)
                throw new Exception($"Таблица пуста: {tb.FileName}");

            lock (_locker) for (int row = 0; row < tb.Table.RowCount; row++)
            {
                try
                {
                    switch (Program.CurrentMode)
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

            throw new Exception($"Отсутствуют свободные/подходящие аккаунты: {tb.FileName}");
        }

        /// <summary>
        /// Получение и конфигурирование аккаунта.
        /// </summary>
        private bool ConfigurePostingSecondWind(IZennoTable table, int row)
        {
            var secondWindMode = MainPublicationManagerSecondWind.CurrentMode;

            var colLogin = (int)TableColumnEnum.PostingSecondWind.Login;
            var colPassword = (int)TableColumnEnum.PostingSecondWind.Password;
            var colProxy = (int)TableColumnEnum.PostingSecondWind.Proxy;
            var colAnswerQuestion = (int)TableColumnEnum.PostingSecondWind.AnswerQuestion;
            var colAccountPhone = (int)TableColumnEnum.PostingSecondWind.AccountNumberPhone;
            var colChannelPhone = (int)TableColumnEnum.PostingSecondWind.ChannelNumberPhone;

            // логин
            if (table.ParseValueFromCell(colLogin, row, out var result))
            {
                if (Program.CheckResourceInWork(Login)) return false;

                this.Login = result;
                this.Directory = new DirectoryInfo(Path.Combine(ProjectKeeper.SharedDirectoryOfAccounts.FullName, Login));

                if (secondWindMode == PublicationManagerSecondWindModeEnum.AuthAndBindingPhone && this.Directory.Exists is false)
                    this.Directory.Create();

                ProfileData.SetProfile(new FileInfo(Path.Combine(Directory.FullName, $"{Login.Split('@').First()}.zpprofile")));
            }
            else throw new Exception($"'{nameof(colLogin)}' - value is void or null");

            // пароль
            if (table.ParseValueFromCell(colPassword, row, out result))
            {
                this.Password = result;
            }
            else throw new Exception($"'{nameof(colPassword)}' - value is void or null");

            // ответ на контрольный вопрос
            if (table.ParseValueFromCell(colAnswerQuestion, row, out result))
            {
                this.AnswerQuestion = result;
            }
            else throw new Exception($"'{nameof(colAnswerQuestion)}' - value is void or null");

            // прокси
            if (table.ParseValueFromCell(colProxy, row, out result))
            {
                this.ProxyData = new ProxyDataModel(result, true);
            }
            else throw new Exception($"'{nameof(colProxy)}' - value is void or null");

            // номер телефона аккаунта
            if (table.ParseValueFromCell(colAccountPhone, row, out result))
            {
                this.PhoneNumber = result;
            }
            else if (secondWindMode == PublicationManagerSecondWindModeEnum.Posting)
                throw new Exception($"'{nameof(colAccountPhone)}' - value is void or null");

            // номер телефона канала
            if (table.ParseValueFromCell(colChannelPhone, row, out result))
            {
                this.ChannelData.NumberPhone = result;
            }
            else if (secondWindMode == PublicationManagerSecondWindModeEnum.Posting)
                throw new Exception($"'{nameof(colChannelPhone)}' - value is void or null");

            Program.AddResourceToCache(Login, true, true);
            
            return true;
        }


    }
}
