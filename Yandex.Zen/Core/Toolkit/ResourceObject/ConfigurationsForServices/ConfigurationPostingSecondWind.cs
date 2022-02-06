using System;
using System.IO;
using System.Linq;
using Yandex.Zen.Core.Toolkit.ResourceObject.Interfaces;
using Yandex.Zen.Core.Toolkit.Extensions;
using Yandex.Zen.Core.Toolkit.ResourceObject.Models;
using Yandex.Zen.Core.Toolkit.ResourceObject.ConfigurationsForServices;
using Yandex.Zen.Core.Toolkit.ResourceObject;

namespace Yandex.Zen.Core.Toolkit.ResourceObject.ConfigurationsForServices
{
    public class ConfigurationPostingSecondWind : ConfigurationBase, IResourceObjectConfiguration
    {
        public ConfigurationPostingSecondWind(DataManager manager, ResourceObjectBase obj) : base(manager, obj) { }

        public bool TryConfigure(int row)
        {
            var table = DataManager.Table.Obj;

            // логин
            if (table.ParseValueFromCell(ColLogin, row, out var result))
            {
                if (Program.CheckObjectInWork(result)) return false;

                Object.Login = result;
                Object.Directory = new DirectoryInfo(Path.Combine(Program.CommonAccountDirectory.FullName, Object.Login));

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
                Object.ProxyData = new ProxyModel(DataManager, result, true);
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
