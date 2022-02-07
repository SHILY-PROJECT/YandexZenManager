using System;
using System.Collections.Generic;
using Yandex.Zen.Core.Enums;
using Yandex.Zen.Core.ServicesComponents.ResourceObject.Models;

namespace Yandex.Zen.Core.ServicesComponents.ResourceObject.Interfaces
{
    public interface IAccount : IResourceObject
    {
        IProfile Profile { get; set; }
        ChannelModel Channel { get; set; }
        string Login { get; set; }
        string Password { get; set; }
        string AnswerQuestion { get; set; }
        string PhoneNumber { get; set; }
        string CurrentMessageInTable { get; set; }
        string WebSite { get; set; }

        void GenerateNewPassword();
        void SaveToTable(params TableColumnsEnum[] tableColumns);
        void SaveToTable(List<Tuple<TableColumnsEnum, string>> values);
        void SaveToTable(TableColumnsEnum tableColumn, string value);
    }
}
