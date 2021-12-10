using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yandex.Zen.Core.Enums;
using Yandex.Zen.Core.Services.Models;
using Yandex.Zen.Core.Services.PostingSecondWindService.Enums;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace Yandex.Zen
{
    public class Dictionaries
    {
        private static IZennoPosterProjectModel Zenno { get => ProjectKeeper.Zenno; }

        public static Dictionary<string, ProgramModeEnum> ProgramModes = new Dictionary<string, ProgramModeEnum>()
        {
            ["Ручное управление аккаунтом в инстансе"] =        ProgramModeEnum.InstanceAccountManagement,
            ["Нагуливание профилей"] =                          ProgramModeEnum.WalkingProfile,
            ["Нагуливание аккаунтов/доноров по zen.yandex"] =   ProgramModeEnum.WalkingOnZen,
            ["Регистрация аккаунтов yandex"] =                  ProgramModeEnum.YandexAccountRegistration,
            ["Создание и оформление канала zen.yandex"] =       ProgramModeEnum.ZenChannelCreationAndDesign,
            ["Публикация статей на канале zen.yandex"] =        ProgramModeEnum.ZenArticlePublication,
            ["Накрутка активности"] =                           ProgramModeEnum.CheatActivity,
            ["Posting - second wind (new theme)"] =             ProgramModeEnum.PostingSecondWind
        };

        public static Dictionary<ProgramModeEnum, TableModel> ModeTables = new Dictionary<ProgramModeEnum, TableModel>
        {
            [ProgramModeEnum.InstanceAccountManagement] =   new TableModel("AccountsShared", Zenno.Variables["cfgPathFileAccounts"]),
            [ProgramModeEnum.YandexAccountRegistration] =   new TableModel("DonorsForRegistration", Zenno.Variables["cfgPathFileDonorsForRegistration"]),
            [ProgramModeEnum.ZenChannelCreationAndDesign] = new TableModel("AccountsForCreateZenChannel", Zenno.Variables["cfgPathFileAccountsForCreateZenChannel"]),
            [ProgramModeEnum.ZenArticlePublication] =       new TableModel("AccountsForPosting", Zenno.Variables["cfgPathFileAccountsForPosting"]),
            [ProgramModeEnum.CheatActivity] =               new TableModel("AccountsForCheatActivity", Zenno.Variables["cfgAccountsForCheatActivity"]),
            [ProgramModeEnum.PostingSecondWind] =           new TableModel("AccountsPostingSecondWind", Zenno.Variables["cfgPathFileAccountsPostingSecondWind"])
        };

        public static Dictionary<string, PostingSecondWindModeEnum> PostingSecondWindModes = new Dictionary<string, PostingSecondWindModeEnum>
        {
            ["Авторизация и привязка номера"] = PostingSecondWindModeEnum.AuthorizationAndLinkPhone,
            ["Постинг"] = PostingSecondWindModeEnum.Posting
        };

    }
}
