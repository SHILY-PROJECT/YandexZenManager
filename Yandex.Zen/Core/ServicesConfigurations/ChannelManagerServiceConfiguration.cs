using System;
using Yandex.Zen.Core.Interfaces;
using Yandex.Zen.Core.ServicesConfigurations.Base;
using System.Collections.Generic;

namespace Yandex.Zen.Core.ServicesConfigurations
{
    public sealed class ChannelManagerServiceConfiguration : BaseAccountConfiguration, IServiceConfiguration
    {
        public ChannelManagerServiceConfiguration(IDataManager manager) : base(manager)
        {
            
        }

        protected override void ServiceConfigure()
        {
            throw new NotImplementedException();
        }

        protected override void SetUpAccount(IEnumerable<string> columns)
        {
            throw new NotImplementedException();
        }

        //public ConfigurationPostingSecondWind(DataManager manager, ResourceObjectBase obj) : base(manager, obj) { }

        //public bool TryConfigure(int row)
        //{
        //    var table = DataManager.Table.Obj;

        //    // логин
        //    if (table.TryParseValueFromCell(ColLogin, row, out var result))
        //    {
        //        if (Program.CheckObjectInWork(result)) return false;

        //        Object.Login = result;
        //        Object.Directory = new DirectoryInfo(Path.Combine(Program.CommonAccountDirectory.FullName, Object.Login));

        //        if (Object.Directory.Exists is false) Object.Directory.Create();

        //        Object.ProfileData.SetProfile(new FileInfo(Path.Combine(Object.Directory.FullName, $"{Object.Login.Split('@').First()}.zpprofile")));
        //    }
        //    else throw new Exception($"'{nameof(ColLogin)}' - value is void or null");

        //    // пароль
        //    if (table.TryParseValueFromCell(ColPassword, row, out result))
        //    {
        //        Object.Password = result;
        //    }
        //    else throw new Exception($"'{nameof(ColPassword)}' - value is void or null");

        //    // ответ на контрольный вопрос
        //    if (table.TryParseValueFromCell(ColAnswerQuestion, row, out result))
        //    {
        //        Object.AnswerQuestion = result;
        //    }
        //    else throw new Exception($"'{nameof(ColAnswerQuestion)}' - value is void or null");

        //    // прокси
        //    if (table.TryParseValueFromCell(ColProxy, row, out result))
        //    {
        //        Object.ProxyData = new ProxyModel(DataManager, result, true);
        //    }
        //    else throw new Exception($"'{nameof(ColProxy)}' - value is void or null");

        //    // номер телефона аккаунта
        //    if (table.TryParseValueFromCell(ColAccountPhone, row, out result))
        //    {
        //        Object.PhoneNumber = result;
        //    }

        //    // номер телефона канала
        //    if (table.TryParseValueFromCell(ColChannelPhone, row, out result))
        //    {
        //        Object.Channel.NumberPhone = result;
        //    }


        //    Program.AddObjectToCache(Object.Login, true, true);

        //    return true;
        //}

    }
}
