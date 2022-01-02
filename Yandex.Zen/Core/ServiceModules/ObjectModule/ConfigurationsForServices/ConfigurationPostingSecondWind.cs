using System;
using System.IO;
using System.Linq;
using Yandex.Zen.Core.ServiceModules.ObjectModule.Interfaces;
using Yandex.Zen.Core.Toolkit.Extensions;
using Yandex.Zen.Core.Toolkit.TableTool.Enums;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace Yandex.Zen.Core.ServiceModules.ObjectModule.ConfigurationsForServices
{
    public class ConfigurationPostingSecondWind : ConfigurationBase, IConfigurationForService
    {
        public ConfigurationPostingSecondWind(ObjectBase obj) : base(obj) { }

        public bool TryConfigure(IZennoTable table, int row)
        {
            

            // логин
            if (table.ParseValueFromCell(ColLogin, row, out var result))
            {
                if (Program.CheckObjectInWork(result)) return false;

                Object.Login = result;
                Object.Directory = new DirectoryInfo(Path.Combine(DataKeeper.SharedDirectoryOfAccounts.FullName, Object.Login));

                if (Object.Directory.Exists is false) Object.Directory.Create();

                Object.ProfileData.SetProfile(new FileInfo(Path.Combine(Object.Directory.FullName, $"{Object.Login.Split('@').First()}.zpprofile")));
            }
            else throw new Exception($"'{nameof(ColLogin)}' - value is void or null");

            // пароль
            if (table.ParseValueFromCell(ColPassword, row, out result))
            {
                Object.Password = result;
            }
            else throw new Exception($"'{nameof(ColPassword)}' - value is void or null");

            // ответ на контрольный вопрос
            if (table.ParseValueFromCell(ColAnswerQuestion, row, out result))
            {
                Object.AnswerQuestion = result;
            }
            else throw new Exception($"'{nameof(ColAnswerQuestion)}' - value is void or null");

            // прокси
            if (table.ParseValueFromCell(ColProxy, row, out result))
            {
                Object.ProxyData = new ProxyDataModel(result, true);
            }
            else throw new Exception($"'{nameof(ColProxy)}' - value is void or null");

            // номер телефона аккаунта
            if (table.ParseValueFromCell(ColAccountPhone, row, out result))
            {
                Object.PhoneNumber = result;
            }

            // номер телефона канала
            if (table.ParseValueFromCell(ColChannelPhone, row, out result))
            {
                Object.Channel.NumberPhone = result;
            }


            Program.AddObjectToCache(Object.Login, true, true);

            return true;
        }
    }
}
